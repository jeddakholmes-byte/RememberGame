**English** · [简体中文](README.zh-CN.md)

# New asset submission area incoming/

Put finished art here first. **Do not drop it straight into `game/`.** This does two things: `game/` always stays a clean, working build, and everyone can see who handed in what.

## Naming

| Asset | Naming | Example |
|---|---|---|
| Face set | `RM_Face_<character name>.png` | `RM_Face_Grace.png` |
| Single-character sprite | `$RM_<character name>.png` | `$RM_Grace.png` |
| Scene / item | `RM_<thing name>.png` | `RM_Gravestone.png` |
| Music / sound effect | `RM_<purpose>`, both `.ogg` and `.m4a` | `RM_DoorCreak.ogg`, `RM_DoorCreak.m4a` |

The hard requirements for sizes and grids are in the parent folder's [`../README.md`](../README.md). **Follow the sizes when you submit too** — an image with the wrong size cannot go into the game.

## When you are replacing a version

If the new asset is meant to **replace** an existing version (say you redrew `$RM_Grace.png`), put the date in the filename and do not overwrite the old file:

```
$RM_Grace_20261005.png
```

That way the old version is still there, so you can compare against it and roll back to it.

## After you hand it in

Say in the group chat which asset you handed in and which line or scene it replaces or adds. Whoever owns the project copies it into `game/`, then plays through to confirm.

**Note:** putting a new face set into `game/img/faces/` does not make it show up in the game — someone still has to point a message event at it. So tell the script writer about new art at the same time.
