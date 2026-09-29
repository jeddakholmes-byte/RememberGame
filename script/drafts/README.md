**English** · [简体中文](README.zh-CN.md)

# Script drafts drafts/

Put newly written script here first. For format, naming and voice rules, see the parent folder's [`../README.md`](../README.md).

## Naming

```
ACT2_<scene name>_script.md        e.g. ACT2_Attic_script.md
```

One file per scene. Do not have several people writing into the same file.

## How to write a change to an existing line

Do not edit `../ACT1_SCRIPT.md` directly (it is exported from the project and any change is overwritten). Create a new file here and write "original / changed to" clearly:

```markdown
# Change: the typo in Act I OldGrave_4

## Event
Map001 → 017 OldGrave_4 (coordinates 18,4)

## Original
It seems to be abondoned a long time ago.

## Changed to
It looks like no one has been here
for a long time.

## Reason
Fix the `abondoned` spelling; also closer to a child's concrete observation.
```

Include the **event name and coordinates**, so whoever builds events can find the place.

## Once it is final

One person merges the draft into the project (into the RPG Maker MV events), then **plays through from New Game** to confirm the lines appear in the right place and in the right order. After that merge, data-exported files such as `../ACT1_SCRIPT.md` have to be exported again, so the text in the repository does not drift from the game.
