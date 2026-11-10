# 03: Character equipment, inventory, and personal stash

**What to build:** Materialize Item Instances from a Character's equipment, inventory sacks, and personal stash, and show them in a Character detail view. Equipment shows all twelve slots and both weapon sets; inventory shows sacks and item positions; the personal stash shows pages and positions. Item records are displayed by record path until ticket 04 resolves names.

**Blocked by:** 02

**Status:** ready-for-agent

- [ ] The Character detail endpoint returns equipment slots, inventory sacks, and personal stash pages with each Item Instance's record names, seed, stack count, and position.
- [ ] The Character detail view renders equipment, inventory, and personal stash grids with the correct item counts and positions.
- [ ] The real level-4 Character shows the four items in the personal stash; equipment and inventory show their actual contents.
- [ ] Item records with an unexpected layout fail closed with a clear warning; the Character summary remains visible.
- [ ] Synthetic fixtures cover an empty Character, a Character with equipment, and a Character with a populated personal stash; the existing structural readers remain green.
