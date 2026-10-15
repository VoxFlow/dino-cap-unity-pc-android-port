# Tools and release notes

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../start/disclaimer.md).*

## Tools used

| Tool | Used for |
|---|---|
| IDA Pro 9.2 (read-only policy) | Inspecting the original iOS program. The policy forbids changing the database (no renames, comments, types or patches) and the tasks report none; a complete mutation history was not verified, and the database file's hash changes when sessions open and close |
| Unity 6000.3 | The game itself. Windows x64 uses the Mono scripting backend. Android uses **IL2CPP** to produce one universal ARMv7 + ARM64 APK, because the stock Unity editor used here supports Android Mono only on ARMv7, so IL2CPP was needed to put ARMv7 and ARM64 in one APK (no custom engine work; not a gameplay matter) |
| Extraction scripts (`Extract_DinoCapData`, A2B/C2B extractor, atlas tools) | Unpacking the original archive and converting animations to a neutral format |
| Unity export tools | Producing the animation/atlas manifest used by the game |
| F3 native trace tool (source only) | An iOS tweak and a PC decoder written to record the laser weapon's behaviour on a jailbroken iPad. It was **never built or run**; no trace was ever captured, and nothing in the game depends on it |
| Source-truth registry tools | Keeping the contract registry valid |

Original game files are never modified; everything shipped is derived from them.

## Release notes (draft)

**Contents:** Windows 64-bit build (`Dino Cap.exe` with its data folder) and an Android universal APK
(`Dino Cap.apk`, Android 7.1 or newer). Download them from the **Releases** section on GitHub; see the
[download guide](../start/download-guide.md). Release **v1.0.0**; SHA-256 checksums are in `SHA256SUMS.txt` on the release page.

**Playable:** City Grind, Jungle Blitz, main menu and options, shop and upgrades, mouse/keyboard on PC and touch on
Android.

**Known differences and open items** (full list in [fidelity and divergences](../fidelity/shipped-quirks-and-divergences.md)):

- The shop table shows an extra edge row that a workaround did not remove (left as is).
- Flamethrower and laser device acceptance, and multi-monitor behaviour, are not fully checked.
- Camera one-frame blip at world edges and a Bazooka/house-animation overlap are original bugs, kept.
- Saves use a JSON file and are not compatible with the original save format.
- Offline only: no Game Center or online scores.
- Two parked automated tests (K4 raptor) do not pass.
- Debug-only: on one tested Android phone and keyboard combination, Back does not close the soft keyboard in the debug number
  fields. Not pursued by owner decision.

**Optional debug tools** are included in every build but are off by default.
