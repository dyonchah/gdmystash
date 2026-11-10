# 01: App shell, status, and path setup

**What to build:** Replace the plain-text root with a real React app shell and make the app usable on first run. The user sees Dashboard, Collection, Characters, and Settings routes, gets candidate Save Roots and Game Folders detected, confirms both paths, and sees live status: which Save Root is active, whether the Game Database is present, and whether Grim Dawn is running. Settings persist across restarts. Collection and Characters show honest empty states until later tickets fill them.

**Blocked by:** None (can start immediately)

**Status:** ready-for-agent

- [ ] Opening the app at the local URL shows a styled, dark-themed React shell with Dashboard, Collection, Characters, and Settings navigation, not a plain-text response.
- [ ] On first run, the app lists detected Save Root candidates (standard local, OneDrive Documents, Steam `userdata`, and the repack's emulated cloud) and Game Folder candidates, and allows manual selection.
- [ ] Saving paths validates them: a Save Root must contain at least one `player.gdc` or a `transfer.gst`/`transfer.gsh`; a Game Folder must contain a Game Database archive.
- [ ] The status surface reports the active Save Root, Game Folder, Game Database presence, app version, and whether a `Grim Dawn` process is running.
- [ ] Settings persist across a server restart, and invalid paths produce actionable messages without breaking navigation.
- [ ] Collection and Characters pages have empty states that explain that an import is the next step.
- [ ] API tests cover status, settings persistence, candidate detection, and invalid paths; CI builds the web app.
