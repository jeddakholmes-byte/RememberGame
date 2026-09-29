# Game reference

The map to the game's insides. Read this before changing an event.

Everything below is derived from `game/data/Map001.json`, `Map002.json`, `System.json` and `Tilesets.json`. If you change the game, change this file too — otherwise it stops being true.

---

## 1. At a glance

| | |
|---|---|
| Title | `RememberMe` |
| Engine | RPG Maker MV 1.6.1 |
| Screen | 816 × 624 (`Community_Basic` plugin) |
| Player | Grace, actor 1, sprite `$RM_Grace`, face `RM_Face_Grace` frame 3 |
| Map 1 | `Act1_Cemetery` — 24 × 18, parallax `!RM_Cemetery`, BGM `RM_Act1_Cemetery`, ambience `RM_Rain` |
| Map 2 | `Act1_House_Flashback` — 24 × 18, parallax `!RM_House`, BGM `RM_Flashback`, ambience `RM_Room` |
| Tileset | `Act1_TransparentCollision` — an invisible collision layer; the art is the parallax |
| Start position | Map 1, (12,12), facing down |
| Switches | 8, all named |
| Variables | 1 (`0001`, the old-grave counter) |
| Total event commands | 508 across both maps |
| Combat | none |

---

## 2. Map 1 — `Act1_Cemetery` (`data/Map001.json`)

20 events. Positions are tile coordinates.

| # | Event | Pos | Trigger | What it does | Pages |
|---|---|---|---|---|---|
| 1 | `OpeningController` | (0,0) | **Autorun** | The cold open. Fades out, sets player followers on, forbids saving, starts the rain ambience, then plays Grace's three opening lines, turns the rain on, fades in, starts the cemetery BGM at volume 32, lets Mom speak, walks Mom two steps, shows the CONTROLS card, sets switch 1 ON, forbids the menu. | p1 autorun (the whole scene), p2 action-button and empty once switch 1 is ON |
| 2 | `Mom` | (13,12) | Action button | First talk: the four-beat exchange ("Mom?" → "Are we going home soon?" → "Where is Grandma now?") then "You can look at Grandma's stone. I'll be right here." Sets self-switch A so she only says the short line afterwards. Page 2 is an autorun that fires when switch 6 (`MomCall`) is ON and Grace has not yet seen the stone — Mom calls her over. | p1 talk, p2 autorun |
| 3 | `Dad` | (14,7) | Action button | Page 1: Grace says "Dad?", Dad says "…", Grace narrates "He's looking at Grandma's stone." Page 2 (after switch 2): the real conversation — Dad turns, "Hey, Gracie.", "Are you crying?", "A little.", "Why?", "Because I miss Grandma." — then the door creak, menu restored, fade, switch 3 ON, transfer to Map 2 at (12,13) facing right. Page 3 is an empty post-flashback page. | p1, p2, p3 |
| 4 | `Gravestone_Main` | (11,6) | Action button | The headstone close-up. Shows `RM_Gravestone` full screen, plays `RM_Chime`, then Grace's five lines about the name and the photograph. Sets switch 2 ON (`VisitedGrave`). Dad turns and wipes his face. | p1 first read, p2 short re-read |
| 5 | `WhiteFlowers` | (10,6) | Action button | Flowers around the main stone. Long version the first time (switch 8 OFF), one-line version after. Sets switch 8 ON. | p1 |
| 6 | `OldGrave_WitheredFlowers` | (5,10) | Action button | "I don't know this person. The flowers here seem withered." Counts the grave. | p1 |
| 7 | `UncleJames` | (8,9) | Action button | One line: "Take your time, Grace." | p1 |
| 8 | `Aunt` | (17,10) | Action button | One line, no name: "She holds a tissue in both hands." | p1 |
| 9 | `Bench_RelicBox` | (5,13) | Action button | "Mom's bag is by the bench. There is a little box, too." | p1 |
| 10 | `AfterFlashback_EndAct` | (1,0) | **Autorun** | The finale, and the last thing in Act I. See §4. | p1 empty, p2 the ending, p3 empty once switch 5 is ON |
| 11 | `ExitLeft` | (11,16) | Player touch | Blocks the gate: Mom says "Stay inside the gate, Grace." and pushes Grace back down. | p1 |
| 12 | `ExitRight` | (12,16) | Player touch | Same as 11. | p1 |
| 13 | `Gravestone_Right` | (12,6) | Action button | A second approach tile for the same headstone. Identical text to event 4. | p1, p2 |
| 14 | `Ambience` | (0,0) | **Parallel** | The sound bed: waits ~15 s, plays `RM_WindGust`; waits ~20 s, `RM_CrowCaw`; waits ~25 s, `RM_WindGust`. Loops once switch 1 is ON. | p1 empty, p2 parallel loop |
| 15 | `Bench_RelicBox` | (4,13) | Action button | Second approach tile for the bench. Same text as event 9. | p1 |
| 16 | `OldGrave_3` | (5,4) | Action button | "There is nothing here." Counts the grave. | p1 |
| 17 | `OldGrave_4` | (18,4) | Action button | "This grave… It seems to be abondoned a long time ago." Counts the grave. **The typo `abondoned` is in the shipped text** — the planned polish pass fixes it, but it is still in `game/`. | p1 |
| 18 | `WhiteFlowers` | (13,6) | Action button | Second flower cluster. Identical to event 5 and shares switch 8. | p1 |
| 19 | `OldGrave_2` | (18,10) | Action button | "I can barely read the name on it." Counts the grave. | p1 |
| 20 | `Grandpa` | (16,7) | Action button | Added during the character pass. Sprite `$RM_Grandpa`, facing left, face `RM_Face_Grandpa` index 6, one narration line: "Grandpa is standing very still." No switch. | p1 |

Two pairs are deliberate duplicates so Grace can interact from either side: events 4/13 (headstone) and 9/15 (bench). If you change one, change its twin.

---

## 3. Map 2 — `Act1_House_Flashback` (`data/Map002.json`)

3 events. The map is a single scripted scene, not a place you explore.

| # | Event | Pos | Trigger | What it does |
|---|---|---|---|---|
| 1 | `FlashbackController` | (0,0) | **Autorun** | The whole flashback and the transfer back. Runs while switch 3 (`FlashbackActive`) is ON. See §4. |
| 2 | `Grandma` | (12,6) | Action button | Sprite `RM_Family` frame 2, facing up. No commands — she is moved by the controller. |
| 3 | `FlashbackDad` | (11,7) | Action button | Sprite `RM_Family` frame 0, facing up. No commands — moved by the controller. |

The house interior is the `!RM_House` parallax; the camera position is set in script with `$gameMap.setDisplayPos(3.5, 3)`.

---

## 4. The two scripted set pieces

### 4.1 `FlashbackController` (Map 2, event 1)

Runs as one continuous autorun:

1. Darken the screen to a cold tint, `setDisplayPos(3.5, 3)`, start `RM_Room` (BGS, volume 16) and `RM_Flashback` (BGM, volume 22), fade in.
2. Dad takes one step, wait 24 frames.
3. Grandma: *"I need to go home."* Dad: *"Mom. You are home."*
4. Dad steps left. Door handle SFX. Grandma: *"No."* Grandma moves; Dad: *"We talked about this."*
5. Two door-handle hits, Grandma: *"Grace?"*
6. Dad: *"Mom, please—"* Door palm, wait, **screen shake**, `RM_KnockHard` at volume 62.
7. Grandma: *"Let me go home!"* **Screen shake again**, `RM_KnockHard` again, wait.
8. Dad: *"I CAN'T DO THIS AGAIN."*
9. Fade BGM and BGS, stop SE, wait, Dad steps away with `RM_Step1`, fade out.
10. Switch 3 OFF, switch 4 ON, transfer back to Map 1 at (13,7).

Steps 6 and 7 — the two screen shakes and the two hard knocks — are the ones the planned polish pass removes. They are still here.

### 4.2 `AfterFlashback_EndAct` (Map 1, event 10, page 2)

Autoruns once switch 4 (`FlashbackComplete`) is ON:

1. Menu restored, screen tinted back, rain and `RM_Rain` restarted, camera scrolls to Dad, fade in, cemetery BGM at volume 45.
2. Dad: *"Grace?"* … *"You okay?"* Grace: *"… Yeah."* Dad steps away.
3. The turn of the chapter — Grace: *"Dad got angry with Grandma sometimes."* / *"Grandma isn't here anymore. And Dad is crying."* / *"I don't understand."*
4. Mom arrives: *"Grace. We're going home now."* Then `RM_Box` starts, Mom: *"I brought this from Grandma's house. Your name is on it."*
5. Show picture `RM_Box`, `RM_Cardboard` + `RM_Swell`, Mom: *"For Grace."* / *"We found it with her things. Maybe she wanted you to have it."*
6. Grace: *"Can I open it?"* Mom: *"When we get home. Let's keep it dry."*
7. Fade out, erase the picture, two closing narration lines, then the card: **`ACT I — END` / `The things Grandma left behind.`**
8. Switch 1 OFF (so the ambience stops), fade everything, `code 354` (wait for the fade to finish), switch 5 ON.

---

## 5. Switches

All eight are named in `System.json`. A page marked *"requires ON"* only becomes the active page when that switch is ON; if several pages qualify, **the highest page number wins**.

| # | Name | Set ON by | Read by | Meaning |
|---|---|---|---|---|
| 1 | `OpeningComplete` | `OpeningController` p1 at the end; `AfterFlashback_EndAct` p2 | `OpeningController` p2, `Ambience` p2 | The cold open has finished. Keeps the ambience loop running for the whole chapter, and stops it at the end. |
| 2 | `VisitedGrave` | `Gravestone_Main` p1, `Gravestone_Right` p1 | `Dad` p2 (page condition), `Mom` p2, both headstone p2 pages | Grace has read Grandma's headstone. **This is the gate that unlocks the flashback** — Dad will not open up until it is ON. |
| 3 | `FlashbackActive` | `Dad` p2, immediately before the transfer | `FlashbackController` p1 (autorun); cleared by that event | "We are inside the flashback." |
| 4 | `FlashbackComplete` | `FlashbackController` p1 at the end | `AfterFlashback_EndAct` p2 (autorun), `Dad` p3, `FlashbackController` p2 | The flashback has been played. |
| 5 | `Act1Complete` | `AfterFlashback_EndAct` p2 | `AfterFlashback_EndAct` p3 | Act I is over. Stops the ending from re-running. |
| 6 | `MomCall` | `OldGrave_WitheredFlowers`, `OldGrave_2`, `OldGrave_3`, `OldGrave_4` — any one of the four | `Mom` p2 (autorun), `Gravestone_Main` p2, `Gravestone_Right` p2, `Mom` p2 clears it | The player has looked at enough side graves, so Mom calls Grace over to the real one. |
| 7 | `MomHintDelivered` | `Mom` p2 | `Mom` p2 | One-shot guard so Mom's "Grandma's stone is near Dad" line plays only once. |
| 8 | `FlowerSeen` | both `WhiteFlowers` events | both `WhiteFlowers` events | Shared by the two flower clusters so the long version plays once and the short version plays after. |

### Variable

| # | Name | Meaning |
|---|---|---|
| 1 | `0001` | Counter of old graves inspected. Incremented by `OldGrave_WitheredFlowers`, `OldGrave_2`, `OldGrave_3`, `OldGrave_4`. When it reaches 4, those events stop setting `MomCall`. |

The counter and switch 6 do overlapping jobs: switch 6 makes Mom call Grace after the *first* old grave; the counter caps the behaviour at four.

---

## 6. Walkthrough

1. The chapter opens on a fade-in. Grace's narration, then Mom's warning, then a CONTROLS card.
2. **Talk to Mom** (13,12) — optional but it establishes "Where is Grandma now?".
3. **Read Grandma's headstone** (11,6) or (12,6). This sets `VisitedGrave`. Optionally inspect the two flower clusters and the four old graves first; after any old grave, Mom calls Grace over.
4. **Talk to Dad** (14,7). The full conversation only plays after step 3. It ends with a door creak and a transfer.
5. **The flashback** plays itself. No input needed.
6. **Back in the cemetery**, the ending autorun: Dad asks if Grace is okay, Grace's realisation, Mom arrives with the box, `ACT I — END`.
7. Grace can talk to Uncle James (8,9), Aunt (17,10), Grandpa (16,7) and inspect the bench (4,13 / 5,13) at any point.

Minimum path: headstone → Dad. Everything else is texture.

---

## 7. Where the dialogue lives

| Speaker | Face sheet | Boxes |
|---|---|---|
| Grace | `RM_Face_Grace` | 46 |
| Mom | `RM_Face_Mom` | 14 |
| Dad | `RM_Face_Dad` | 10 |
| Grandma | `RM_Face_Grandma` | 4 |
| Uncle James | `RM_Face_UncleJames` | 1 |
| Aunt | `RM_Face_Aunt` | 1 |
| Grandpa | `RM_Face_Grandpa` | 1 |
| Narrator (no face) | — | 4 |

The complete in-game text, in order, is listed in [../script/ACT1_SCRIPT.md](../script/ACT1_SCRIPT.md). That file is generated from the map data, so it is the truth about what the player actually reads.

Grace's lines use the colour code `\C[4]`, Mom `\C[6]`, Dad `\C[1]`, Grandma `\C[5]`, Uncle James `\C[3]`, and narration `\C[7]`. The name tag is a separate line, written as `\C[4]GRACE\C[0]`.

---

## 8. Recipes

**Change a line of dialogue.** MV editor → Map 1 → double-click the event → select the page → edit the text in the Show Text box. Never edit `Map001.json` by hand while the editor is open; the editor will overwrite your change on save.

**Add an interaction hint.** Not wired up yet. `RM_Act1_Experience.js` reads `<RMHint: your text>` from an event's **Note field** (the top-right box in the event editor) — not from a Comment command. Then enable the plugin. Both steps are outstanding.

**Add a new character to the map.** Drop the sprite into `game/img/characters/` (144 × 192 for a `$`-prefixed single character), the face sheet into `game/img/faces/` (576 × 288), create an event, set its image, and set the face name plus an index from 0 to 7 at each Show Text box.

**Move the player's start point.** MV editor → right-click Map 1 → *Edit Map* → set the player start. The current start is (12,12).

**Test a switch change.** Always start a **New Game**. A save from the middle of the chapter will restore the old switch states and make your change look broken.
