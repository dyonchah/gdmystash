# Development environment

This is the specific setup used to develop and test the application. It is not part of the product.

## Machine

- Windows 11 (build 26300)
- Intel Core Ultra 5 125H, 16 GB RAM, Intel Arc Graphics
- Node.js 24, npm 11
- .NET 8 SDK installed per-user at `%LOCALAPPDATA%\Microsoft\dotnet`

## Game installation

- Path: `C:\Users\office\gms\Grim Dawn (2016)\Grim Dawn`
- The install is a RUNE repack with the base game, Ashes of Malmouth, Forgotten Gods, and Fangs of Asterkarn.
- The root `Grim Dawn.exe` is x86; `x64\Grim Dawn.exe` is x64. The project targets x64.

## Fixes required to run the repack

1. Install **Visual C++ 2013 Redistributable** x86 and x64. The game needs `VCOMP120.DLL`.
2. Install **Visual C++ 2015–2022 Redistributable** x86. The x64 runtime was already present.
3. Install **DirectX End-User Runtime (June 2010)**. Windows 11 does not ship `d3dx9_43.dll` / `d3dcompiler_43.dll`.
4. Copy `x64\steam_api64.dll` to the game root. This repack's x64 launcher expects it there when the working directory is the root.
5. Launch `x64\Grim Dawn.exe` with the **game root** as the working directory, or use `Launcher x64.bat`.
6. In `%USERPROFILE%\Documents\My Games\Grim Dawn\Settings\options.txt`, set:
   - `resolution = 1920 1080`
   - `refreshRate = 60 60`
   - `screenMode = 1`
   - `skipCompatibilityChecks = true`
   - `setupComplete = true`

## Save roots

| Mode | Path |
|---|---|
| Standard local | `%USERPROFILE%\Documents\My Games\Grim Dawn\save` |
| Steam Cloud | `<Steam>\userdata\<SteamID>\219990\remote\save` |
| This repack's emulated cloud | `C:\Users\Public\Documents\Steam\RUNE\219990\remote\save` |

The emulated cloud folder is a normal local directory. The application should auto-detect all three and let the user confirm the active root.

## Notes

- The game has not been played yet on this machine; there are no `player.gdc` or `transfer.gst` files as of the initial setup.
- Test fixtures from `gd-edit` (`test-resources/save-files/*.gdc`) are used until real saves exist.
