# Dino Cap 1 — Unity port (PC and Android)

> **Disclaimer.** Dino Cap 1, 2 and 3 were made by Triniti Interactive. Everything else here is AI-reviewed, may change at any time and
> should not be fully trusted. See [the disclaimer](Docs/start/disclaimer.md).

## Play it first

- **[Download guide](Docs/start/download-guide.md)** — get the Windows or Android build from the **Releases** section of this GitHub page (not from the *Code* button).
- **[Controls for PC (mouse and keyboard)](Docs/start/controls-pc.md)** — A/D to move, left click shoot,
  right click melee, wheel to change weapon, E for doors, Esc to pause.
- **Curious how this was figured out?** We didn't guess. We took the original iPad game apart, rebuilt it from what it really does, and checked it against a real iPad. [Here's the whole trail](Docs/findings.md), wrong turns included.


## What this is

A faithful rebuild of Dino Cap 1 (iOS, version 1.9.8) for modern PC and Android, made by studying the original game
rather than guessing. Pages are layered: each starts with an "In simple terms" box, then the engineering detail, then
evidence pointers.

Status: documentation audited batch by batch with findings addressed; builds rebuilt, the full suite run and a clean-checkout build done; Android saving confirmed by the owner. Open items are listed in the [validation matrix](Docs/release/build-validation-matrix.md) and the release notes.


## Build from source

Open the project folder in **Unity 6000.3.8f1** (the Input System package is declared in `Packages/manifest.json`). The four scenes in
`Assets/DinoCap/Scenes` are listed in the build settings. The shipped builds use the editor methods in
`Assets/DinoCap/PortV2/Editor/DC1ConvergenceBuild.cs` (`BuildShippedWindows`: Windows x64, Mono; `BuildShippedAndroid`: Android, IL2CPP,
ARMv7 + ARM64 in one APK), for example:

```
Unity.exe -batchmode -quit -projectPath <this folder> -executeMethod DinoCap.PortV2.EditorTools.DC1ConvergenceBuild.BuildShippedWindows
```

Their output folder is computed relative to the project (a sibling `Dev/Operator/DC1-MASTER/Release` folder that is not part of this repository);
change that path in the script if you build elsewhere. These steps were reproduced from a clean checkout of this repository on the build machine (Windows and Android both built; see [the check](Docs/release/clean-checkout-build.md)); a build on a different computer has not been tried.
The repository includes the game's art and audio as converted from the original (the owner's decision); the reverse-engineering database,
the original IPA and the working reports stay private.
