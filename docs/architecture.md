# Architecture

## Context

GD My Stash is a single-user, local application. It runs on the same Windows PC as Grim Dawn (x64), stores everything locally, and serves a web UI on localhost. The game may be running at the same time.

The hard part is not search. It is moving items between the game and the collection without corrupting saves or duplicating items.

## Constraints

- Grim Dawn saves are binary files with block checksums and a rolling XOR stream. Every block must round-trip byte-identically.
- GDIA's live transfer does not edit `transfer.gst`. It injects an x64 DLL and intercepts `GAME::InventorySack::AddItem`; the add is suppressed and the item is handed to the app. Deposit inserts the item into the second-to-last stash tab.
- File-level edits are only safe while the game is closed. That is the GD Stash model.
- The game has no anti-cheat. Save editing is a normal single-player activity, but corruption is a real risk and must be engineered away.
- v1 targets the x64 client only.

## Components

| Component | Responsibility |
|---|---|
| `GdMyStash.Data` | ARZ/ARC readers, GDC/GST codecs, item database index, item stat engine, character parser. No UI, no HTTP. |
| `GdMyStash.Core` | Domain model, collection service, search, transfer orchestration, backups, safety invariants. |
| `GdMyStash.Server` | ASP.NET Core minimal API, static UI hosting, tray host, WebView2 shell, lifecycle. |
| `GdMyStash.Hook` | x64 C++ DLL injected into the game (phase 3). Intercepts item adds, reports stash state, inserts deposits. Talks to the server over a named pipe. |
| `web` | React + TypeScript UI. |

## Data flow

### Offline sweep

1. The user closes the game.
2. The server verifies that no `Grim Dawn` process is running.
3. The save root is discovered and locked for the operation.
4. `transfer.gst` / `player.gdc` are copied to a timestamped backup.
5. The files are parsed and the user selects what to import.
6. Selected items are written to the collection.
7. If the stash must be modified, the app writes a new file to a temp path, verifies parse → encode byte equality, then atomically replaces the original.

### Offline deposit

Same as sweep, but items are removed from the collection and written into the deposit tab of `transfer.gst`. The game must be closed. The app verifies the result and keeps the backup.

### Live loot (phase 3)

1. The hook is attached to the running x64 game.
2. The player moves an item into the inbox tab.
3. The hook intercepts `InventorySack::AddItem` and suppresses the add.
4. The hook serializes the item replica and sends it over IPC.
5. The server stores the item in the collection and acknowledges.
6. If the acknowledgement fails, the hook forwards the original add so the item is not lost.

### Live deposit (phase 3)

1. The user selects items and clicks Transfer.
2. The server sends item replicas to the hook.
3. The hook inserts them into the deposit tab, but only when the stash is open and the player is at the smuggler.
4. The server removes the items from the collection only after the hook confirms insertion.

## Storage

SQLite is the system of record. FTS5 provides full-text search.

Outline:

- `vaults(id, mode, dataset, name, created_at)`
- `items(id, vault_id, basename, prefix, suffix, modifier, transmute, seed, relic_name, relic_bonus, relic_seed, augment_name, augment_seed, ascended_name, ascended_bonus, stack_count, seed_reroll_count, affix_reroll_count, favorite, acquired_at, source, stats_json)`
- `characters(id, vault_id, name, level, class, hardcore, expansion, last_seen_at, raw_hash)`
- `character_items(character_id, slot, item_id)`
- `stash_tabs(vault_id, kind, page, width, height)`
- `saved_searches(id, name, query_json, favorite)`
- `backups(id, path, created_at, reason)`
- `settings(key, value)`
- `items_fts` — FTS5 index over item names, affixes, and tooltip text.

The schema will evolve; migrations are versioned.

## Safety invariants

1. **Backup first.** Every write starts with a timestamped copy of the file being changed.
2. **Round-trip gate.** A file is written only if parse → encode produces the exact original bytes before the intended edit.
3. **Atomic replace.** Write to a temp file, flush, then rename over the original.
4. **Game process check.** Offline writes are refused while the game is running.
5. **Cloud check.** Real Steam Cloud is detected and blocked. The repack's local `remote` folder is treated as a local save root.
6. **Verify after write.** Re-read the file, parse it, and confirm the expected change. Keep the backup until the next successful read.
7. **Fail closed.** Unknown block versions, unknown item records, or a hook version mismatch result in refusal, not a best-effort write.
8. **No invented stats.** Unknown items are stored with their raw record names and marked as unknown; the UI never displays fabricated numbers.

## Failure modes

| Failure | Behaviour |
|---|---|
| Game patched, block version unknown | Read-only mode; writes disabled; clear message. |
| Hook version mismatch | Hook refuses to attach; offline mode remains available. |
| Save file locked | Retry, then report; never force. |
| Item record missing from the local database | Store raw record names; show "Unknown item". |
| Crash during write | Temp file is discarded; original and backup remain. |
| Real Steam Cloud detected | Writes blocked; instructions to switch to local saves. |

## Phases

See [roadmap.md](roadmap.md).

## Open questions

- Final IPC protocol between the server and the hook.
- Whether to adapt GDIA's HookDll directly or write a new hook using its signatures as a reference.
- Coverage of the stat engine for components, augments, relics, set bonuses, and ascended items.
- Reliable detection of real Steam Cloud vs the repack's emulated local `remote` folder.
