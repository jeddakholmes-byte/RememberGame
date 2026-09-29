# Remember Me — Act I

An RPG Maker MV build of *Remember Me*, the serious game made for **CSS5190 Game Your Psychology: AI, Creativity & Mental Health** (CUHK-Shenzhen, Fall 2026).

Everything in this repository is the playable game plus the documentation the team needs to open, run, edit and redeploy it.

---

## Play it

| | |
|---|---|
| **Play in a browser** | <https://jeddakholmes-byte.github.io/RememberGame/> |
| **Direct link to the game** | <https://jeddakholmes-byte.github.io/RememberGame/game/> |
| **Open in RPG Maker MV** | open `game/Game.rpgproject` in RPG Maker MV 1.6.1 |
| **Run it locally without MV** | see [docs/LOCAL_SETUP.md](docs/LOCAL_SETUP.md) |

Controls: **arrow keys** to walk, **Enter / Space** to look or talk, **Esc / X** to open the menu. The game is 816 × 624 and uses the keyboard only.

---

## What is in here

```
RememberGame/
├── README.md                  ← you are here
├── index.html                 ← landing page (the "Play" portal on GitHub Pages)
├── .nojekyll                  ← tells GitHub Pages not to run Jekyll
├── docs/                      ← all written documentation
│   ├── FILE_GUIDE.md          ← what every file and folder is for
│   ├── GAME_REFERENCE.md      ← maps, events, switches, walkthrough
│   ├── ACT1_SCRIPT.md         ← the full in-game text, in order
│   ├── LOCAL_SETUP.md         ← run it locally and open it in RPG Maker MV
│   ├── DEPLOY.md              ← how this repo was published and how to update it
│   ├── VERIFICATION.md        ← the browser playthrough that proved the build runs
│   ├── ENHANCEMENT_REVIEW.md  ← review of the 2026-09-29 enhancement package
│   └── screenshots/           ← frames captured during that playthrough
├── game/                      ← THE GAME. 1169 files, 400 MB. RPG Maker MV 1.6.1 project
└── enhancement/               ← the 2026-09-29 enhancement package (docs + art + patch data)
```

**`game/` is the whole RPG Maker MV project, unmodified.** Nothing was trimmed, renamed or rewritten. It is simultaneously:

- the project you open in the RPG Maker MV editor, and
- the web build that GitHub Pages serves, because RPG Maker MV games are plain HTML5 + JavaScript.

That is why the web version is a 1:1 copy of the original rather than a reimplementation: there is nothing to reimplement.

---

## The game

**Act I — a single chapter.** Grace, age 7, is at her grandmother's funeral. She walks a small cemetery, looks at the headstone, talks to her mother, father, uncle and grandfather, then follows her father into a flashback of the last argument he had with Grandma before she died. The chapter ends with Mom handing Grace the box that Grandma left for her.

| | |
|---|---|
| Player character | Grace (`$RM_Grace`) |
| Maps | `Act1_Cemetery` (Map001, 24 × 18) and `Act1_House_Flashback` (Map002, 24 × 18) |
| Event commands | 508 |
| Dialogue | 81 boxes, 187 lines, 513 words |
| Play time | roughly ten minutes |
| Switches used | 8 (all named — see [docs/GAME_REFERENCE.md](docs/GAME_REFERENCE.md)) |
| Ending | ends on `ACT I — END / The things Grandma left behind.` |

The chapter does not branch and cannot be failed. The only optional content is the four side conversations and the flower/grave inspections.

---

## Current state of the build — read this before you edit

`game/` is **not** the raw first draft and **not** the finished enhanced draft. It sits in between:

- The **character work is in**: Grandpa exists as an event, Aunt and Uncle James have their own sprites and face sets, the opening CONTROLS card is written, the menu/save access rules are in, and Dad's page-2 transfer goes to Map002 (12,13).
- The **polish pass is not finished**: the flashback still plays two screen shakes and two `RM_KnockHard` sounds, the "step backwards" fix is missing, there are no `<RMHint:>` notes on the events, the `RM_Act1_Experience` plugin is present but switched off, and `data/System.json` still reports `locale: "zh_CN"` with the default Chinese UI terms and `titleBgm` volume 90.

The full gap list, with the evidence, is in [docs/ENHANCEMENT_REVIEW.md](docs/ENHANCEMENT_REVIEW.md). Nothing was changed in `game/` when this repo was published — the gap is reported, not silently repaired.

**But the build does run, start to finish.** Before publishing, the game was played in a browser from New Game through the flashback to the `ACT I — END` card and the return to the title screen, with no script errors and no missing assets. The record is in [docs/VERIFICATION.md](docs/VERIFICATION.md).

---

## For teammates

| I want to… | Go to |
|---|---|
| understand what a file does | [docs/FILE_GUIDE.md](docs/FILE_GUIDE.md) |
| find where a specific line of dialogue or a switch lives | [docs/GAME_REFERENCE.md](docs/GAME_REFERENCE.md) |
| read the whole script as text | [docs/ACT1_SCRIPT.md](docs/ACT1_SCRIPT.md) |
| run the game or open the editor | [docs/LOCAL_SETUP.md](docs/LOCAL_SETUP.md) |
| publish an update to the site | [docs/DEPLOY.md](docs/DEPLOY.md) |
| see what was tested before publishing | [docs/VERIFICATION.md](docs/VERIFICATION.md) |
| finish the enhancement pass | [docs/ENHANCEMENT_REVIEW.md](docs/ENHANCEMENT_REVIEW.md) + `enhancement/` |

---

## Credits and asset licensing

The game was built with **RPG Maker MV 1.6.1** (Kadokawa / Degica). Most of the files under `game/img/` and `game/audio/` are the **Run Time Package assets that ship with RPG Maker MV**; they are redistributed here as part of the project, which is what the RTP is for. The original art and audio made for this game are the files whose names begin with `RM_` — the title art, the two parallax backgrounds, the gravestone and box pictures, the seven character face sheets, the four walk sheets, the collision tileset, the title BGM and roughly thirty sound effects.

The written commentary in `docs/` was produced for this project.
