# Clean-checkout build check

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../start/disclaimer.md).*

> **In simple terms.** To check that the repository really contains everything needed to build the game, the project was cloned into an empty folder and built from there, with none of the working folder's cached files.
> It built, with the same files as the shipped builds. Most files are byte-identical; the differing ones are listed below, and some of them are not fully explained.

**What was done (15 Oct).** `git clone` of this repository (taken before its history was squashed into one commit) into a fresh folder (no `Library`, no sibling folders, none of the archived notes), then Unity 6000.3.8f1 in batch mode with the project's own build methods
(`DC1ConvergenceBuild.BuildShippedWindows` and `BuildShippedAndroid`; output goes to a sibling folder next to the checkout, not into the repository). Same PC as the original builds, so this is a **clean checkout, not a clean machine**.

| | Windows x64 (Mono) | Android (IL2CPP, ARMv7 + ARM64) |
|---|---|---|
| Result | Succeeded, 0 errors, 4 warnings | Succeeded, 0 errors, 7 warnings (the same compiler warnings as before, plus linker flag warnings) |
| Files / entries | 155 files, same set as the shipped build | 731 APK entries, same set as the shipped APK |
| Byte-identical | 148 of 155, including `Dino Cap.exe` and all game data | 720 of 731 |
| Differing | `boot.config` (random build ID), two global manager files, two level files (1-2 bytes), and the two managed code DLLs (same size, different bytes) | `boot.config`, `globalgamemanagers`, `unity_app_guid`, and the native libraries `libil2cpp`, `libunity`, `libgame`, `libswappywrapper` for both ABIs (same sizes) |
| Recorder / capture code | not compiled in | not compiled in |
| New fixes present | yes (names found in the code) | yes (names found in the IL2CPP metadata) |

Reading the result: the file sets are identical and the game data is byte-identical, so the repository is complete for building and the README's build steps work from a clean checkout. What the differences are, as far as established:
- Build ID and GUID files (`boot.config`, `unity_app_guid`, the global manager files) differ as expected; the clean build's global file also lists the (compiled-out) debug recorder's script file name as an empty entry.
- The two managed code DLLs differ in bytes, but an independent static parse by the auditor found the same method names in the same order and identical method IL bodies (78 methods in `Assembly-CSharp`, 1,756 in `DinoCap.PortV2.Runtime`). That is not a full metadata or runtime check.
- `libgame.so`, `libil2cpp.so` and `libunity.so` differ only in build-ID and debug-link sections (auditor's ELF comparison).
- **`libswappywrapper.so` (both ABIs) also differs in its code and read-only data sections. This difference is unclassified: it was not examined further, and runtime equivalence of the clean-checkout build was not established.** No functional problem was observed or inferred.

Byte-for-byte reproducibility was **not** achieved and is not claimed.

**Not done:** a build on a different computer; a run of either clean-checkout build on a device; a full test-suite run in the clean checkout.
