# Act I — in-game script

> Generated from `game/data/Map001.json` and `Map002.json`. This is the text the player
> actually reads. It is a reading copy, not the source of truth — if it disagrees with the map
> data, the map data wins. Branch labels come from the Conditional Branch commands in the maps.

---

## Map 1 — Act1 Cemetery

Events are listed in map order, not the order the player reaches them.

### Event 01 · OpeningController · (0,0)

- **GRACE** — Everyone wore black today. Mom wore black. Dad wore black.
- **GRACE** — Even Uncle James wore a tie. He never wears ties.
- **MOM** — Stay where I can see you, okay?
- **GRACE** — Okay.
- **MOM** — Grandma's name is on that stone. You can go and look. I'll be right here.
- *(narration)* — CONTROLS ↑↓←→: Move ENTER / SPACE: Look or talk ESC: Pause / Save

### Event 02 · Mom · (13,12)

*if self switch A is OFF*

- **GRACE** — Mom?
- **MOM** — Mm?
- **GRACE** — Are we going home soon?
- **MOM** — In a little while.
- **GRACE** — Where is Grandma now?
- **MOM** — I don't know, sweetheart.
- **GRACE** — Oh.
- **MOM** — You can look at Grandma's stone. I'll be right here.
*otherwise (not: self switch A is OFF)*

- **MOM** — I'll be right here.
*Page 2*

*if switch 2 VisitedGrave is OFF › if switch 7 MomHintDelivered is OFF*

- **MOM** — Grandma's stone is near Dad. I'll be right here.

### Event 03 · Dad · (14,7)

- **GRACE** — Dad?
- **DAD** — ...
- **GRACE** — He's looking at Grandma's stone.
*Page 2*

- **GRACE** — Dad?
- **GRACE** — Dad.
- **DAD** — Hey, Gracie.
- **GRACE** — Are you crying?
- **DAD** — A little.
- **GRACE** — Why?
- **DAD** — Because I miss Grandma.
- **GRACE** — Oh.

### Event 04 · Gravestone_Main · (11,6)

- **GRACE** — That's Grandma's name. Mom taught me to read it.
- **GRACE** — She is home now.
- **GRACE** — Her picture doesn't look like her. She looks younger.
- **GRACE** — She knew my name then.
- **GRACE** — Dad's wiping his face.
*Page 2*

- **GRACE** — She is home now.

### Event 05 · WhiteFlowers · (10,6)

*if switch 8 FlowerSeen is OFF*

- **GRACE** — There are so many flowers.
- **GRACE** — Grandma didn't like strong smells. Flowers made her sneeze.
- **GRACE** — I don't remember when she stopped saying that.
*otherwise (not: switch 8 FlowerSeen is OFF)*

- **GRACE** — Flowers made her sneeze.

### Event 06 · OldGrave_WitheredFlowers · (5,10)

- **GRACE** — I don't know this person. The flowers here seem withered.

### Event 07 · UncleJames · (8,9)

- **UNCLE JAMES** — Take your time, Grace.

### Event 08 · Aunt · (17,10)

- *(narration)* — She holds a tissue in both hands.

### Event 09 · Bench_RelicBox · (5,13)

- **GRACE** — Mom's bag is by the bench. There is a little box, too.

### Event 10 · AfterFlashback_EndAct · (1,0)

- **DAD** — Grace?
- **DAD** — You okay?
- **GRACE** — ... Yeah.
- **GRACE** — Dad got angry with Grandma sometimes.
- **GRACE** — Grandma isn't here anymore. And Dad is crying.
- **GRACE** — I don't understand.
- **MOM** — Grace. We're going home now.
- **MOM** — I brought this from Grandma's house. Your name is on it.
- **GRACE** — For Grace.
- **MOM** — We found it with her things. Maybe she wanted you to have it.
- **GRACE** — Can I open it?
- **MOM** — When we get home. Let's keep it dry.
- **GRACE** — Grandma left something for me.
- **GRACE** — I wonder what's inside.
- *(narration)* — ACT I — END The things Grandma left behind.

### Event 11 · ExitLeft · (11,16)

- **MOM** — Stay inside the gate, Grace.

### Event 12 · ExitRight · (12,16)

- **MOM** — Stay inside the gate, Grace.

### Event 13 · Gravestone_Right · (12,6)

- **GRACE** — That's Grandma's name. Mom taught me to read it.
- **GRACE** — She is home now.
- **GRACE** — Her picture doesn't look like her. She looks younger.
- **GRACE** — She knew my name then.
- **GRACE** — Dad's wiping his face.
*Page 2*

- **GRACE** — She is home now.

### Event 15 · Bench_RelicBox · (4,13)

- **GRACE** — Mom's bag is by the bench. There is a little box, too.

### Event 16 · OldGrave_3 · (5,4)

- **GRACE** — There is nothing here.

### Event 17 · OldGrave_4 · (18,4)

- **GRACE** — This grave... It seems to be abondoned a long time ago.

### Event 18 · WhiteFlowers · (13,6)

*if switch 8 FlowerSeen is OFF*

- **GRACE** — There are so many flowers.
- **GRACE** — Grandma didn't like strong smells. Flowers made her sneeze.
- **GRACE** — I don't remember when she stopped saying that.
*otherwise (not: switch 8 FlowerSeen is OFF)*

- **GRACE** — Flowers made her sneeze.

### Event 19 · OldGrave_2 · (18,10)

- **GRACE** — I can barely read the name on it.

### Event 20 · Grandpa · (16,7)

- *(narration)* — Grandpa is standing very still.

---

## Map 2 — Act1 House Flashback

One scripted autorun scene; nothing here needs input.

### Event 01 · FlashbackController · (0,0)

- **GRANDMA** — I need to go home.
- **DAD** — Mom. You are home.
- **GRANDMA** — No.
- **DAD** — We talked about this.
- **GRANDMA** — Grace?
- **DAD** — Mom, please—
- **GRANDMA** — Let me go home!
- **DAD** — I CAN'T DO THIS AGAIN.

