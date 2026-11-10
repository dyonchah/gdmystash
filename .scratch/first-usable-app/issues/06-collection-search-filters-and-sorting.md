# 06: Collection search, filters, and sorting

**What to build:** Make a large Collection navigable. The Collection page gets a search bar, filter controls, sorting, a result count, and URL-persisted query state. Search covers item names and affixes. Filters cover slot, rarity, required level range, damage type, set, Monster Infrequent, and affix. Results update without a full page reload and show a useful empty state when nothing matches.

**Blocked by:** 05

**Status:** ready-for-agent

- [ ] Full-text search finds items by name and by affix name.
- [ ] Filters cover slot, rarity, required level range, damage type, set, Monster Infrequent, and affix, and can be combined.
- [ ] Sorting by name, required level, and rarity works and is reflected in the URL together with the query and filters.
- [ ] The result count updates with the query, and an empty result shows a clear empty state with a way to clear filters.
- [ ] API tests seed a temporary Collection and assert search, each filter, combined filters, sorting, and pagination.
