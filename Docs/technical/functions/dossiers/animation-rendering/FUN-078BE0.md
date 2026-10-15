# sub_78BE0: a2b.part_child_binding

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-078BE0` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x78BE0` |
| RVA (EA - 0x4000) | `0x74BE0` |
| File offset | `0x74BE0` |
| Size | 362 bytes, 120 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_78BE0` (IDA automatic name) |
| Bytes SHA-256 | `cc490f58254bed91c24192713566c9444165ae5d1af957547f284cb31bd2c784` |
| Subsystem | animation-rendering |
| Direct callers | 19 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `a2b.part_child_binding`: PROVEN; `a2b.weapon_effect_attachment_subsystem`: PROVEN; `citygrind.house.e_family_bazooka_overlaps_body_in_original`: PROVEN_USER_OBSERVATION_SHIPPED_QUIRK |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `a2b.part_child_binding` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): Core body-part binding between CASprite records and pre-existing CAObjSpriteCallBack children is POSITIONAL INDEX correspondence within sub_76A20 (Nth CASprite in the active frame's own vector <-> Nth child in actorAnimState+112's own array), NOT name-based lo

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0x78BE0  f0b5         PUSH            {R4-R7,LR}
0x78BE2  03af         ADD             R7, SP, #0xC
0x78BE4  2de9000d     PUSH.W          {R8,R10,R11}
0x78BE8  adf14004     SUB.W           R4, SP, #0x40 ; '@'
0x78BEC  24f00f04     BIC.W           R4, R4, #0xF
0x78BF0  a546         MOV             SP, R4
0x78BF2  04f9ed82     VST1.64         {D8-D11}, [R4@128]!
0x78BF6  04f9efc2     VST1.64         {D12-D15}, [R4@128]
0x78BFA  90b0         SUB             SP, SP, #0x40; int
0x78BFC  0446         MOV             R4, R0
0x78BFE  2068         LDR             R0, [R4]
0x78C00  0194         STR             R4, [SP,#0x80+var_7C]; int
0x78C02  0068         LDR             R0, [R0]; int
0x78C04  fff7eefa     BL              sub_781E4
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
void __fastcall sub_78BE0(int **a1, int a2, int a3, int a4, int a5)
{
  float32x2_t v5; // d0
  float32x2_t v6; // d1
  float32x2_t v7; // d2
  float32x2_t v8; // d3
  float32x2_t v9; // d9
  float32x2_t v10; // d11
  int *v12; // r11
  int *v13; // r5
  unsigned int v14; // r8
  int v15; // r4
  unsigned int v16; // r6
  int v17; // r1
  __int32 *v18; // r5
  __int32 *v19; // r0
  float32x2_t v20; // d0
  int v21; // [sp+0h] [bp-98h]
  int **v22; // [sp+4h] [bp-94h]
  void *v23; // [sp+8h] [bp-90h]
```

## References found in the code

Strings referenced: none

Globals referenced: `flt_78D50`, `flt_78D54`, `flt_78D58`, `flt_78D5C`

Callers (with call sites): `0x2A8A8`@`0x3CEF8`, `0x2A8A8`@`0x3D1DE`, `0x2A8A8`@`0x3D4E0`, `0x2A8A8`@`0x42F32`, `0x7D554`@`0x7D78E`, `0x7D81C`@`0x7D992`, `0x7D81C`@`0x7DAB6`, `0x7D81C`@`0x7DB6A`, `0x7D81C`@`0x7DC46`, `0x7D81C`@`0x7DDF4`, `0x7D81C`@`0x7DEF4`, `0x7D81C`@`0x7DFA8`

Callees: `0x77FCC`, `0x781E4`, `0x1B4908`, `0x1B7648`, `0x1B7778`, `0x1B7788`, `0x1B7798`

## Unity translation

- Code comments citing this address (1): `Runtime/Power2DSourceTruthRenderer.cs:961`

## Validation

- No test is individually linked to this function.

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
