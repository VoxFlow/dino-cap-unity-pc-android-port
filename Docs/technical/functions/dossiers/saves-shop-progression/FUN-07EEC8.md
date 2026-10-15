# sub_7EEC8: citygrind.ammo.family_a_starting_200_source_derived

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-07EEC8` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x7EEC8` |
| RVA (EA - 0x4000) | `0x7AEC8` |
| File offset | `0x7AEC8` |
| Size | 320 bytes, 110 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_7EEC8` (IDA automatic name) |
| Bytes SHA-256 | `102ce3bc69ab8bf84a6c882389d896e9ad3020c826256d86d47e70df7cfd2f6c` |
| Subsystem | saves-shop-progression |
| Direct callers | 6 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `citygrind.ammo.family_a_starting_200_source_derived`: PROVEN; `citygrind.n251.lifecycle_visual_menu.checkpoint`: PROVEN |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `citygrind.ammo.family_a_starting_200_source_derived` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): Family A's starting City Grid ammo (dword_21BE24[0]) is unconditionally set to 200 by sub_7EEC8 (the City-Grind-start event), independently of the user's own runtime observation - Task004's own dedicated archaeology (evidence.json) decompiled this chain. Decre

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0x7EEC8  f0b5         PUSH            {R4-R7,LR}
0x7EECA  03af         ADD             R7, SP, #0xC
0x7EECC  2de9000d     PUSH.W          {R8,R10,R11}
0x7EED0  82b0         SUB             SP, SP, #8
0x7EED2  4cf61270c0f21900 MOV             R0, #(flt_21BDF0 - 0x7EEDE)
0x7EEDA  7844         ADD             R0, PC; flt_21BDF0
0x7EEDC  6af0a6ff     BL              sub_E9E2C
0x7EEE0  4cf60c74c0f21904 MOV             R4, #(dword_21BE00 - 0x7EEF4)
0x7EEE8  46f6fe01c0f21501 MOV             R1, #(aWeaponProp - 0x7EEF6); "weapon.prop"
0x7EEF0  7c44         ADD             R4, PC; dword_21BE00
0x7EEF2  7944         ADD             R1, PC; "weapon.prop"
0x7EEF4  2046         MOV             R0, R4
0x7EEF6  69f0a7ff     BL              sub_E8E48
0x7EEFA  fef72bfb     BL              sub_7D554
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
void sub_7EEC8()
{
  int v0; // r0
  int v1; // r0
  int v2; // r10
  int v3; // r11
  int v4; // r5
  char v5; // r8
  int v6; // r1
  int v7; // r2
  int v8; // [sp+0h] [bp-20h]
  int i; // [sp+4h] [bp-1Ch]

  sub_E9E2C(&flt_21BDF0);
  v0 = sub_E8E48(&dword_21BE00, "weapon.prop");
  sub_7D554(v0);
  if ( dword_214FB0 >= 5 )
  {
    v8 = dword_214FB0 / 5;
    for ( i = 1; ; ++i )
```

## References found in the code

Strings referenced: `weapon.prop` (`0x1D57F4`)

Globals referenced: `byte_21C3C4`, `dword_214FB0`, `dword_21BE00`, `dword_21BE0C`, `dword_21BE10`, `dword_21BE24`, `flt_21BDF0`

Callers (with call sites): `0x2A8A8`@`0x2A954`, `0x2A8A8`@`0x2AA58`, `0x5D380`@`0x5D880`, `0xA1518`@`0xA15C0`, `0xA1518`@`0xA16B0`, `0xCAA38`@`0xCAF38`

Callees: `0x7D554`, `0x7D81C`, `0x7E498`, `0xE8E48`, `0xE9E2C`

## Unity translation

- Code comments citing this address (9): `Runtime/CityGridController.cs:3045`, `Runtime/CityGridController.cs:3066`, `Runtime/CityGridController.cs:3071`, `Runtime/CityGridController.cs:3073`, `Runtime/CityGridController.cs:3077`, `Runtime/CityGridController.cs:5359`, `Runtime/CityGrindStartPreset.cs:6`, `Runtime/MainMenuController.cs:622` ...

## Validation

- No test is individually linked to this function.

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
