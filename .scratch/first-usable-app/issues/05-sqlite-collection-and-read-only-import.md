# 05: SQLite collection and read-only snapshot import

**What to build:** Give the app a persistent Collection. A manual import copies the active saves to a timestamped backup, parses every Character and the Transfer Stash, and writes a snapshot into SQLite. The Collection page lists every item with its resolved name and source location. Re-importing replaces the previous snapshot instead of duplicating items, and nothing in the Save Root is modified. Softcore and Hardcore are separate Vaults.

**Blocked by:** 03, 04

**Status:** ready-for-agent

- [ ] A manual import copies the relevant save files to a timestamped backup, parses all Characters and the Transfer Stash, and returns counts plus warnings.
- [ ] The import writes Characters, Items, and item locations into SQLite; the Collection page lists items with resolved names, type, and source.
- [ ] Re-importing the same saves produces no duplicate items and preserves the identity used by favorites.
- [ ] The Collection is separated into Softcore and Hardcore Vaults; the UI has a Vault switcher and shows the active mode.
- [ ] No file in the Save Root is modified by the import; a failed parse keeps the previous Collection and leaves the backup intact.
- [ ] Tests cover a fresh import, a repeated import, a failed import, and the API Collection count using a temporary database and synthetic saves.
