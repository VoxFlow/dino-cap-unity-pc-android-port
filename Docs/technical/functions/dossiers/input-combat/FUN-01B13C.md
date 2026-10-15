# sub_1B13C: player.manual_weapon_switch.clears_melee_and_fire_flags

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-01B13C` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x1B13C` |
| RVA (EA - 0x4000) | `0x1713C` |
| File offset | `0x1713C` |
| Size | 1592 bytes, 532 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_1B13C` (IDA automatic name) |
| Bytes SHA-256 | `6998686f2d399fa383280c6aafd0693d2273fbd46621fba14f5c3f4e0950d72e` |
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
0x1B13C  f0b5         PUSH            {R4-R7,LR}
0x1B13E  03af         ADD             R7, SP, #0xC
0x1B140  2de9000d     PUSH.W          {R8,R10,R11}
0x1B144  adf14004     SUB.W           R4, SP, #0x40 ; '@'
0x1B148  24f00f04     BIC.W           R4, R4, #0xF
0x1B14C  a546         MOV             SP, R4
0x1B14E  04f9ed82     VST1.64         {D8-D11}, [R4@128]!
0x1B152  04f9efc2     VST1.64         {D12-D15}, [R4@128]
0x1B156  a4b0         SUB             SP, SP, #0x90
0x1B158  8046         MOV             R8, R0
0x1B15A  0d46         MOV             R5, R1
0x1B15C  d8f80400     LDR.W           R0, [R8,#4]
0x1B160  57f1d8fd     BL              sub_172D14
0x1B164  61f1dafa     BL              sub_17C71C
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
void __fastcall sub_1B13C(int a1, int a2)
{
  float32x2_t v2; // d0
  float32x2_t v3; // d1
  __int64 v4; // d8
  __int64 v5; // d9
  __int64 v6; // d10
  __int64 v7; // d11
  __int64 v8; // d12
  __int64 v9; // d13
  __int64 v10; // d14
  __int64 v11; // d15
  int v14; // r0
  const void *v15; // r0
  _DWORD *v16; // r0
  int v17; // r10
  __int32 v18; // r11
  __int32 *v19; // r6
  int v20; // s16
  float32x2_t v21; // d16
```

## References found in the code

Strings referenced: `Audio_itemAudio` (`0x1D49C7`), `secondtimer` (`0x1D50FF`), `LaddhpFrame` (`0x1D510B`), `lifemap` (`0x1D5117`)

Globals referenced: `dword_21BE00`, `dword_21BE0C`, `dword_21BE30`, `flt_1B528`, `flt_1B778`, `flt_21BDF0`

Callers (with call sites): none by direct reference

Callees: `0x1B0D0`, `0x1B780`, `0x1B95C`, `0x54D94`, `0x54DEC`, `0x65A08`, `0x7D81C`, `0x7EB04`, `0x172BC8`, `0x172C78`, `0x172C90`, `0x172D14`, `0x172D18`, `0x172D1C`, `0x172D50`, `0x172DEC`

## Unity translation

- No Unity code comment cites this address: **unmapped** (not proof the behaviour is absent from the port).

## Validation

- Tests/Task027N236CityGridRepro.cs
- Tests/DC1EightDefectRepro.cs

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
