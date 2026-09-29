# Verification record — does the build actually run?

Before this repository was published, the shipped build in `game/` was run in a real browser and driven from **New Game** to the end of Act I. This file records exactly what was done, what was observed, and what was *not* tested.

Screenshots are in [`screenshots/`](screenshots/).

---

## Method

- Served `game/` over HTTP (`python3 -m http.server`) and loaded `index.html` in Chrome.
- Drove the browser through the DevTools protocol: the game's own input layer was used (`Input._onKeyDown` / `Input._onKeyUp`) rather than synthetic OS key events, because RPG Maker MV polls input once per frame and a key press shorter than one frame is dropped.
- Read the engine's live state between presses: current scene, `$gameMap._mapId`, player position, the eight switches, `$gameMessage.isBusy()`, `$gameMap.isEventRunning()`.
- The game ran at a steady 60 fps in the test browser.

---

## Result: the chapter completes

| Step | Expected | Observed |
|---|---|---|
| Boot | title screen with `GAME START / CONTINUE / SETTINGS` | ✓ |
| Start a new game | `Scene_Map`, Map 1, player at (12,12), opening message running | ✓ after 1 press |
| Play the cold open | ends with switch 1 (`OpeningComplete`) ON and the player free to move | ✓ after 12 presses |
| Enter the flashback | Map 2, player at (12,13), the flashback autorun playing | ✓ |
| Play the flashback | ends with switch 3 OFF, switch 4 (`FlashbackComplete`) ON, player back on Map 1 at (13,7) | ✓ after 14 presses |
| Play the ending | ends with switch 5 (`Act1Complete`) ON | ✓ after 24 presses |
| After the ending | returns to the title screen (command 354 = *Return to Title Screen*) | ✓ |

**No JavaScript errors and no frozen frames.** All 229 image and audio files referenced by `data/*.json` were confirmed present on disk, and every request the browser made during the run was served with HTTP 200. The log line for the final state was:

```
{"scene":"Scene_Title","map":1,"x":13,"y":7,"sw":"11111000","busy":false,"run":true}
```

`sw:"11111000"` is switches 1–8, so 1 through 5 are ON and 6–8 are OFF — exactly the state the ending is meant to leave behind.

---

## What the screenshots show

| File | What it is |
|---|---|
| `00_title_screen.png` | The title art with the command window. `optDrawTitle` is `false` in `System.json`, so the art is drawn by the opening event rather than by the engine. |
| `01_opening_over_black.png` | Grace's first line, mid-typewriter. **This is intentional**: the opening event fades the screen to black *before* the first two lines, and fades back in afterwards. |
| `02_cemetery_free_roam.png` | The cemetery once the player is free, in the rain. Grace, Mom, Dad, Uncle James, Aunt and Grandpa are all on screen with their own sprites. |
| `03_flashback.png` | The flashback interior with Grandma's portrait and English dialogue. |
| `04_back_in_cemetery.png` | Back in the cemetery after the flashback. |
| `05_back_at_title_after_the_ending.png` | The title screen again, after the ending returned to it. |

---

## Checks that were performed along the way

- **Custom image geometry** — all `RM_*` assets match the engine's grid rules: `$RM_*` walk sheets are 144 × 192 (3 × 4 of 48 × 48); face sheets are 576 × 288 (4 × 2 of 144 × 144); the title is 816 × 624; the two parallaxes are 1152 × 864; the collision tileset is built on a 48 px grid; `Window.png` is 192 × 192.
- **No asset is missing** — every image and audio file referenced by `data/*.json` exists on disk, and every audio track referenced exists in both `.ogg` and `.m4a`.
- **No encryption** — `System.json` has no `hasEncryptedImages` / `hasEncryptedAudio` keys, so the plaintext project files load directly from a static host. (Encryption is what would make a project unplayable when served as plain files.)
- **The collision layer behaves** — tile 1536 in `Tilesets.json` has flag `1536` (low nibble zero → passable) and tile 1537 has flag `1551` (low nibble `0x0F` → impassable in all four directions), which matches the tileset's own note: *"A5 tile 1 = walkable, tile 2 = blocked."*
- **Event commands are internally consistent** — switch and variable reads and writes were traced across both maps; every switch that is tested somewhere is set somewhere, and the eight switches are named.

---

## What was NOT tested

Be explicit about this when you present the work.

1. **The RPG Maker MV editor was not opened.** Everything here is about the runtime. The project has not been opened and re-saved by MV 1.6.1 in this session, so if some editor-level problem exists (a plugin parameter edit, a tileset assignment), it has not been seen.
2. **The natural walking route to Dad was not played.** The test set the `VisitedGrave` switch directly and jumped into the flashback, mirroring exactly what Dad's event does, but the intervening walk, the headstone interaction, the flower and old-grave inspections, Mom's call, and the four side characters were not exercised by input.
3. **The menu was not opened.** `Esc` was never pressed in the test, and the menu is forbidden during the opening and allowed afterwards, so this has not been seen either way.
4. **The two known gaps are still present** and were observed to be present, not fixed: the flashback still plays two screen shakes and two `RM_KnockHard` sounds, and the system language is still `zh_CN`, so system text is Chinese. See [ENHANCEMENT_REVIEW.md](ENHANCEMENT_REVIEW.md) for the full list.
5. **Only Chrome, only desktop.** Safari and Firefox were not tested. Safari is the one to watch, because it needs the `.m4a` audio rather than the `.ogg`.
6. **No local playtest by a human.** This is an automated run with no audio verification.

---

## How to re-run it

```bash
cd game
python3 -m http.server 8080
```

Then open <http://127.0.0.1:8080/> and play from New Game. The full walkthrough is in [GAME_REFERENCE.md](GAME_REFERENCE.md) §6.
