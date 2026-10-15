# Build and validation matrix

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../start/disclaimer.md).*

> **In simple terms.** This table says exactly which builds exist, what was checked on each, and what was not. Compiling, launching,
> playing and matching the original are separate levels; a tick at one level does not imply the next.

## Build artifacts (rebuilt 15 Oct from the current source)

| Target | Backend / architecture | Artifact and SHA-256 | Result |
|---|---|---|---|
| Windows x64 | Mono | `Dino Cap.exe` `b258942764bdcc8f1a1694baab9c8300e9e04f05bb140dfcf0519731fe2235bc` (the Unity player, unchanged); principal game runtime `DinoCap.PortV2.Runtime.dll` `97468adc62d698b2da361ea13eed34596278d79ca1c3ef2056475be6d8b04c7a`; `Assembly-CSharp.dll` `0b919a74cccccf245343f232c1f49a76afda0a18a9cf17e0131de0e0e059e58e`; 156 files, about 159 MB | Succeeded, 0 errors, 4 warnings |
| Android | IL2CPP, ARMv7 + ARM64 in one universal APK (`armeabi-v7a` and `arm64-v8a` `libil2cpp.so` confirmed inside); package `com.dc1port.convergence`; min Android 7.1 (API 25) | `Dino Cap.apk` `d06347ec4fd6b967c4689266711befadf4f3cb3bad4a3e13173cde2f69e78591`, 50.4 MB | Succeeded, 0 errors, 5 warnings |

Earlier builds are superseded: 14 Oct (runtime DLL `1ae0e1eb...f02d`, APK `88d74109...eda`) and the first 15 Oct rebuild (runtime DLL `804f0929...aac89`, APK `05ea5c55...50c4`, made before the Android fix for pausing while a move control is held). Only the builds in the table above are the release candidate.
Both rebuilt artifacts contain the later fixes (checked by searching the shipped code for the new class and method names). Neither contains the debug frame recorder
or the capture harness (their names are absent from the shipped code). A file time cannot show which patches a build contains; the release candidate is identified by
the checksum file and release-tree manifest published with it.

## Validation levels

| Level | Windows | Android | Evidence |
|---|---|---|---|
| Compiles | yes | yes | build results above (the 15 Oct rebuild and the clean-checkout build) |
| Automated tests (headless, Unity editor), fresh full run on this source | 1215 tests: 1211 passed, 1 skipped (manual capture), 3 failed: the parked K4 raptor pair and one environment error that passed when rerun alone (40 of 40) | same code (editor tests, not an Android device run) | [testing page](../quality/testing-and-validation.md) |
| Recorded smoke runs (4 scenes load, 0 scene errors) | Windows Mono 64-bit | Android IL2CPP: a 32-bit Android 13 phone, a 64-bit Android 12 phone and a 64-bit Android 15 tablet, all PASS | `DC1_Build_Establishment` smoke records. These ran on **earlier development builds**, not on the hashed APK above; no artifact-to-run link exists for the current files |
| Gameplay played by the owner | yes | yes | owner testimony within the scenarios tried |
| Matches the original | owner compared against an iPad for many items | owner compared touch behaviour | [fidelity](../fidelity/shipped-quirks-and-divergences.md) |
| Fresh test suite on the release candidate | done (above) | done (above) | NUnit result retained privately |
| Rebuilt release candidate hashes | done (above) | done (above) | this page |
| Built from a clean checkout of the repository (same PC, no cached files) | yes: 0 errors, same file set, game data identical, managed code identical by an auditor's static IL parse | yes: 0 errors, same entries; build-ID files and native libraries differ, and one native library (`libswappywrapper`) differs in its code sections, which is unclassified; runtime equivalence not established | [clean-checkout build check](clean-checkout-build.md) |
| Smoke run of the rebuilt artifacts on a device | not yet | not yet | to do (the Android pause-while-moving fix is verified by headless pointer tests only) |
| Android save persistence verified on device | n/a | **confirmed by the owner** (15 Oct, saving works on the owner's Android device; owner testimony, not an automated check) | owner report |
| Original-device acceptance of flamethrower and laser | open | open | known limitations |

## Environments

Windows: the owner's PC (graphics API checks are in the build reports). Android smoke devices: Android 13 (32-bit only), Android 12 (64-bit) and Android 15 (64-bit). Exact device coverage of the final
release candidate is not yet established. Not tested: multi-monitor and mixed-DPI desktop use, Android versions outside those above.

## Known limitations (summary)

See [release notes](tools-and-release-notes.md) and [fidelity and divergences](../fidelity/shipped-quirks-and-divergences.md).

Status of this page: DOCS_DRAFT. `RELEASE_EVIDENCE_VERIFIED` is not claimed.
