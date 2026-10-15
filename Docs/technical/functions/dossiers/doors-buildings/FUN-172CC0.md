# sub_172CC0: citygrind.house.door_icon_range_com2_md

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-172CC0` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x172CC0` |
| RVA (EA - 0x4000) | `0x16ECC0` |
| File offset | `0x16ECC0` |
| Size | 4 bytes, 2 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_172CC0` (IDA automatic name) |
| Bytes SHA-256 | `9afbafa9500eaae209946422ad5c3fe101a12bf9fc2256f82dcf6232cc6f23bd` |
| Subsystem | doors-buildings |
| Direct callers | 24 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `citygrind.house.door_icon_range_com2_md`: PROVEN_NATIVE_ARM (gate formula) / LIKELY (child world-position equals root + part x) / IM |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `citygrind.house.door_icon_range_com2_md` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): Pulsing icon_houseFrame (houseCtrl, 0.1 s 100%/80% quad toggle, sub_69F30) is shown/hidden by sub_56030, which asks each houseClassiPad via sub_CE2C: |player.x - com2.x| < com2.w*0.7f (0x3F333333) + 5.0f, com2 = the composite child named 'com2' (sub_77708), x 

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0x172CC0  2830         ADDS            R0, #0x28 ; '('
0x172CC2  7047         BX              LR
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
int __fastcall sub_172CC0(int a1)
{
  return a1 + 40;
}
```

## References found in the code

Strings referenced: none

Globals referenced: none among the named globals

Callers (with call sites): `0xCE2C`@`0xCE5E`, `0xD308`@`0xD336`, `0xD308`@`0xD3A2`, `0x10ECC`@`0x10F08`, `0x13174`@`0x131CC`, `0x54FA0`@`0x55610`, `0x66DCC`@`0x67168`, `0x66DCC`@`0x671B4`, `0x67458`@`0x67520`, `0x67884`@`0x6789C`, `0x679F4`@`0x67A22`, `0x67CE8`@`0x67D18`

Callees: none

## Unity translation

- Code comments citing this address (2): `Runtime/CityGridController.cs:135`, `Runtime/Power2DSourceTruthRenderer.cs:1478`

## Validation

- No test is individually linked to this function.

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
