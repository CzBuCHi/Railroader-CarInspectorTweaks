# Car Inspector Tweaks

Simple mod, that modifies warious dialogs:

1. Whistle Buttons
    - Adds 'Previous' and 'Next' buttons under whistle drop down to simplify whistle selection.
1. Faster strong man
    - Engineer is able to push car up to 6mph.
1. Remember tab
    - Game will remember selected tab when selecting different car of same type.
1. Copy repair destination
    - Adds 'Copy repair destination' button to equipment panel that will set entire consist repair destination based on currect car.
1. Show car speed
    - Shows car speed on car tab (locomotives are excldued).
1. Show car oil
    - Shows car oil state on car tab.
1. Bleed all
    - Adds 'Bleed all' button to car panel.
1. Toggle switch
    - Adds 'in front' and 'behind' buttons to manual orders and yard tab to toggle closest switch in front / behind train.
1. Manual controls
    - Adds controls to manual orders tab.
1. Update customize window
    - Updates car customize window when different car that can be customized is selected.
1. Manage Consist
    - Adds 'connect air', 'release handbrakes' and 'oil all cars' buttons to manual orders and yard tab.
    - buttons are visible only if needed
    - oil all cars buttons show oil status of worst car in consist
1. Waypoint controls
    - Adds 'WP' mode to orders tab to set / clear waypints

## Installation

-   Download `CarInspectorTweaks-VERSION.zip` from the releases page
-   Install with [Railloader](<[https://www.nexusmods.com/site/mods/21](https://railroader.stelltis.ch/)>) or
    [Railloader - Experimental]([https://cdn.discordapp.com/attachments/1184809935616352336/1314710127953055784/Railloader_1.9.0.10.zip)

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
