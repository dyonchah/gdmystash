# 09: Favorites and saved searches

**What to build:** Let the user keep track of good finds and reuse complex searches. Items can be starred from the Collection card and the item drawer; the star persists across imports and restarts. The user can save the current search with a name, load it later, and delete it. A Favorites filter shows only starred items.

**Blocked by:** 06

**Status:** ready-for-agent

- [ ] Starring and unstarring an item works from both the Collection card and the item drawer and survives a restart and a re-import of unchanged items.
- [ ] The Collection can be filtered to Favorites and shows an accurate count.
- [ ] The current query, filters, and sort can be saved with a name, loaded later, and deleted.
- [ ] Saved searches and favorites are stored in SQLite and do not depend on browser storage.
- [ ] API and database tests cover persistence across a restart, favorite filtering, and saved-search round trips.
