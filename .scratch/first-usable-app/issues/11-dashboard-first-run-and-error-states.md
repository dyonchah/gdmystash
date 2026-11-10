# 11: Dashboard, first-run, and error states

**What to build:** Make the first usable app feel finished. The Dashboard becomes the landing page with the active Save Root, Game Database status, whether Grim Dawn is running, Character and item counts, last import, and backup count. First-run, empty, loading, and error states guide the user instead of showing blank panels. The layout is polished and responsive down to phone width.

**Blocked by:** 05, 06, 09, 10

**Status:** ready-for-agent

- [ ] The Dashboard shows the active Save Root, Game Database status, game process state, Character count, item count, last import time, and backup count.
- [ ] First run with no configured paths, no saves, or a missing Game Database shows a guided next action instead of an empty or broken screen.
- [ ] Collection, Characters, item drawer, and Settings have loading skeletons and error boundaries; a failed request leaves navigation usable.
- [ ] The layout is usable at desktop width and at phone width, and the dark theme is consistent across all pages.
- [ ] The empty-state and error copy is actionable, and a manual smoke test covers the first-run, healthy, and broken-path scenarios.
