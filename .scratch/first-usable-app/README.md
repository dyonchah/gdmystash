# First usable app — tickets

Source spec: [001-first-usable-app.md](../../docs/specs/001-first-usable-app.md)

Tickets are in dependency order. Each ticket is a vertical slice with its own acceptance criteria and a single fresh-context scope.

| # | Ticket | Blocked by |
|---|---|---|
| 01 | App shell, status, and path setup | None |
| 02 | Character summary and character list | 01 |
| 03 | Character equipment, inventory, and personal stash | 02 |
| 04 | Game Database index and item names | 03 |
| 05 | SQLite collection and read-only snapshot import | 03, 04 |
| 06 | Collection search, filters, and sorting | 05 |
| 07 | Item detail drawer | 06 |
| 08 | Exact base and affix stats | 07 |
| 09 | Favorites and saved searches | 06 |
| 10 | Backups and import history | 05 |
| 11 | Dashboard, first-run, and error states | 05, 06, 09, 10 |

Status values follow the local tracker convention: `ready-for-agent` for every ticket below.
