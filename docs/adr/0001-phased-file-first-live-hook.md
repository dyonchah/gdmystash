# 0001 — Phase file-first, then add the live hook

- **Status:** Accepted
- **Date:** 2026-11-10
- **Deciders:** project owner

## Context

GD Item Assistant (GDIA) does not edit `transfer.gst` to move items. It injects an x64 C++ DLL into the game process and intercepts `GAME::InventorySack::AddItem`. The intercepted add is suppressed, the item is serialized to a CSV queue, and the assistant imports it. Deposits go the other way: the assistant writes a CSV file, and the hook inserts the item into the second-to-last stash tab.

This is why GDIA can loot and deposit while the game is running. There is no documented file-only live mode. File-level tools such as GD Stash edit `.gst` only when the game is closed.

The product requirement is live parity with GDIA: the game is running, and items move both ways. The owner also approved a hybrid approach: a safe file-only mode first, with the hook added afterwards.

The hook is x64, uses Microsoft Detours, and is known to trigger antivirus false positives and to be sensitive to game updates. Writing it from scratch is possible but risky; adapting GDIA's MIT-licensed HookDll is the shorter path.

## Decision

1. Build the data layer and the file-only workflow first.
2. Treat live mode as a first-class goal, not an optional extra.
3. In phase 3, adapt GDIA's MIT-licensed HookDll behind a clean IPC boundary (named pipe + JSON), rather than exposing GDIA's CSV/window-message protocol to the rest of the app.
4. Keep the file-only and live paths behind one `TransferService` interface so the UI does not care which is active.

## Alternatives considered

- **Full live hook first.** Fastest to the headline feature, but it blocks all parser and UI work on the riskiest component and makes early testing depend on the game process.
- **File-only forever.** Safest and simplest, but does not meet the live requirement.
- **Clean-room hook from scratch.** Better control and a cleaner protocol, but significantly more reverse-engineering and testing.

## Consequences

- Two transfer modes must be designed, documented, and tested.
- The hook is optional at runtime; the app must degrade gracefully to offline mode.
- x64 only for v1.
- Antivirus false positives are expected; code signing and documentation may be needed if the app is distributed.
- Offline writes must refuse to run while the game is open; live writes must refuse when the stash state is unknown.
- The IPC boundary must be stable before the hook is written.
