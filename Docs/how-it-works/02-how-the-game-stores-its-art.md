# 2. How the original game stores its art, animation and settings

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../start/disclaimer.md).*

> **In simple terms.** The original game ships almost everything inside one archive file. Pictures live on a few large
> sheets (like sticker sheets). Characters are not stored as movies; they are stored as *recipes* that say, for every
> animation frame, which sticker goes where. The port reads those recipes directly instead of re-drawing the animations
> by hand, which is why the motion looks like the original.

## 2.1 The archive

The iOS release keeps its game data in `DinoCapData.dat` (about 12.9 MB). It is a plain ZIP archive. Unpacking it yields
(counts from the extracted corpus):

| Kind | Count | What it is |
|---|---|---|
| `.a2b` | 67 | Animation files for characters, weapons and effects |
| `.c2b` | 32 | Companion animation files that are merged onto an `.a2b` (for example the house enter/exit motion) |
| `.png` | 41 | Texture sheets and loose images |
| `.wav` | 32 | Sound effects and music |
| `.prop` | 6 | Small XML text files with game numbers (for example dinosaur stats) |
| `.ttf` | 1 | The game font |

The original files are never modified by this project. Everything the port uses is *derived* from them.

## 2.2 Game numbers are plain text

Dinosaur strength is data, not code. A `.prop` file lists each dinosaur type with six numbers:

```xml
<Dinosaur type="1">
  <HP>5</HP> <SPD>50</SPD> <EXP>3</EXP> <FRQ>4</FRQ> <ATK>10</ATK> <ADIS>100</ADIS>
</Dinosaur>
```

`HP` is health, `SPD` walking speed, `EXP` reward, `FRQ` how often that type appears, `ATK` bite damage and `ADIS` the
distance at which it starts to bite. Which of these numbers actually *matters* in play is a separate question; it was
answered by following the code that reads them (see the fidelity chapter), not by the names alone.

## 2.3 Sticker sheets (the atlas)

Pictures are packed on a handful of large PNG sheets. The game finds a picture by *name*: it takes a reference such as
a body-part name, appends the word `Frame`, and looks that name up in a table. The table gives the rectangle on the sheet.

Key facts, all recovered from the shipped program rather than guessed:

- The table is **not a file**. The game builds it at start-up from 822 hard-coded "create frame" calls
  (plus 43 "create texture" calls) spread over 12 builder functions.
- Every entry is a plain, axis-aligned rectangle. No rotation, no trimming, no offsets. The phone and the iPad use
  different sheets but the same names.
- Of the 161 distinct names used by the animation files, 147 resolve to an exact rectangle on both platforms,
  4 are numbered sequences, and 10 are intentionally absent (they are drawn by code, such as some weapon and effect
  layers). None are ambiguous.

## 2.4 Animation files are recipes

An `.a2b` file stores, for each frame of each clip, a list of *parts*. Each part has a name, a picture reference,
a position, a size, a rotation-like value and a colour tint. Playing a clip means: show frame 0's parts, wait the clip's
frame time, show frame 1's parts, and so on.

Details that the port reproduces because the original does them:

- A clip advances at most **one frame per update**, and the timer resets to zero when it does.
- The frame is **drawn first**, then time is added and the frame advances. A clip that starts above frame 0 therefore
  shows its starting frame for exactly one update before moving on.
- Parts are drawn in the order they are stored.
- A part is matched to a body-part object by **position in the list**, not by name.
- A `.c2b` file is merged onto an `.a2b` by *adding* position values and *copying* size, picture and tint.

## 2.5 What the port stores

The Unity project does not contain the original binary files at run time. A converter turned the recipes into one
neutral data file (`combat_manifest.json`) holding **140 animations, 21 patches and 232 atlas entries**, and the game
code plays those recipes with its own player that follows the rules above. Recovering the format first, proving it by
rebuilding the original bytes exactly (99 of 99 test files round-tripped byte-for-byte), and only then converting to
Unity was a deliberate order: it keeps "what the format means" separate from "how Unity displays it".

## 2.6 Evidence pointers

- Registry contracts: `a2b.*`, `animation.*`, `gamesprite.quad_pipeline`, `casprite.pivot.16_20`.
- Reports: Task010 (format reconnaissance), Task017 (extractor and round-trip), Task019 (atlas registry).
- Confidence: the mechanisms above are PROVEN from the shipped code. The exact graphics-library state the original used
  for some sheets is covered in chapter 3.
