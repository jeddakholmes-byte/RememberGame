# Local setup

Two things you might want to do on your own machine: **play it** and **edit it**. They are different setups.

---

## A. Play it locally

You cannot open `game/index.html` by double-clicking it. The browser will refuse to load the map and database files, and you will get a black screen with an error in the console. RPG Maker MV loads its data over HTTP, so it needs an HTTP server.

Open Terminal, go to the repository, and serve the `game` folder:

```bash
cd path/to/RememberGame/game
python3 -m http.server 8080
```

Then open <http://127.0.0.1:8080/> in Chrome, Firefox or Safari.

No Python? Any static server works:

```bash
npx serve -l 8080 game      # Node
php -S 127.0.0.1:8080 -t game
```

**Stop the server** with `Ctrl + C` when you are done. Leaving it running is harmless but it will hold port 8080.

### What you should see

A short "Made with MV" splash, then the title art with `Remember Me` and the command menu (`GAME START / CONTINUE / SETTINGS`). Blank screen instead? Check the list at the bottom of this page.

---

## B. Open it in the RPG Maker MV editor

`game/Game.rpgproject` is the editor's entry point — it just contains `RPGMV 1.6.1`.

1. Install **RPG Maker MV** on Windows or macOS. Version **1.6.1** or later. The version stored in the project is 1.6.1; opening a project in an older editor is what causes "cannot load" errors.
2. Launch MV. On the start screen choose **Open Project**, then select `game/Game.rpgproject`.
3. The map tree shows two maps: `Act1_Cemetery` and `Act1_House_Flashback`. Double-click one to edit it.
4. Press the **play** button to test. MV runs the game from the project folder.

On macOS, MV is a 64-bit app; if Gatekeeper complains, allow it under *System Settings → Privacy & Security*.

### Where things are in the editor

| What you want | Where it is |
|---|---|
| Edit dialogue | Map → double-click the event → pick the page → edit the Show Text box |
| The cold open and the ending | Map 1 → event `OpeningController`, and event `AfterFlashback_EndAct` page 2 |
| The flashback script | Map 2 → event `FlashbackController` page 1 |
| Switches and their names | **Database** button in the toolbar → *Switches* tab |
| Screen size | Plugin Manager → `Community_Basic` → `screenWidth` / `screenHeight` |
| Plugin on/off and parameters | Plugin Manager |
| Event interaction hints | Event → **Note** field, top right. Not a Comment command. |
| Move the player start | Right-click Map 1 → **Edit Map** |

### The debug menu

The engine maps the physical key `F9` to the debug function (the `keyMapper` entry `120: 'debug'` in `js/rpg_core.js`). The official Chinese manual has no shortcut table, so treat this as engine behaviour rather than documented behaviour. It only works when the project is launched in test mode, and it is not exposed in the browser build.

On a freeze, force-quit the test window with `Alt + F4` (Windows) or the window's close button. Do not kill the editor itself.

---

## C. Traps that cost an afternoon

| Symptom | Cause | Fix |
|---|---|---|
| Black screen, nothing loads | Opened `index.html` via `file://` | Serve over HTTP (section A) |
| `Failed to load: data/Map001.json` | Wrong working directory — you served the repo root instead of `game/` | `cd game` first, then serve |
| Everything is silent | Browser blocked autoplay. Chrome and Safari require a user gesture before audio starts | Click once on the page, then start the game |
| No music on Safari but fine in Chrome | Safari does not play `.ogg` | Both `.ogg` and `.m4a` must exist. Do not delete either format. |
| A sprite appears as a giant crop of the wrong character | The file lost its `$` prefix, or the sheet is not 3 × 4 cells | `$` = single character. Sheets are 144 × 192 for a `$` sprite. |
| A picture looks like a tiled pattern | The file lost its `!` prefix (for parallaxes, `!` means "do not tile") | Restore the `!` |
| "This project was created in a newer version" | Your MV is older than 1.6.1 | Update MV |
| Your edit does not show up in game | You loaded a save from the middle of the chapter — switch states are restored from the save | Always test from **New Game** |
| Chinese boxes instead of letters | The window font cannot render the characters it was asked for | `RM_EnglishTypography.js` is the project's fix; the system language is still `zh_CN`, see [ENHANCEMENT_REVIEW.md](ENHANCEMENT_REVIEW.md) |
| The repo shows hundreds of changed lines after one editor save | MV rewrites whole JSON files, and it writes them without the `indent` keys | Expected. Review with `git diff --stat`, and expect `Map001.json` to be rewritten in full. |

---

## D. If you are editing and pushing back

Read [DEPLOY.md](DEPLOY.md). The short version:

```bash
git add -A
git commit -m "Act I: fix the flashback staging"
git push
```

GitHub Pages redeploys by itself in under a minute. Never commit from inside `game/` if you also have the editor open — save in MV first, then commit.
