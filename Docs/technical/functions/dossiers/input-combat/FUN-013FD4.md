# sub_13FD4: player.manual_weapon_switch.clears_melee_and_fire_flags

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-013FD4` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x13FD4` |
| RVA (EA - 0x4000) | `0xFFD4` |
| File offset | `0xFFD4` |
| Size | 7338 bytes, 2571 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_13FD4` (IDA automatic name) |
| Bytes SHA-256 | `724bca3a8cc833826c482b0b0bb7decd1d37696a142bbe00d5510420269a0fdc` |
| Subsystem | input-combat |
| Direct callers | 0 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `player.manual_weapon_switch.clears_melee_and_fire_flags`: PROVEN_NATIVE (decompile of sub_68080, callers of sub_65A08) / IMPLEMENTED / AUTOMATED_FOC |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `player.manual_weapon_switch.clears_melee_and_fire_flags` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): The manual next-weapon function sub_68080 sets byte_21BE74 (melee flag) and byte_21BE78 (fire flag) to 0 at the top of its family-advance loop, then (if the fire-active byte +188 is set) calls sub_66310 and clears +188, advances the family (wrapping, skipping 

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0x13FD4  f0b5         PUSH            {R4-R7,LR}
0x13FD6  03af         ADD             R7, SP, #0xC
0x13FD8  2de9000d     PUSH.W          {R8,R10,R11}
0x13FDC  adf14004     SUB.W           R4, SP, #0x40 ; '@'
0x13FE0  24f00f04     BIC.W           R4, R4, #0xF
0x13FE4  a546         MOV             SP, R4
0x13FE6  04f9ed82     VST1.64         {D8-D11}, [R4@128]!
0x13FEA  04f9efc2     VST1.64         {D12-D15}, [R4@128]
0x13FEE  adf5087d     SUB.W           SP, SP, #0x220; float
0x13FF2  0990         STR             R0, [SP,#0x260+var_23C]; int
0x13FF4  48f29a05c0f21e05 MOV             R5, #(off_1FC0BC - 0x14022)
0x13FFC  48f29001c0f21e01 MOV             R1, #(off_1FC0B4 - 0x14024)
0x14004  48f23016c0f21e06 MOV             R6, #(off_1FC14C - 0x1401C)
0x1400C  48f29a03c0f21e03 MOV             R3, #(off_1FC0C0 - 0x14026)
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
void __fastcall sub_13FD4(_DWORD *a1)
{
  float32x2_t v1; // d0
  float32x2_t v2; // d1
  __int64 v3; // d8
  __int64 v4; // d9
  __int64 v5; // d10
  __int64 v6; // d11
  __int64 v7; // d12
  __int64 v8; // d13
  __int64 v9; // d14
  __int64 v10; // d15
  int v11; // r0
  int v12; // r0
  int v13; // r0
  int v14; // r0
  int v15; // r0
  int v16; // r0
  int v17; // r0
  int v18; // r0
```

## References found in the code

Strings referenced: `whiteFrame` (`0x1D4939`), `effectClassiPad` (`0x1D4C48`), `bg_bloodFrame` (`0x1D4CD8`), `bar_bloodFrame` (`0x1D4CE6`), `bg_blood1Frame` (`0x1D4CF5`), `icon_bullet0Frame` (`0x1D4D04`), `icon_noneFrame` (`0x1D4D16`), `icon_stopFrame` (`0x1D4D25`)

Globals referenced: `byte_21BE6C`, `byte_21BE70`, `byte_21BE74`, `byte_21BE78`, `dword_214FC4`, `dword_219D74`, `dword_219D84`, `dword_219D94`, `dword_219DA4`, `dword_219DF4`, `dword_219E24`, `dword_219E54`

Callers (with call sites): none by direct reference

Callees: `0x15C80`, `0x15F84`, `0x54D94`, `0x54DEC`, `0x5590C`, `0x65A08`, `0x661D8`, `0x69F14`, `0x7D81C`, `0x7EC50`, `0x171AA0`, `0x172BC8`, `0x172BD0`, `0x172C0C`, `0x172C90`, `0x172D18`

## Unity translation

- Code comments citing this address (1): `Runtime/CityGridController.cs:3068`

## Validation

- Tests/Task027N236CityGridRepro.cs
- Tests/DC1EightDefectRepro.cs

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
