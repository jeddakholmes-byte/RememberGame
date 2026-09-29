**English** · [简体中文](README.zh-CN.md)

# Art asset guide

This folder is for you. Everything you need to change lives in `game/img/` and `game/audio/`; you do not need to touch any other folder.

Open these two images in a browser first — the rules make sense in about 30 seconds:

- [`specs/grid_rules.png`](specs/grid_rules.png) — how the grid is divided
- [`specs/faces_grace.png`](specs/faces_grace.png) — what a real reference sheet looks like

---

## 1. Where assets go and what they must be

| Asset type | Where it goes | File size | Grid | Naming | Notes |
|---|---|---|---|---|---|
| Character sprite (single) | `game/img/characters/` | **144 × 192** | 3 columns × 4 rows, 48×48 per cell | prefix with `$`, e.g. `$RM_Grace.png` | `$` means "this sheet holds one person" |
| Character sprite (multiple) | `game/img/characters/` | **576 × 384** | 4 characters across × 2 characters down, 144×192 each | no `$`, e.g. `RM_Family.png` | one sheet holds 8 characters |
| Character face set | `game/img/faces/` | **576 × 288** | 4 columns × 2 rows, 144×144 per cell | `RM_Face_<character>.png` | 8 expressions per sheet, numbered 0–7 |
| Parallax background | `game/img/parallaxes/` | free (currently **1152 × 864**) | none | prefix with `!`, e.g. `!RM_Cemetery.png` | `!` means "do not tile"; this is the base image for a whole map |
| Full-screen picture (item close-ups and so on) | `game/img/pictures/` | **816 × 624** | none | `RM_<thing>.png` | headstone close-up, keepsake box |
| Title screen | `game/img/titles1/` | **816 × 624** | none | `RM_Title.png` | |
| Map collision layer | `game/img/tilesets/` | a multiple of 48 | 48×48 per cell | `RM_Collision_A5.png` | **do not redraw this**, see section 5 |
| Music | `game/audio/bgm/` | — | — | `RM_<name>` | **must ship as both `.ogg` and `.m4a`** |
| Ambient sound | `game/audio/bgs/` | — | — | `RM_<name>` | same as above |
| Sound effect | `game/audio/se/` | — | — | `RM_<name>` | same as above |

That is the whole list. **There is no separate UI asset folder** and you do not need to make a window skin (section 5).

---

## 2. Character sprites: 3 columns × 4 rows

Take `$RM_Grace.png` (144 × 192) as the example, see [`specs/sprites_grace.png`](specs/sprites_grace.png):

```
row 1  facing down   →  frame 1   frame 2   frame 3
row 2  facing left   →  frame 1   frame 2   frame 3
row 3  facing right  →  frame 1   frame 2   frame 3
row 4  facing up     →  frame 1   frame 2   frame 3
```

- **The row order is fixed: down, left, right, up.** You cannot swap it.
- **The middle frame of each row is the standing pose**, used whenever the character stands still. The walk animation cycles 1→2→3→2.
- Cells are 48×48, so a 3×4 sheet is 144×192. One extra pixel and the cells bleed into each other.
- Do not lose the `$` prefix. Without it the game treats the sheet as holding 8 characters and cuts the sprite into one large block.

**A multi-character sheet** (`RM_Family.png`, 576 × 384) holds 4 characters across and 2 rows, 8 in total. Each one is laid out exactly like a single sheet, just side by side. Every character takes 144×192. Right now Mom uses the second one (index 1), Dad the first (index 0) and Grandma the third (index 2).

---

## 3. Character face sets: 4 columns × 2 rows, indexes 0–7

The portrait on the left of a message box. The file is 576 × 288, cut into 8 cells of 144×144:

```
top row      0    1    2    3
bottom row   4    5    6    7
```

**Numbering runs left to right, top to bottom.** The script refers to these numbers, so the order of the expressions matters — change the expression in one cell and the matching lines in the game change with it.

The real reference sheet for each character (grey box = drawn but not used yet, green box = already used by the script):

| Character | File | Reference sheet | Expressions in use |
|---|---|---|---|
| Grace (protagonist) | `RM_Face_Grace.png` | [view](specs/faces_grace.png) | 2, 3, 5, 7 |
| Mom | `RM_Face_Mom.png` | [view](specs/faces_mom.png) | 1, 2, 3, 6 |
| Dad | `RM_Face_Dad.png` | [view](specs/faces_dad.png) | 1, 2, 4, 5, 6, 7 |
| Grandma | `RM_Face_Grandma.png` | [view](specs/faces_grandma.png) | 2, 4 |
| Grandpa | `RM_Face_Grandpa.png` | [view](specs/faces_grandpa.png) | 6 |
| Aunt | `RM_Face_Aunt.png` | [view](specs/faces_aunt.png) | 2 |
| Uncle James | `RM_Face_UncleJames.png` | [view](specs/faces_unclejames.png) | 0 |

> **Changing an expression in a grey box means changing an image nobody uses yet.** Changing one in a green box changes every line in the game that uses that expression. Say so in the group chat before you start.

---

## 4. Scenes, items, title

| File | Size | What it is |
|---|---|---|
| `!RM_Cemetery.png` | 1152 × 864 | the whole cemetery base image. Everything visible on the map is in this one image; the map itself only handles collision |
| `!RM_House.png` | 1152 × 864 | the interior base image for the flashback scene, otherwise the same |
| `RM_Gravestone.png` | 816 × 624 | the full-screen close-up shown when you look at the headstone |
| `RM_Box.png` | 816 × 624 | the keepsake box close-up at the ending |
| `RM_Title.png` | 816 × 624 | title screen |
| `icon/icon.png` | 128 × 128 | browser tab and desktop icon |

The two parallax images are **single images spread across the whole screen**, not tiles, so you can draw them however you like as long as the composition works.

---

## 5. Things you do not need to redraw

These come with the engine. Changing them only breaks the game, so leave them alone:

| File | Why you should not touch it |
|---|---|
| `game/img/system/Window.png` | the nine-slice window skin for message boxes and menus; the slice positions are hard-coded in the engine |
| `game/img/system/IconSet.png` | the icon set, one icon per cell, where the position is tied to the item number |
| the tiles in `game/img/tilesets/` | only `RM_Collision_A5.png` and `RM_Empty_B.png` are ours: one draws invisible "walkable / not walkable" marks, the other is deliberately blank. All the map art is in the parallax images |
| `game/img/animations/`, `battlebacks*/`, `enemies/`, `sv_*/` | used by combat. This game has no combat; these are all stock engine files |
| `game/fonts/` | fonts. Changing fonts is a separate job |

The interaction hint at the bottom of the screen (`Enter / Space: ...`) is drawn by code, not an image, and needs no asset.

---

## 6. Two rules you must not break

**1. Do not rename or delete existing files.** The game looks up images by filename string. If you change `$RM_Aunt.png` to `RM_Aunt.png` or `aunt_v2.png`, the game throws no error — it just **silently shows nothing**, which is hard to trace.

**2. Sizes must be exact.** One pixel out and the cells bleed: a face set shows half of the neighbouring expression, a sprite sheet becomes one large block or half a person.

To add new art, use a **new filename**. Do not overwrite the old one.

---

## 7. Handover process

1. **Put it in [`incoming/`](incoming/) first.** Do not drop it straight into `game/`.
2. Name it by the rules below, so anyone can see at a glance what it is and who it is for:
   - Face set: `RM_Face_<character name>.png` → e.g. `RM_Face_Grace.png`
   - Single-character sprite: `$RM_<character name>.png` → e.g. `$RM_Grace.png`
   - Scene / item: `RM_<thing name>.png` → e.g. `RM_Gravestone.png`
   - Audio: `RM_<purpose>`, both `.ogg` and `.m4a` → e.g. `RM_DoorCreak.ogg` + `RM_DoorCreak.m4a`
3. Say "handed in XX" in the group chat, and say which line or scene the image replaces if you can.
4. Whoever owns the project copies it into the right folder under `game/`, **then must play through to confirm it** (asset changes do not take effect on their own; the game has to reload).

**Tell the script writer about any new filename too.** A new face set is invisible in the game until someone wires it into a message event.

---

## 8. Full checklist for adding a new character

1. Draw a 144×192 sprite sheet, put it in `game/img/characters/`, filename `$RM_<character>.png`
2. Draw a 576×288 face set, put it in `game/img/faces/`, filename `RM_Face_<character>.png`
3. Open `game/Game.rpgproject` in RPG Maker MV
4. Create a new event on the map and pick `$RM_<character>` as the image
5. Insert a "Show Text" command in the event, pick `RM_Face_<character>` as the face, and fill in the index number
6. Test: **start from New Game**, do not load an old save
7. Confirm you do not see "one large block", "half a face" or "the neighbouring expression bleeding in"

(To find out how to build the event and which index to use, ask the script writer or read [`../docs/GAME_REFERENCE.md`](../docs/GAME_REFERENCE.md).)

---

## 9. Self-check before handover

- [ ] the file is in the right folder
- [ ] the size matches the table above (sprites 144×192, face sets 576×288, pictures 816×624)
- [ ] the background is a transparent PNG (face sets and sprites must be)
- [ ] the `$` and `!` prefixes are not missing
- [ ] no existing file has been renamed
- [ ] audio has both the `.ogg` and the `.m4a`
- [ ] you have actually looked at the result in the game
