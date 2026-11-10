# 04: Game Database index and item names

**What to build:** Make item records human-readable. The app indexes the base Game Database and all installed expansion archives, indexes the text archives, and resolves each Item Instance to a display name, type, slot, required level, and rarity. Character grids stop showing raw record paths and show real item names. Settings reports the Game Database status and any indexing problem.

**Blocked by:** 03

**Status:** ready-for-agent

- [ ] The app indexes the base archive and every installed expansion archive, reports the total record count, and rebuilds the index when the archive fingerprint changes.
- [ ] Item display names resolve through text tags; a missing tag falls back to the record path and is marked as unresolved rather than invented.
- [ ] Item type, slot, required level, and rarity are derived from the Item Definition; unknown values display as `Unknown`.
- [ ] Character equipment, inventory, and personal stash cards show resolved names, type, required level, and rarity.
- [ ] Settings shows Game Database status and actionable errors for a missing, unreadable, or incompatible archive.
- [ ] Synthetic ARZ/ARC fixtures cover record lookup and text-tag resolution; real game data is a local-only integration check and is never committed.
