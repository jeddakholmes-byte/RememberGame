# Remember Me · Unity Project

English | [简体中文](README.zh-CN.md)

[Repository home](../README.md) · [Play v10](https://jeddakholmes-byte.github.io/RememberGame/unity-v10/) · [Contributing](../CONTRIBUTING.md)

## Open and play

1. Install Unity **6000.6.1f1** and add this directory in Unity Hub.
2. Open `Assets/Scenes/RememberMeAct1.unity` and press Play.
3. Choose **New Game** for Act I. Choose **Kitchen** on the title menu to enter Act II directly. Its scene is `Assets/Scenes/Kitchen.unity`.
4. Read the [interaction conventions](Docs/Interaction.md) before changing interactions.

Web builds require Web Build Support. Windows builds require Windows Build Support. The local `Play-RememberMe.cmd` opens the current public web version.

## Where to make changes

| Area | Files or directory |
| --- | --- |
| Act I story and state | `Assets/Scripts/Act1Game.cs`, `Act1Story.cs` |
| Act I rooms, objects, collisions, and layout | `Assets/Editor/Act1Builder.cs`, `Assets/Scripts/Act1Room.cs` |
| Act II kitchen, hallway, living room, and story | `Assets/Scripts/KitchenPrototype.cs` |
| Kitchen UI, recipe, and inventory | `Assets/Scripts/KitchenCanvas.cs`, `KitchenPresentation.cs`, `KitchenInventory.cs` |
| Shared input, dialogue, and menus | `Assets/Scripts/StoryInput.cs`, `Act1HUD.cs` |
| Character movement and animation | `Assets/Scripts/GraceMotor.cs`, `ActorView.cs`, `GrandmaFigure.cs` |
| Act I art and audio | `Assets/Art/`, `Assets/Audio/` |
| Act II art, audio, and room data | `Assets/Resources/KitchenChapter/` |
| Editor checks and web builds | `Assets/Editor/Act1Verification.cs`, `WebBuild.cs` |
| Full route and browser verification | `Assets/Scripts/KitchenRouteCheck.cs`, `QA/Verify-Web.cjs` |
| Historical screenshots | [QA/Reference](QA/Reference/README.md) |

## Build and verify

In Unity, **Remember Me → Build for GitHub Pages** writes both acts to `Build/GitHubPages/docs/`. The command-line entry point is `RememberMe.Editor.WebBuild.Build`, with `-customBuildPath`. The Windows entry point is `RememberMe.Editor.Act1Builder.Build`, which writes to `Build/Windows/`.

From the repository root run `node tools/build-players.cjs "<Unity editor executable>"` to build Windows and WebGL in sequence. Zip the contents of Build/Windows together. Players extract the full ZIP and run RememberMe.exe.

Builds call `Act1Builder.Assemble` to regenerate the Act I scene. Keep layout changes in the assembly script and commit the scene together with new assets and their `.meta` files.

Run Windows route verification with `-remember-smoke -with-kitchen`. Add `-kitchen-capture` followed by a screenshot directory to capture images. Browser verification uses `QA/Verify-Web.cjs` and requires Node.js, Playwright, and Chrome or Chromium. See the [deployment guide](../docs/DEPLOY.md) for commands.

## Local files and historical material

`Library/`, `Temp/`, `Logs/`, `UserSettings/`, and `Build/` are generated locally and covered by `.gitignore`. `QA/Reference/` holds early screenshots, including scenes from earlier story versions.

`Assemble-Art.ps1` is an early import tool that depends on the original workspace and external source images. For routine work, use the existing resources under `Assets/`.

[LegacyDeployment](Docs/Archive/LegacyDeployment/README.md) preserves early GitHub Actions deployment examples. The current site serves built files from the root of the repository's `main` branch. Follow the repository deployment guide.
