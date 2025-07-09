# What Am I Playing? - BlishHUD Module

A BlishHUD module that helps you decide what to play by offering random role suggestions based on your unlocked professions and elite specializations in Guild Wars 2.

*Please note that these instructions are subject to change as the project moves forward as new features are implemented.*

## Setup

1. Install Visual Studio 2022 if you don't have it yet. The community version is free: https://visualstudio.microsoft.com/downloads/
1. Download the newest Blish HUD version from the website and extract the blish zip: https://blishhud.com/
1. Download the What Am I Playing module code or use git clone.
1. Change the executablePath in \WhatAmIPlayingModule\Properties\launchSettings.json to where the blishhud.exe from the extracted blish zip is.
1. Open the WhatAmIPlayingModule.sln in Visual Studio. 
1. Right-click on the Solution icon in the Solution Explorer and select **Restore Nuget packages** (may not be necessary when using visual studio 2022)
1. Start Guild Wars 2.
1. In the visual studio menu bar click on the dropdown next to the green arrow. Select "gw2".
1. Press the green arrow to start blish with the What Am I Playing module in debug mode. It will overlay Guild Wars 2.

You can overlay a powershell or other window instead of gw2, too. But when you don't overlay gw2, api keys will not work in blish because gw2 mumble link cannot be accessed by blish. Overlaying something else than gw2 can be still useful for debugging modules.

Additional infos for debugging ("Configuring Your Project" can be ignored): https://blishhud.com/docs/modules/overview/debugging

## Features

- **Random Role Suggestions**: Get random role suggestions based on your unlocked professions and elite specializations
- **Role Categories**: Choose from different role types:
  - **Full Random**: Any role from any unlocked profession/spec
  - **DPS**: Damage-dealing roles (with options for Power or Condition damage)
  - **Alacrity**: Alacrity support roles
  - **Quickness**: Quickness support roles
  - **Heal Alac**: Healing with alacrity support
  - **Heal Quick**: Healing with quickness support
- **Account Integration**: Uses your Guild Wars 2 API to check unlocked content
- **Clean Interface**: Easy-to-use window with role suggestions and character information
- **Customizable Settings**: Adjust preferences and appearance

## How It Works

The module connects to your Guild Wars 2 account via API to check which professions and elite specializations you have unlocked. When you request a role suggestion, it randomly selects from your available options based on the role category you choose.

This is perfect for players who:
- Can't decide what to play
- Want to try different roles and builds
- Have many characters and want to rotate through them
- Are looking to expand their role repertoire

## Development

This module is based on the BlishHUD example module template and has been customized for providing random role suggestions in Guild Wars 2.
