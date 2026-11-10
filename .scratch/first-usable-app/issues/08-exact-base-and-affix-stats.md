# 08: Exact base and affix stats

**What to build:** Turn the item drawer from record-level information into a real tooltip. The stat engine computes exact base item stats plus prefix and suffix stats from the Item Definition and the Item Instance seed, and displays them in the same sections a player expects in game. Anything the engine cannot compute is shown as text or marked `not computed`, never as a wrong number.

**Blocked by:** 07

**Status:** ready-for-agent

- [ ] For a known base Item Definition, prefix, suffix, and seed, the computed damage, armor, attribute, resistance, and other supported values match the in-game tooltip.
- [ ] The item drawer renders computed stats in readable sections and keeps affix lines grouped with their source.
- [ ] Components, augments, relics, ascended affixes, set bonuses, and procs remain text-only and are clearly marked as not computed in this milestone.
- [ ] Unsupported stat kinds are surfaced as unresolved text and never silently dropped or fabricated.
- [ ] Deterministic tests cover several base item types and seeds; a local manual comparison against the real game is documented.
- [ ] The implementation uses the MIT-licensed GDIA stat logic as its reference and does not copy unlicensed code.
