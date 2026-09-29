# Review of the 2026-09-29 enhancement package

This is a read-only review of `enhancement/` — what it contains, what it asks for, what has already been applied to `game/`, what has not, and where its own instructions are weak. **Nothing in `game/` or `enhancement/` was changed while producing this repo.**

Every claim below was checked against the delivered files. The comparison baseline is:

| Label | Path | What it is |
|---|---|---|
| **ORIGINAL** | `RememberMeAct1Assets/data` | the first draft, before the enhancement |
| **GAME** | `game/data` | the build that ships in this repository |
| **TARGET** | `enhancement/patch_data` | what the enhancement package says the result should be |

---

## 1. What the package contains

Six instruction documents, a plain-text duplicate of them, an English changelog, three folders of deliverables and a folder of checking records.

| Path | Role |
|---|---|
| `00_从这里开始.md` | Master index. Describes two routes — edit by hand, or import the patched data files — and records that the package never wrote to the original project. **Read this first.** |
| `01_人物素材导入.md` | New art for Uncle James, Aunt and Grandpa: file names, grid geometry, face indices, event wiring. |
| `02_闪回与结尾逐步修改.md` | The flashback and ending rewrite. The longest and the riskiest document. |
| `03_游玩体验逐项设置.md` | Volume, menu/save access, the CONTROLS card, Mom's hint gating, the shared flower switch, two text fixes, the optional experience plugin, title BGM, UI terms. |
| `04_更换英文字体.md` | Why the window font kept falling back, and two ways to fix it. |
| `05_另一台电脑验收与排错.md` | A 16-item acceptance checklist plus a symptom → cause table. |
| `06_事件指令对照.md` | A readable listing of the *modified* events. Explicitly "not pasteable" — a reference, not importable data. |
| `CHANGELOG_EN.md` | One-page English summary. |
| `教程_纯文本.txt` | 00–06 concatenated for readers without a Markdown viewer. It inherits every defect of its sources, so fix in both places. |
| `assets/` | The art to merge in, the two plugin JS files, and a corrected `gamefont.css`. |
| `patch_data/` | `Map001.json`, `Map002.json`, `System.json`, `Actors.json` — the target state. |
| `generation/` | Source AI portraits, the prompts, a custom-font CSS example. |
| `review/` | Bounding boxes, 514 static checks, a browser playthrough log, SHA-256 fingerprints of every source file. **Historical evidence only** — `00` says so explicitly. |
| `tools/` | The build and check scripts. Not needed to apply the changes. |

---

## 2. The prescribed procedure, in order

1. **Work on a copy.** Close RPG Maker MV, duplicate the project as `RememberMeAct1_Enhanced`, open only that copy.
2. **Merge the art** into `img/faces` and `img/characters`. Never rename the files, and never drop the `$` prefix from a walk sheet.
3. **Wire in the characters** (01): Uncle James at (8,9), Aunt at (17,10) — the event may be renamed from `Relative` to `Aunt` — and a brand-new `Grandpa` event at (16,7), facing left, one narration line, no switch.
4. **Rebuild the flashback and the ending** (02): rename switches 0001–0008, retarget Dad's transfer to Map002 (12,13) facing right with no fade, restage Grandma and Dad, rebuild `FlashbackController` page 1 from the table, and rewrite the ending so Mom walks over instead of the screen cutting to black.
5. **Tune the play experience** (03): opening BGM to 32, menu access rules, the four-line CONTROLS card, Mom's one-time long conversation, Mom's one-time hint, the shared flower switch, the two grave-text fixes.
6. **Install the optional plugins** (03 §六, 04): `RM_Act1_Experience` above `RM_EnglishTypography`, both after `Community_Basic`; make sure `RM_Act1_Polish` stays **off**. Add `<RMHint: …>` text to the **Note** field of 15 events.
7. **System settings** (03 §七): title BGM to 25, optionally switch off `MadeWithMv`, translate the player-visible Terms to English.
8. **Verify** (05). Always start from **New Game** — a mid-chapter save restores old switch states and will make correct work look broken.

Two warnings worth repeating, because they are the ways people break this project:

- **Never enable both `RM_Act1_Polish` and `RM_Act1_Experience`.** They both add footstep sounds and you will hear them twice.
- **`<RMHint: …>` goes in the event's Note field, not in a Comment command.** A Comment looks right in the editor and silently does nothing.

---

## 3. What is already in `game/` — verified

Measured by comparing the three data sets field by field. "Applies" means GAME already matches TARGET.

| Enhancement item | ORIGINAL | GAME | TARGET | Applies? |
|---|---|---|---|---|
| Switch names 0001–0008 | 2 of 8 named | all 8 named | all 8 named | yes |
| Variable 0001 named `OtherGravesInspected` | unnamed | **unnamed** | named | **no** |
| Grandpa event at (16,7) | absent | present, `$RM_Grandpa` | present | yes |
| Uncle James sprite | `RM_Family` #3 | `$RM_UncleJames` | `$RM_UncleJames` | yes |
| Uncle James face index | — | **0** | **7** | **no** |
| Aunt sprite | `RM_Family` #4 | `$RM_Aunt` | `$RM_Aunt` | yes |
| Aunt sprite stance pattern | 1 | **0** | 1 | **no** |
| Grandpa face index | — | **6** | **2** | **no** |
| CONTROLS card in the opening | absent | present | present | yes |
| Opening BGM volume | 50 | 32 | 32 | yes |
| Mom's long conversation once only (self-switch A) | absent | present | present | yes |
| Mom's hint gating on 0002 / 0007 | absent | present | present | yes |
| Both flower events share switch 0008 | absent | present | present | yes |
| Dad's transfer → Map002 (12,13), facing right, no fade | (12,11) up | (12,13) right, no fade | (12,13) right | yes |
| `setDisplayPos(3.5, 3)` camera script | absent | present | present | yes |
| `<RMHint: …>` notes on the events | 0 | **0** | 15 | **no** |
| `RM_Act1_Experience` enabled in `plugins.js` | off | **off** | — | **no** |
| `System.json` locale | `zh_CN` | **`zh_CN`** | `en_US` | **no** |
| `System.json` UI Terms | Chinese | **Chinese** | English | **no** |
| `System.json` `optFollowers` | true | **true** | false | **no** |
| `System.json` title BGM volume | 90 | **90** | 25 | **no** |
| `OldGrave_4` typo `abondoned` fixed | present | **still present** | fixed | **no** |
| `OldGrave_2` text final wording | "The name on the grave is barely legible." | "I can barely read the name on it." | "I can barely read the name." | half |
| Flashback: screen shakes | 3 | **2** | 0 | **no** |
| Flashback: `RM_Heartbeat` uses | 2 | 0 | 0 | yes |
| Flashback: `RM_KnockHard` uses | 2 | **2** | 0 | **no** |
| Flashback: "1 Step Backward" move | absent | **absent** | present | **no** |
| Map002 Grandma position | (12,7) | (12,6) | (12,5) | **no** |
| Map002 FlashbackDad position | (11,8) | (11,7) | (11,6) | **no** |
| Map002 tile (12,4) walkable | blocked | **blocked** | walkable | **no** |

### What this means

`game/` is **a partially completed pass**, not the original draft and not the target. The character work and the staging around Dad's transfer are done; the flashback rebuild, the hint system, and every `System.json` change are not.

Two consequences a player can see right now:

1. **The cemetery and the flashback still run in Chinese UI terms.** `System.json` says `locale: "zh_CN"` and the Terms are the Chinese defaults, while `RM_EnglishTypography.js` is forcing an Arial window font. Latin dialogue renders fine, but any menu or system message is Chinese in a Latin font.
2. **The flashback still shakes twice and knocks twice.** The whole point of `02` was to take the melodrama out of that scene. It is still there.

Two consequences a teammate will hit:

3. **No interaction hints, no footsteps, no simplified menu** — `RM_Act1_Experience.js` is sitting in `js/plugins/` but is not listed in `js/plugins.js`, so the engine never loads it. Turning it on without first adding the `<RMHint:>` notes gives you the menu and the footsteps only.
4. **The flashback staging is in an in-between state.** Grandma sits at (12,6) rather than the original (12,7) or the target (12,5), and tile (12,4) is still blocked. With the current data this is *self-consistent* — her move route only takes her to (12,5), which is walkable — but it matches neither the original layout nor the target, so the enhancement's own acceptance row "Grandma (12,5), Dad (11,6), door tile (12,4) walkable" cannot be used as-is to judge it.

### Do not import `patch_data/` wholesale

`patch_data/Map001.json` and `Map002.json` would overwrite the maps, and `game/` has already diverged from the version those files were built against. Importing them would silently undo the character wiring and the staging work that *is* done. Use `01`–`04` by hand. The only safe whole-file import is `System.json`, and even that replaces the language, the follower setting, the title volume and the switch names together.

---

## 4. Findings on the documents themselves

Only issues that were checked against the files are listed.

**1. `06_事件指令对照.md` does not cover six of the events that need hint notes. (highest impact)**
`03` §六 lists `<RMHint:>` notes for 15 events: 002, 003, 004, 005, 006, 007, 008, 009, 013, 015, 016, 017, 018, 019 and the new Grandpa. `06` — the file advertised as the per-event reference — contains only 001, 002, 003, 005, 007, 008, 010, 017, 018, 019, 020 for Map 1, and its hint strings cover only 7 of the 9 required texts. Missing entirely: `Look at Grandma's stone` (004, 013) and `Look at the bench` (009, 015). A teammate rebuilding from `06` alone will produce a cemetery where two of the interactable objects never show a hint.

**2. Switch 0008 is named two different things.**
`02` §一 and `03` §四 call it `FlowersSeen`. The delivered `patch_data/System.json` says `FlowersSeen`; the build in `game/` says `FlowerSeen`. Pick one and use it everywhere, or a teammate searching for the switch will not find the other spelling.

**3. `02`'s fallback contradicts `05`'s acceptance criteria.**
`02` §三 step 4 offers a fallback: "keep the map as it is, park Grandma at (12,5), cancel the next section's up-step." `05` then requires you to check "Grandma (12,5), Dad (11,6), door tile (12,4) walkable". Following the fallback makes that row fail, and neither document says so.

**4. `00` gives two routes without a decision rule.**
It says "when editing by hand, **do not** copy `patch_data`", and then describes importing `patch_data` wholesale. The only gate is "only if the project is still identical to the version supplied", which nobody can check by eye. Since `game/` has already diverged, the whole-file route is closed for this project — say so in writing.

**5. `04` describes work that is already done.**
Method A steps 5–6 tell you to copy and enable `RM_EnglishTypography.js`. Both are already true in `game/`: the file is in `js/plugins/` and it is switched on in `js/plugins.js` with the Arial / 26 / 2 parameters. Harmless, but it makes the document read as if the font fix is outstanding when it is not.

**6. The plugin order is stated in two places and never together.**
`03` §六 says put `RM_Act1_Experience` after `Community_Basic`. `04` step 7 says put `RM_EnglishTypography` after `RM_Act1_Experience`. The combined order is therefore `Community_Basic → RM_Act1_Experience → RM_EnglishTypography`, but no document writes that line.

**7. One Terms row is worth a second look (not an error).**
`03` §七 maps both `游戏结束 / GAME OVER` and `返回标题` to `Return to Title`, and the delivered `patch_data/System.json` does the same. That is consistent, and defensible for a chapter with no game-over — but it means two different editor slots read identically, so check the in-game menu after applying it.

**8. Minor.** `05` line 37 contains `确認` (traditional form in an otherwise simplified document). `06` shows Uncle James at a face index of 7 while the review record's own asset check only covers two of the six new PNGs, so the index choices for Grandpa and the face sheets rest on `01` alone.

---

## 5. Ways a teammate could break the project

| Mistake | What happens | Avoid it by |
|---|---|---|
| Copying `patch_data/*` over `game/data/*` | Undoes the character wiring and staging already done | Follow 01–04 by hand |
| Copying `assets/` into the project root | Scatters art into the wrong folders | Merge `assets/img`, `assets/js`, `assets/fonts` into the matching folders |
| Replacing the whole `img` or `js` folder | Deletes everything not in the package | Never replace a folder; merge into it |
| Dropping the `$` from `$RM_Aunt.png` etc. | The sprite renders as a giant corner of the wrong frame | `$` means "one character, not eight" |
| Skipping the (12,4) passable tile while using the target staging | A forced move route with *skip = false* can stall the flashback | Either paint the tile or use the fallback and accept that `05` will flag it |
| Putting `0006 OFF` inside the two conditional branches | Mom's autorun repeats forever | It must sit outside both branches |
| Using per-event self-switches for the two flower clusters | The second cluster replays the long version | They share plain switch 0008 |
| Choosing "1 Step Down" instead of "1 Step Backward" | Grace walks into Dad | `02` §四 says this explicitly |
| Writing `<RMHint:>` as a Comment command | No hints ever appear | Use the event's Note field |
| Deleting the wrong blackout in the ending | The final card loses its fade, or the mid-scene cut stays | Only the five mid-scene commands go; keep the last fadeout |
| Enabling `RM_Act1_Polish` alongside `RM_Act1_Experience` | Footsteps play twice | Keep `RM_Act1_Polish` off |
| Testing from an old save | Every switch change looks broken | Always **New Game** |
| Method B: CSS saved as `.txt`, or `CustomFont.ttf.ttf` | The font silently does not load | Check the real file name and extension |

---

## 6. Recommended order to finish it

Do it in this order and save a backup after each stage, so you can tell which stage broke something.

1. **Characters and Grandpa** (01). Mostly done — fix the three face/stance indices listed in §3, and check the four characters with the acceptance test at the end of `01`.
2. **Flashback and ending** (02). The largest remaining job. Rebuild `FlashbackController` page 1 from the `06` table, delete the two shakes and the two `RM_KnockHard`, add the "1 Step Backward", and restage Grandma and Dad.
3. **Font and the experience plugin** (03 §六, 04). Add the 15 `<RMHint:>` notes first — including the six `06` does not list — then enable `RM_Act1_Experience` between `Community_Basic` and `RM_EnglishTypography`.
4. **System settings** (03 §七). `locale`, Terms, title BGM volume, `optFollowers`. Then check the menu in game.
5. **Verify** (05), from New Game.

If there is no time for all of it, `05` nominates the same three priorities: **the character art and Grandpa, the flashback's backwards step and its sound, and the font plugin.** Everything already in `game/` covers a large part of the first of those, so the flashback and the font are the real work.

---

## 7. One thing worth adding to the package

The package has no single "state report" — a file saying which changes are in the build and which are not. That is why this document exists. If the team keeps editing, keep a short status table like §3 up to date in the repository rather than inside the package.
