# CarInspectorTweaks

Simple mod, that modifies Car inspector dialog:

1. Game will now remember selected tab (Car, Equipment, Passenger, Operations, Orders) based on type of car (Locomotive, Tender, Passenger, etc.)
2. Orders tab shows buttons [Select] and [Follow]
3. Manual mode on orders tab shows sliders for Throttle, Reverser, Independent and Train Brake

## Installation

* Download `CarInspectorResizer-VERSION.zip` from the releases page
* Install with [Railloader]([https://www.nexusmods.com/site/mods/21](https://railroader.stelltis.ch/))

## Project Setup

In order to get going with this, follow the following steps:

1. Clone the repo
2. Copy the `Paths.user.example` to `Paths.user`, open the new `Paths.user` and set the `<GameDir>` to your game's directory.
3. Open the Solution
4. You're ready!

### During Development
Make sure you're using the _Debug_ configuration. Every time you build your project, the files will be copied to your Mods folder and you can immediately start the game to test it.

### Publishing
Make sure you're using the _Release_ configuration. The build pipeline will then automatically do a few things:

1. Makes sure it's a proper release build without debug symbols
1. Replaces `$(AssemblyVersion)` in the `Definition.json` with the actual assembly version.
1. Copies all build outputs into a zip file inside `bin` with a ready-to-extract structure inside, named like the project they belonged to and the version of it.
