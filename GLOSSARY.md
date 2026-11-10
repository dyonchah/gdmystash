# Glossary

The project uses the following terms consistently in code, UI, and documentation. Definitions describe the domain, not the implementation.

| Term | Definition |
|---|---|
| **Collection** | The set of items the application owns, independent of where those items currently are. |
| **Vault** | An isolated collection for a game mode or dataset. Softcore and Hardcore are separate vaults; mods will get their own vaults later. |
| **Item Definition** | A record in the game database that describes a kind of item: a base item, an affix, a component, or an augment. |
| **Item Instance** | A specific item, identified by its record names and seed. Two instances of the same base item can roll different stats. |
| **Item Replica** | The game's serialized representation of an item instance, used when moving the item between the game and the application. |
| **Game Database** | The compiled game data (`database.arz` plus the expansion archives) that defines item types, affixes, and stats. |
| **Game Folder** | The Grim Dawn installation directory that contains the game database and assets. |
| **Snapshot Import** | A read-only import that copies and parses save files without modifying them. It is not a Sweep because nothing is removed from the game. |
| **Affix** | A prefix or suffix applied to an item instance. |
| **Component** | An item attached to a piece of gear. In the save format these are stored in the `relic` fields. |
| **Augment** | A faction augmentation attached to a piece of gear. |
| **Relic** | A crafted item with a completion bonus. |
| **Ascended** | A Fangs of Asterkarn item variant with an additional affix. |
| **Stash** | In-game storage. It can be a character's personal stash or the shared transfer stash. |
| **Transfer Stash** | The shared stash that is available to every character in a game mode. |
| **Inbox Tab** | The transfer stash tab that the game writes new items into and the application loots from. |
| **Deposit Tab** | The transfer stash tab that the application writes returned items into. |
| **Loot** | Move an item from the game into the collection. |
| **Transfer** | Move an item from the collection back into the game. |
| **Sweep** | An explicit, offline import of items from existing stash tabs or characters. |
| **Live Mode** | The game is running and the hook is active; loot and transfer happen without restarting the game. |
| **Offline Mode** | The game is closed; the application reads and writes save files directly. |
| **Hook Session** | The lifetime of the injected helper inside a running game process. |
| **Character** | A player save (`player.gdc`) with progression, equipment, and inventory. |
| **Save Root** | The directory that contains the game's save files. |
| **Emulated Cloud Save** | The repack's local `remote` directory. It looks like a cloud save to the game but is an ordinary local folder. |
| **Favorite** | A user-marked item or saved search that is pinned for quick access. |
