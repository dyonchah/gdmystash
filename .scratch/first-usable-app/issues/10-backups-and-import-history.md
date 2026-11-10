# 10: Backups and import history

**What to build:** Make imports auditable. Settings gets a Backups section listing every snapshot with its timestamp, size, and path, plus an Import History section showing each run's status, counts, duration, and warnings. A failed import is recorded with its error and keeps its backup. Restoring a backup is not part of this ticket.

**Blocked by:** 05

**Status:** ready-for-agent

- [ ] Settings lists backups with timestamp, size, and a way to open the containing folder.
- [ ] Import history lists each run with status, character count, item count, duration, and warnings.
- [ ] A failed import is recorded with a readable error, the previous Collection remains intact, and the backup is retained.
- [ ] The last successful import time is visible on the Dashboard and in Settings.
- [ ] API tests cover a successful run, a failed run, and backup listing; no restore operation is exposed.
