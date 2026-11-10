# 07: Item detail drawer

**What to build:** Let the user inspect an item without leaving the Collection or Character view. Clicking an item opens a detail drawer with the resolved name, type, slot, rarity, required level, affixes, component, augment, relic, ascended affix, set name, stack count, and current location. Components, augments, set bonuses, and procs are shown as text in this ticket; exact computed numbers arrive in ticket 08.

**Blocked by:** 06

**Status:** ready-for-agent

- [ ] Clicking an item in the Collection or a Character grid opens a detail drawer with the record-level details and every resolved attachment.
- [ ] The drawer shows component, augment, relic, ascended affix, set name, and set-bonus text when present, and clearly marks anything not computed yet.
- [ ] Unknown or unresolved records display their raw record paths and an `Unknown item` marker instead of invented values.
- [ ] The drawer shows the item's source location and closes without losing the Collection query or scroll position.
- [ ] API tests assert the detail contract for a normal item, an item with a component and augment, and an unresolved record.
