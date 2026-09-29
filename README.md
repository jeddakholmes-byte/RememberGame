**English** · [简体中文](README.zh-CN.md)

# Remember Me — Act I

A turn-based narrative game for the final project of **CSS5190 Game Your Psychology: AI, Creativity & Mental Health** (The Chinese University of Hong Kong, Shenzhen, Fall 2026), made with RPG Maker MV 1.6.1.

This repository is both a **playable game** and the **team's workbench**: art specs, script specs, file lists and the latest build all live here.

---

## Which file to look at

| You are | Look at this | What is in it |
|---|---|---|
| **Artist** | [`art/README.md`](art/README.md) | which folder each kind of asset goes in, file sizes and how the grid maths works, naming rules, the list of art that already exists, the handover process, the submission area |
| **Writer** | [`script/README.md`](script/README.md) | how one line of dialogue is written in the game, how many characters per row, colour codes and expression indexes, character voice, templates you can copy, where drafts go |
| Looking for what a file is for | [`docs/FILE_GUIDE.md`](docs/FILE_GUIDE.md) | what every folder and key file in the repository is for, and whether you can change it |
| Looking for a line or a switch | [`docs/GAME_REFERENCE.md`](docs/GAME_REFERENCE.md) | the 23 events, 8 switches and the walkthrough for both maps |
| Wanting to read the full script | [`script/ACT1_SCRIPT.md`](script/ACT1_SCRIPT.md) | every in-game line of Act I, in order of appearance |
| Adding files to the repository | [`docs/DEPLOY.md`](docs/DEPLOY.md) | where to put them, how to commit, how to publish |
| Just wanting to play | <https://jeddakholmes-byte.github.io/RememberGame/> | play in the browser, nothing to install |

---

## Play / open

| | |
|---|---|
| **Play online** | <https://jeddakholmes-byte.github.io/RememberGame/> |
| **Direct game link** | <https://jeddakholmes-byte.github.io/RememberGame/game/> |
| **Open it in MV to edit** | Open `game/Game.rpgproject` with RPG Maker MV 1.6.1 |
| **Run it on your own machine** | `cd game && python3 -m http.server 8080`, then open <http://127.0.0.1:8080/>. **Double-clicking `index.html` does not work** — it must be served over HTTP |

Controls: **arrow keys** to walk, **Enter / Space** to look or talk, **Esc / X** for the menu. The screen is 816 × 624 and only the keyboard is used.

---

## What is in the repository

```
RememberGame/
├── README.md              ← you are here
├── index.html             ← landing page; English by default, with an EN / 中文 switch
├── art/                   ← for the artist
│   ├── README.md              asset specs, sizes, naming, inventory, handover process
│   ├── specs/                 auto-generated reference sheets (8 expressions and the sprite grid per character)
│   └── incoming/              put new assets here first — not straight into game/
├── script/                ← for the writer
│   ├── README.md              writing rules, format, voice, templates
│   ├── ACT1_SCRIPT.md         the full text of Act I, exported from the project (English only)
│   └── drafts/                new script drafts
├── docs/                  ← technical and operations
│   ├── FILE_GUIDE.md          what every file is for
│   ├── GAME_REFERENCE.md      maps / events / switches / flow
│   ├── DEPLOY.md              how to add files, how to publish
│   ├── VERIFICATION.md        the browser test record taken before going live
│   └── screenshots/           game screenshots
└── game/                  ← the game itself. 1169 files, 400 MB, an RPG Maker MV 1.6.1 project
```

**Languages.** Every document above except `script/ACT1_SCRIPT.md` also exists in Chinese at the same path with `.zh-CN.md` appended — `README.zh-CN.md`, `art/README.zh-CN.md`, and so on. Each page links to its other language in the first line. `ACT1_SCRIPT.md` has no translation because it is the English text the game actually plays.

---

**`game/` is the complete project, and nothing in it has been deleted or changed.** It is two things at once:

- the project you open in RPG Maker MV, and
- the web build GitHub Pages is serving.

This is not a coincidence. An RPG Maker MV game is already HTML5 + JavaScript, with no "conversion" step — so the web build is a 1:1 copy of the original, not a rewrite.

---

## What the game is

**Act I, a single chapter.** Grace is seven, at her grandmother's funeral. She walks once around the cemetery, looks at the headstone, talks to Mom, Dad, Uncle James and Grandpa, then follows Dad into a flashback — the last argument between Dad and Grandma before she died. The chapter ends with Mom handing Grace the box Grandma left her.

| | |
|---|---|
| Protagonist | Grace (`$RM_Grace`) |
| Maps | `Act1_Cemetery` (Map001) and `Act1_House_Flashback` (Map002), both 24 × 18 |
| Event commands | 508 |
| Dialogue | 81 message boxes, 187 lines, 513 words |
| Length | about ten minutes |
| Switches | 8, all named — see [`docs/GAME_REFERENCE.md`](docs/GAME_REFERENCE.md) |
| Ending | `ACT I — END / The things Grandma left behind.`, then back to the title screen |

There is no branching and you cannot fail. The only optional content is four side conversations and the examination of the flowers and the old headstone.

---

## Build state — read before you edit

The build is **not final**, but the polish pass has started.

**Already in.** The four characters all have their own sprites and face sets, and Grandpa's event exists at (16,7). The opening CONTROLS card is written, and the menu and save access rules are in place. Mom's long conversation only plays once and the two flower clusters share a switch. Dad's page-2 transfer goes to Map002 (12,13) facing right. The headstone and the box close-ups fade in and out instead of popping. **The box handover works again:** after Mom's line the screen fades out, Mom and Grace are repositioned to (8,14) and (9,14), and the scene fades back in on the two of them at the bench — so the full-screen `RM_Box` picture reads as a deliberate cutaway rather than a teleport.

**Still not in.** The flashback still plays two screen shakes and two `RM_KnockHard` sounds, and Grace's last step there is still "move up" rather than "step backward". No event carries an `<RMHint:>` note yet, and `RM_Act1_Experience` sits in `js/plugins/` without being switched on in `js/plugins.js` — so there are no interaction hints and no footsteps. `data/System.json` still reports `locale: "zh_CN"` with the default Chinese UI terms, `optFollowers: true` and title BGM volume 90. Event 017 still contains the typo `abondoned`, and the ending's Control Switches command still turns on the whole range 0001–0005 instead of only 0005.

**Nothing was changed to hide this.** No file in `game/` was edited when the repository was published.

The build does run, though: it has been played in a browser from New Game through the flashback to the `ACT I — END` card, with no script errors and no missing assets. See [docs/VERIFICATION.md](docs/VERIFICATION.md).

---

## Before you add files

- **New assets go into `art/incoming/` first**, and new script goes into `script/drafts/` first. `game/` is always a clean, working build — do not drop half-finished work straight into it.
- **Do not rename or overwrite existing files in `game/img/` or `game/audio/`.** The game looks up images by filename string, so a rename throws no error — the image just silently fails to show.
- **Always play through after adding something, and start from New Game.** Loading an old save brings the old switch states in with it and makes a correct change look broken.
- For the exact commit and publish steps, see [`docs/DEPLOY.md`](docs/DEPLOY.md).

---

## Copyright and asset sources

Made with **RPG Maker MV 1.6.1** (Kadokawa / Degica). Most files in `game/img/` and `game/audio/` are Run Time Package assets that ship with RPG Maker MV, and distributing them with the project is what the RTP is for.

**Every asset made for this game starts with `RM_` in the filename** — the title image, the two parallax base images, the headstone and keepsake box close-ups, seven face sets, four sprite sheets, the collision tiles, the title music, and about thirty sound effects.
