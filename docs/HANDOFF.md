**English** · [简体中文](HANDOFF.zh-CN.md)

# Contributor handoff

Updated November 25, 2026. Start here, then read the [Unity project guide](../RememberMeUnity/README.md) and [interaction rules](../RememberMeUnity/Docs/Interaction.md).

Before the next upload, run node tools/check-repo.cjs and follow the [upload workflow](UPLOAD.md). release.json records the published version and its source commit.

Project documentation now provides 27 English/Chinese pairs, including the complete historical Act I reading copy and kitchen working draft. English files are the default entry. The upload preflight checks pairing and language links through tools/check-languages.cjs.

## Uploaded work

| Material | Location or commit | Status |
| --- | --- | --- |
| November 24 kitchen, recipe and family-memory work | 63ffbf0, 563cb85, ab11d82 | Included in main and traceable through commit history |
| November 25 narrative, art and interaction fixes | 98c6c90 | Source, assets, .meta files, scenes and web output included in main |
| Bilingual READMEs, contribution workflow and screenshot organization | b81c9a7, 608e6c8 | Included in main |
| Active Unity project | RememberMeUnity/Assets, Packages, ProjectSettings | Ready to clone and open in Unity Hub |
| Published browser build | unity-v10/ | Publicly available through unity/ |
| Act II kitchen working draft v0.4 | [scene1_draft_2.md](../script/drafts/scene1_draft_2.md) | Added from the supplied local draft for implementation review |
| Side-fence proposal | [Preview](handoff/Cemetery-Side-Fence-Preview.png) | Uploaded for the user's replacement decision |
| Browser and public-entry evidence | [Full-route log](handoff/2026-11-25-browser-verification.log), [public-entry log](handoff/2026-11-25-public-verification.log) | Uploaded as concise verification records |
| Archive routing evidence | [Eight-entry check](handoff/2026-11-25-archive-verification.log) | Browser redirects preserve query parameters and fragments |

[Download v10 for Windows x64](https://github.com/jeddakholmes-byte/RememberGame/releases/download/v10-windows-x64/RememberMe-v10-Windows-x64.zip). Extract the entire ZIP and run RememberMe.exe.

## November 24–25 changes

- November 24 added the kitchen chapter, ingredients and recipes, Dad's cookie, family-memory clues, and the hallway and living-room sequence, published as v9.
- November 25 revised the cemetery's family interactions and four special headstones. The first music-box memory became an empty house, followed by a rainy walk home and the bedroom's second keepsake interaction.
- Bedroom furniture, rainy interiors and window views, ingredient illustrations and three visible recipe cards were added. Collision footprints and recovery for older saves inside furniture were corrected.
- The pause inventory and kitchen save/load were restored. The collected tray moved into inventory, and the water-door overlay was placed behind dialogue.
- Shared confirmation handling gained WASD menu navigation, recipe undo and progress retention. Scripted walking gained continuous poses and grounded soles. These changes shipped in v10.

## Verification

[Play v10](https://jeddakholmes-byte.github.io/RememberGame/unity-v10/) and use the player's Feedback link to report an issue.

Browser verification covers A/D horizontal navigation, W/S vertical navigation, both story routes, recipe undo, inventory and save/load, water-door recovery, Grandma's escort and the ending. The automated route checks successive walking poses during scripted movement. Human playtests assess the animation's visual quality.

## Next contributor tasks

### 1. Acceptance playtest and feedback

- Create a task branch from main and open Unity 6000.6.1f1 using the [contribution workflow](../CONTRIBUTING.md).
- Play from both New Game and Kitchen. Record the fireplace's foot placement, kitchen threshold, undo and reopening recipe cards, resuming after pause, Grandma's escort and living-room entry.
- For each report, record the version, reproduction steps, expected and actual behavior. Fix the relevant handler, repeat the route and include results in the PR.

### 2. Windows hardware acceptance

The desktop package uses v10 gameplay revision 98c6c90, the Unity 2022.3.20f1c1 compatibility build environment and Direct3D 11. The packaged player passed Act I, the chapter transition and the kitchen route under CrossOver on macOS. Check display, audio, WASD and recipe undo on native Windows hardware next.

### 3. Review the Act II draft and fence proposal

- Compare the kitchen v0.4 draft with triggers, dialogue and branches in KitchenPrototype.cs. List the differences and proposed changes before implementation.
- The draft includes an early pixel-art direction. Current integrated painterly assets provide the visual reference.
- The uploaded side-fence preview awaits the user's replacement decision.

## Implementation references

- Act1Game.cs and Act1Story.cs own Act I behavior. Act1Builder.cs owns layout. Builds reassemble Act I, so keep persistent layout edits in the builder.
- KitchenPrototype.cs owns Act II flow, KitchenCanvas.cs its native UI, and KitchenInventory.cs inventory and saves.
- Act1HUD.HandleMenuInput and StoryInput coordinate input. Mouse and keyboard call the same action handler.
- GraceMotor.MoveScripted preserves scripted walking. Place handles room entry and position recovery.
- Act I uses 96 PPU and Act II uses 134 PPU, with different coordinate origins. Apply the conversions in the interaction guide.
- Commit new assets with their .meta files. Historical web builds are in archive/web/, and root 404.html maintains earlier shared entry URLs.
