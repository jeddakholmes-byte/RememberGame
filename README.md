**English** · [简体中文](README.zh-CN.md)

# Remember Me

A narrative game about Grace, her grandmother and family memories, made for CSS5190 Game Your Psychology at The Chinese University of Hong Kong, Shenzhen.

[Play v10 · Acts I–II](https://jeddakholmes-byte.github.io/RememberGame/unity-v10/) · [Download for Windows x64](https://jeddakholmes-byte.github.io/RememberGame/windows-v10/) · [Report playtest feedback](https://github.com/jeddakholmes-byte/RememberGame/issues/new) · [Contributing](CONTRIBUTING.md)

Start with the [handoff notes](docs/HANDOFF.md) for November 24–25 changes, uploaded materials, verification evidence and next tasks.

## Start here

| Task | Entry |
| --- | --- |
| Open and edit the game | [Unity project setup](RememberMeUnity/README.md) |
| Change interaction, inventory or movement | [Interaction rules](RememberMeUnity/Docs/Interaction.md) |
| Find scripts, art or historical material | [Documentation index](docs/README.md) |
| Build and publish | [Deployment guide](docs/DEPLOY.md) |
| Write new scenes or supply artwork | [Contribution workflow](CONTRIBUTING.md) |

## Current game

The Unity project contains Acts I and II. Grace explores the cemetery, sees a family flashback and briefly visits an empty memory house. A rainy journey home leads into her bedroom. Opening the keepsake brings her into Grandma's kitchen, where ingredients, picture cards and baking lead to the hallway and living-room sequence.

The v10 release adds WASD menu navigation, reversible recipe-card placement, illustrated inventory, kitchen save/load, corrected water-door overlays and grounded scripted walking.

## Repository layout

| Path | Contents |
| --- | --- |
| [RememberMeUnity/](RememberMeUnity/) | Active Unity source project, assets, editor builders and QA script |
| [art/incoming/](art/incoming/) | Artwork submissions and source references |
| [script/drafts/](script/drafts/) | Narrative proposals and revisions |
| [docs/](docs/) | Documentation index, deployment and earlier reference material |
| [unity-v10/](unity-v10/) | Published v10 WebGL output |
| [unity/](unity/) | Stable browser entry that redirects to the current release |
| [archive/web/](archive/web/) | Archived v2–v9 builds, with redirects from earlier shared entry URLs |
| [game/](game/) | Original RPG Maker MV project and browser edition |
| [tools/](tools/) | MV asset, map and audio utilities, plus archived web-entry verification |

## Play and edit

Use a desktop browser for the [online game](https://jeddakholmes-byte.github.io/RememberGame/unity-v10/). WASD or arrows move Grace. Shift runs, C crouches, E / Enter / Space confirms, I opens inventory and Esc opens the menu. In menus, A/D selects horizontally and W/S vertically. Backspace undoes a recipe card. Placed cards also accept mouse clicks to undo placement.

Install Unity **6000.6.1f1**, add RememberMeUnity through Unity Hub, and open Assets/Scenes/RememberMeAct1.unity. The title's Kitchen entry opens Act II directly. Install Web Build Support for WebGL builds and Windows Build Support for Windows builds.

## Collaborate

Pull main, create a feature branch, make a focused change, play the affected route and open a pull request. Commit Unity assets together with their .meta files. Scene assembly is driven by Assets/Editor/Act1Builder.cs, which runs during builds, so put lasting scene-layout changes in that builder.

[Contribution steps and review checklist](CONTRIBUTING.md)

## Credits and assets

The repository contains project-created artwork, animation and audio alongside the original MV resources. Track the source and license when adding an asset. The web build includes the Tinos font license. MV resources remain subject to their original license terms.
