# 02: Character summary and character list

**What to build:** Read the header and bio/attribute block of every `player.gdc` in the active Save Root and show the Characters page. Each Character card shows name, level, class, hardcore flag, expansion, iron, and core attributes. A corrupt Character is reported without hiding the healthy ones. The Dashboard shows the Character count.

**Blocked by:** 01

**Status:** ready-for-agent

- [ ] The app discovers all campaign Characters in the active Save Root and reads their name, level, class, hardcore flag, expansion, iron, and attributes.
- [ ] The Characters page lists one card per Character with the summary values; the empty state is shown when no Characters exist.
- [ ] A corrupt or unsupported Character file produces a warning and does not prevent other Characters from loading.
- [ ] The Dashboard reports the number of imported Characters.
- [ ] Synthetic `.gdc` fixtures cover a healthy Character, an empty Character list, and a corrupt file; a local-only check against the real level-4 Character is documented.
