# sub_FDE0: dino.offscreen_removal_margin

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-00FDE0` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0xFDE0` |
| RVA (EA - 0x4000) | `0xBDE0` |
| File offset | `0xBDE0` |
| Size | 102 bytes, 32 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_FDE0` (IDA automatic name) |
| Bytes SHA-256 | `7d08fa2ae47e6ac26c25c7f8f6ec03013ab77c6a73c04dacefa99a9d4c8b3874` |
| Subsystem | dinosaurs-spawning |
| Direct callers | 1 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `dino.offscreen_removal_margin`: PROVEN |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `dino.offscreen_removal_margin` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): Both CityGridController and CombatSandboxV2 now use the SAME PROVEN 500.0f offscreen-removal margin (native sub_FDE0, exact IEEE-754 bit patterns 0xC3FA0000/0x43FA0000). CombatSandboxV2's own prior 480f was N.2.34's own superseded approximation, never updated 

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0xFDE0  90b5         PUSH            {R4,R7,LR}
0xFDE2  01af         ADD             R7, SP, #4
0xFDE4  2ded028b     VPUSH           {D8}
0xFDE8  84b0         SUB             SP, SP, #0x10
0xFDEA  0446         MOV             R4, R0
0xFDEC  6068         LDR             R0, [R4,#4]
0xFDEE  62f143ff     BL              sub_172C78
0xFDF2  90ed008a     VLDR            S16, [R0]
0xFDF6  6068         LDR             R0, [R4,#4]
0xFDF8  62f18cff     BL              sub_172D14
0xFDFC  6cf1b2fd     BL              sub_17C964
0xFE00  0146         MOV             R1, R0
0xFE02  6846         MOV             R0, SP
0xFE04  56f14afc     BL              sub_16669C
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
int __fastcall sub_FDE0(int a1)
{
  float32x2_t v1; // d0
  float32x2_t v2; // d1
  float32x2_t v3; // d2
  float v5; // s16
  int v6; // r0
  int v7; // r1
  int v8; // r0
  int result; // r0
  float32x2_t v10; // d1
  float v11[4]; // [sp+0h] [bp-1Ch] BYREF

  v5 = *(float *)sub_172C78(*(_DWORD *)(a1 + 4));
  v6 = sub_172D14(*(_DWORD *)(a1 + 4));
  v8 = sub_17C964(v6, v7);
  result = sub_16669C(v11, v8);
  v1.f32[0] = v11[0];
  v2.i32[0] = -1007026176;
  v10 = vadd_f32(v1, v2);
```

## References found in the code

Strings referenced: none

Globals referenced: `flt_FE48`, `flt_FE4C`

Callers (with call sites): `0xFD58`@`0xFDB4`

Callees: `0x100B8`, `0x16669C`, `0x172C78`, `0x172D14`, `0x17C964`

## Unity translation

- Code comments citing this address (5): `Runtime/CityGridController.cs:6470`, `Runtime/CityGridController.cs:6478`, `Runtime/CombatSandboxV2.cs:158`, `Runtime/CombatSandboxV2.cs:1772`, `Runtime/CombatSandboxV2.cs:1781`

## Validation

- No test is individually linked to this function.

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
