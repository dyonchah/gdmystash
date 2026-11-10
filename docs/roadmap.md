# Roadmap

## Phase 0 — Foundations (current)

- [x] Design interviews and decisions.
- [x] Repository, glossary, ADRs.
- [x] .NET solution and project scaffolding.
- [x] CI (build + tests).

## Phase 1 — Data layer

- [x] GDC/GST XOR codec with round-trip tests.
- [x] Structural GDC reader: header, block checksums, nested inventory and stash blocks.
- [x] Structural GST reader: header, nested pages, checksums.
- [x] CLI: `gdmystash inspect <file>`.
- [ ] Full item reader: materialize records and coordinates.
- [ ] GDC blocks 8 (skills), 13 (factions), 14–16 (UI/stats).
- [ ] ARZ reader: header, record table, string table, LZ4 payload.
- [ ] ARC reader: file table, parts, LZ4.
- [ ] Item database index (records, templates, text tags).
- [ ] Item stat engine: base + prefix + suffix.
- [ ] Extend stats: components, augments, relics.
- [ ] Local fixture suite from real saves (not committed).
- [ ] CLI: `gdmystash dump-db`.

## Phase 2 — Server and UI

- [ ] SQLite schema + migrations + FTS5.
- [ ] Collection import and sweep.
- [ ] Search, facets, favorites, saved searches.
- [ ] Item tooltip.
- [ ] Character view.
- [ ] Backups, rollback, JSON export.
- [ ] Local web UI + WebView2 shell + tray.

## Phase 3 — Live mode

- [ ] Hook spike: attach to x64 game, read stash state.
- [ ] IPC protocol and framing.
- [ ] Live loot.
- [ ] Live deposit with safety checks.
- [ ] Version detection and fail-closed behaviour.

## Phase 4 — Packaging and polish

- [ ] Installer / self-contained build.
- [ ] Autostart and update check.
- [ ] Mod vaults.
- [ ] Performance pass on large collections.
- [ ] Documentation and screenshots.

## Later

- Build planner integration.
- Trading between trusted users.
- Linux via Proton (out of scope for v1).
