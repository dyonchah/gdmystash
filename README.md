# GD My Stash

A local-first, self-hosted item manager for **Grim Dawn**, built as a modern alternative to GD Item Assistant (GDIA).

> **Status:** early development. The design is frozen; the data layer is being built.

## Why another item manager?

GDIA solves the hard problem — infinite storage — but its workflow and UI get in the way. GD My Stash keeps the useful part (live loot and deposit while the game is running) and rebuilds everything around it: a local web UI, fast faceted search, full item tooltips, character awareness, and a safety model that never puts a save at risk.

## What it will do

- **Infinite collection** in a local SQLite database.
- **Search and filters:** slot, rarity, required level, damage type, set, Monster Infrequent, affix, and full text; favorites and saved searches.
- **Full tooltips:** exact base stats, affixes, components, and augments; set bonuses and procs as text; ascended items later.
- **Character view:** level, class, equipment, inventory, personal stash, skills, and devotion.
- **Live mode:** loot and deposit while the game is running, via an x64 hook (phase 3).
- **Offline mode:** sweep and deposit by editing save files only when the game is closed (phase 1).
- **Safety:** timestamped backups, byte-identical round-trip checks, rollback, and JSON export.

## Non-goals for v1

- Build planner or DPS calculator.
- Trading, multiplayer, or a hosted service.
- Linux and macOS.
- Mod support (separate vaults later).
- The 32-bit game client.

## Architecture

| Layer | Technology |
|---|---|
| Backend | C# / .NET 8, ASP.NET Core minimal API |
| Storage | SQLite + FTS5 |
| Frontend | React + TypeScript |
| Desktop shell | WebView2 + tray icon; the same UI works in a browser |
| Data layer | ARZ/ARC readers, GDC/GST codecs, item stat engine, character parser |
| Live hook | x64 C++ DLL (phase 3), adapted from GDIA's MIT-licensed HookDll |

The backend and the UI run on the same machine as the game. Nothing leaves the PC.

See [docs/architecture.md](docs/architecture.md) for the full picture and [docs/roadmap.md](docs/roadmap.md) for the plan.

## Repository layout

```
src/
  GdMyStash.Core/       Domain model and services
  GdMyStash.Data/       Game data and save parsers
  GdMyStash.Server/     Local HTTP server and desktop host
  GdMyStash.Hook/       x64 hook (phase 3)
web/                    React UI
tests/
  GdMyStash.Tests/      Unit and fixture tests
docs/
  adr/                  Architecture decision records
```

## Safety principles

1. The game's save files are the source of truth.
2. No write without a timestamped backup and a successful parse → encode byte-identical check.
3. Offline writes happen only when the game process is not running.
4. Live writes happen only through the hook, and only when the stash state is known safe.
5. Real Steam Cloud saves are detected and blocked; the repack's local `remote` folder is treated as a local save root.

## Development

Requirements: .NET 8 SDK, Node.js 20+, Git.

```powershell
dotnet build
npm --prefix web install
npm --prefix web run dev
```

Commands will be filled in as the projects are scaffolded.

## License

TBD. Third-party components and their licenses are listed in [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).

## Credits

- [GD Item Assistant](https://github.com/marius00/iagd) — MIT; the reference for live transfer and a source of parser code.
- [grim-dawn-save-editor](https://github.com/kraken444444/grim-dawn-save-editor) — MIT; clean reference for the GDC/GST codec.
- [gd-edit](https://github.com/GrimDawn-max/gd-edit) — EPL-1.0; format reference only, no code copied.
- [GrimDawnItemStats](https://github.com/marius00/GrimDawnItemStats) — no license file found; reference only until clarified.
- Grim Dawn and its assets are property of Crate Entertainment. This project does not distribute game assets.
