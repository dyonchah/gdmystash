# Spec: First usable local collection and character viewer

- **Status:** ready-for-agent
- **Date:** 2026-11-10
- **Feature slug:** `first-usable-app`
- **ADRs:** [0001](../adr/0001-phased-file-first-live-hook.md), [0002](../adr/0002-x64-only.md)

## Problem Statement

The app currently opens to an empty page. The server returns a plain-text root and a health endpoint, but there is no UI, no Collection, and no Character view. I cannot tell whether the app found my Grim Dawn saves, which Game Database it is using, or what items my Character has.

I want a local web app that reads my saves safely and shows my Collection and Characters, with search and item details, without risking a save file.

## Solution

A local web app served at `http://localhost:5044` that:

- detects and validates the Save Root and Game Folder;
- imports a read-only snapshot of every Character and the Transfer Stash into a local SQLite Collection;
- resolves Item Definitions, names, types, and base stats from the installed Game Database (`database.arz` plus expansion archives and text archives);
- presents a Dashboard, a searchable Collection, Character views, and Settings;
- never writes to the Save Root in this milestone.

Live looting and deposit through the hook are a later spec. This milestone deliberately proves the read-only data path and the UI before any save mutation exists.

## User Stories

1. As a Grim Dawn player, I want the app to open to a real UI, so that I know it is working and not just serving a placeholder string.
2. As a player, I want the app to detect my Save Roots, so that I do not have to find them manually.
3. As a player, I want to confirm or change the Save Root, so that the app works with my installation.
4. As a player, I want the app to find my Game Folder and Game Database, so that item names and stats resolve.
5. As a player, I want to see whether Grim Dawn is running, so that I know whether an import snapshot may be inconsistent.
6. As a player, I want a one-click read-only import, so that my Collection appears without editing any save.
7. As a player, I want imports to be repeatable and idempotent, so that items are not duplicated and favorites survive.
8. As a player, I want a timestamped backup on every import, so that I can recover if something goes wrong.
9. As a player, I want to see all my Characters, so that I know who has what.
10. As a player, I want to see a Character's equipment, so that I can review their build.
11. As a player, I want to see a Character's inventory, so that I can find items carried but not stashed.
12. As a player, I want to see a Character's personal stash, so that I can browse items stored per Character.
13. As a player, I want to see the Transfer Stash, so that I can browse account-wide items.
14. As a player, I want Softcore and Hardcore items separated into Vaults, so that I do not mix modes.
15. As a player, I want a Collection list of all owned items, so that I can see everything at once.
16. As a player, I want full-text search, so that I can find an item by name or affix.
17. As a player, I want to filter by slot, so that I can find a weapon or armor piece.
18. As a player, I want to filter by rarity, so that I can focus on epics and legendaries.
19. As a player, I want to filter by required level, so that I can find items for my current Character.
20. As a player, I want to filter by damage type, set, Monster Infrequent, and affix, so that I can find build-specific items.
21. As a player, I want to sort results, so that I can browse by name, level, or rarity.
22. As a player, I want to click an item and see its details, so that I can evaluate it.
23. As a player, I want to see exact base and affix stats, so that I can compare items accurately.
24. As a player, I want to see components, augments, relics, and set bonuses, so that I understand the full item.
25. As a player, I want to favorite items, so that I can keep track of good finds.
26. As a player, I want to filter favorites, so that I can review them quickly.
27. As a player, I want to save searches, so that I can reuse complex filters.
28. As a player, I want clear empty states, so that I know what to do when there is no data.
29. As a player, I want clear errors for missing, corrupt, or unsupported files, so that I can fix the problem.
30. As a player, I want unknown items to be marked clearly, so that I do not trust invented stats.
31. As a player, I want the app to remember my settings, so that I do not reconfigure it every launch.
32. As a player, I want the UI to work on a phone, so that I can browse from the couch.
33. As a player, I want a dark theme, so that it fits the game and is easy on the eyes.
34. As a player, I want the Collection to persist across restarts, so that imports are not lost.
35. As a player, I want the app to stay local, so that my saves and items never leave my PC.

## Implementation Decisions

### Stack and runtime

- Backend: C# / .NET 8, ASP.NET Core minimal API, `System.Text.Json`.
- Storage: SQLite via `Microsoft.Data.Sqlite`, with FTS5 for search. A small ordered migration runner, not EF Core.
- Frontend: React + TypeScript + Vite, plain CSS with design tokens, no component library. TanStack Query for server state and React Router for routes.
- The server serves the built UI from `wwwroot`. During development, Vite proxies `/api` to `http://localhost:5044`.
- The server binds to loopback only. LAN and phone access are a follow-up that needs an auth decision.
- Local data lives under `%LOCALAPPDATA%\GdMyStash\` (SQLite database, backups, logs). The repository stays separate from the game folder.

### Paths and first run

- The app needs two paths: the Save Root and the Game Folder.
- Save Root candidates include the standard local path, the OneDrive Documents path, Steam `userdata/<id>/219990/remote/save`, and the repack's Emulated Cloud Save path. The user can always browse to a folder manually.
- Game Folder candidates include common Steam library paths and a manual selection. The folder is valid only if it contains `database/database.arz`.
- Settings are persisted and validated on save. The app shows a first-run setup screen when either path is missing.
- The app detects a running `Grim Dawn` process and warns that a snapshot may be inconsistent. A read-only import is still allowed; the app copies files first and retries on checksum failure.

### Read-only snapshot import

- Import copies the relevant save files to a temporary snapshot before parsing. The Save Root is never written to.
- An import replaces the previous snapshot for each source (Character or Transfer Stash), rather than appending. This keeps the Collection a mirror of the saves and makes re-import idempotent.
- Item identity is content-based: a fingerprint over the Item Instance fields plus an occurrence index within its source. Favorites are keyed by this identity and survive re-import when the item is unchanged.
- Each import writes a timestamped backup under `%LOCALAPPDATA%\GdMyStash\backups\` and records an import run with counts, warnings, and duration.
- A failed parse never deletes the previous snapshot or the backup. The UI reports the failure and keeps the last good Collection.
- The shared Transfer Stash and the personal stash both feed the Collection. Equipment and inventory do not feed the Collection; they are Character state and are displayed in the Character view.

### Vaults

- A Vault is a separate Collection per game mode. Softcore (`transfer.gst`) and Hardcore (`transfer.gsh`) are separate Vaults.
- Characters are assigned to the Vault that matches their hardcore flag.
- All installed expansions share the same Vault; expansion data is part of the Game Database, not a separate Collection.

### Game Database

- Index the base `database.arz` and all installed expansion archives (`gdx1`, `gdx2`, `gdx3`).
- Index `Text_EN.arc` and the expansion text archives for display names and stat labels.
- Cache the index in SQLite keyed by an archive fingerprint; rebuild when the fingerprint changes.
- Resolve an Item Instance to an Item Definition by its base record, prefix, suffix, modifier, component, and augment records.
- Display names come from the record's name tag. If the tag is missing, fall back to the record path. Never invent a name.
- Type, slot, required level, and rarity are derived from the Item Definition. Unknown values are shown as `Unknown`.
- The index is read-only and never modifies the Game Folder.

### Item details and stats

- The item drawer shows the resolved name, type, slot, rarity, required level, affixes, component, augment, relic, ascended affix, set name, and location.
- Exact computed stats cover base item stats plus prefix and suffix stats, using the game's seed draw order. The MIT-licensed GDIA `StatTranslator` is the reference; `GrimDawnItemStats` is reference-only until its licensing is clarified.
- Components, augments, relics, ascended affixes, set bonuses, and procs are shown as text and record names in this milestone. Their exact rolled numbers and active set-bonus calculation are out of scope.
- Anything not computed is marked as such. The UI never displays a fabricated number.

### API and UI

- API errors use RFC 7807 ProblemDetails.
- The API exposes status, settings, path candidates, characters, character detail, import, collection, item detail, favorites, saved searches, backups, and import history.
- The UI has four routes: Dashboard, Collection, Characters, Settings. The Dashboard is the landing page.
- The Collection is search-first: a query bar, filter chips/sidebar, result count, and a virtualized or paginated list.
- The item drawer is reachable from the Collection and from Character grids.
- The UI is responsive down to phone width and uses a dark theme by default.
- Empty, loading, and error states are first-class, not an afterthought.

### Data model

- `settings` — key/value for Save Root, Game Folder, theme, and last import.
- `vaults` — mode (Softcore/Hardcore), display name.
- `characters` — name, level, class, hardcore, expansion, iron, attributes, source hash, imported timestamp.
- `items` — Item Instance fields, fingerprint, favorite flag, resolved name/type/slot/rarity/level, source.
- `item_locations` — Character equipment slot, inventory sack/slot, personal stash page/x/y, or Transfer Stash page/x/y.
- `items_fts` — FTS5 index over resolved name, affix names, type, and set name.
- `saved_searches` — name and query JSON.
- `backups` — path, timestamp, size, reason.
- `import_runs` — start/end, status, counts, warnings.
- `game_records` — cached Game Database index keyed by archive fingerprint.

## Testing Decisions

- The primary seam is the **HTTP API**. Tests use `WebApplicationFactory<Program>` against a temporary data directory and a temporary Save Root. They assert JSON contracts and user-visible behavior, not internal classes.
- The only new internal seam is **`IGameDatabase`**: API tests provide a small deterministic catalog of Item Definitions and text tags instead of shipping game assets. This keeps tests fast and license-clean.
- Save parsing is tested directly with synthetic `.gdc` / `.gst` fixtures generated by the existing writer. The existing `GdCodecTests`, `GdcInspectorTests`, and `GstInspectorTests` are prior art.
- The ARZ and ARC readers get a minimal hand-built archive fixture with one record and one text tag, so the LZ4/record-table path is covered without game files.
- Real game data and real saves are local-only integration tests, enabled by environment variables and skipped in CI. They are never committed.
- The UI is verified manually and through the API contract in this milestone. Browser automation is not added yet; it would be a new seam and is not needed to prove the read-only path.
- Acceptance is behavior-first: “open the app and see the detected Save Root”, “import and see four personal-stash items”, “search for a sword and filter by slot”, “open an item and see base and affix stats”.
- The seams are exactly two: the HTTP API and `IGameDatabase`. If a ticket needs a third seam, it must justify it in the ticket before implementation.

## Out of Scope

- Live Mode, the hook, and any write to save files.
- Loot, Transfer, Sweep, deposit, and the Inbox/Deposit tabs.
- Trading, multiplayer, accounts, and any network exposure beyond loopback.
- Build planner, DPS calculator, and character editing.
- Mods, custom games, the x86 client, Linux, and macOS.
- Exact component/augment/relic/ascended stat rolls, proc calculations, and active set-bonus calculation.
- Automatic import or file watching; import is a manual action.
- Localization. The UI is English and item names come from `Text_EN`.
- Restoring a backup; this milestone only creates and lists backups.

## Further Notes

- Current repository state: solution scaffold, GDC/GST codec, structural readers, CLI `inspect`, 13 passing tests, GitHub Actions build.
- The server currently returns a plain-text root and a health endpoint. This spec replaces that with the first real UI.
- Local manual test paths: Save Root `C:\Users\Public\Documents\Steam\RUNE\219990\remote\save`; Game Folder `C:\Users\office\gms\Grim Dawn (2016)\Grim Dawn`.
- ADR-0001 explains why this milestone is read-only and why live transfer is staged later. ADR-0002 explains the x64-only decision.
- The next spec after this one is the Live Mode hook: stash state, item replicas, named-pipe IPC, and safe loot/deposit.
