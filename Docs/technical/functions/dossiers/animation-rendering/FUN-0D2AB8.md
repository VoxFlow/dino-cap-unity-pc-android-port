# sub_D2AB8: citygrind.viewport_aspect_policy.native_contract_and_unity_proof

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-0D2AB8` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0xD2AB8` |
| RVA (EA - 0x4000) | `0xCEAB8` |
| File offset | `0xCEAB8` |
| Size | 714 bytes, 224 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_D2AB8` (IDA automatic name) |
| Bytes SHA-256 | `1120bfead19f49f2be8000d4e9c47471190f24170c31d0fa3dd2f669642f39bb` |
| Subsystem | animation-rendering |
| Direct callers | 1 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `citygrind.viewport_aspect_policy.native_contract_and_unity_proof`: PROVEN |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `citygrind.viewport_aspect_policy.native_contract_and_unity_proof` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): Native's own logical view is 1024x768 (4:3), vertically-fixed, horizontally-follow-only camera (Task024D, already-accepted, re-confirmed non-stale this reopening). Task024D's own Section 8 already explicitly specified a 'vertical-locked, horizontally-extensibl

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0xD2AB8  f0b5         PUSH            {R4-R7,LR}
0xD2ABA  03af         ADD             R7, SP, #0xC
0xD2ABC  90b0         SUB             SP, SP, #0x40
0xD2ABE  0446         MOV             R4, R0
0xD2AC0  0e46         MOV             R6, R1
0xD2AC2  6068         LDR             R0, [R4,#4]
0xD2AC4  a0f0d8f8     BL              sub_172C78
0xD2AC8  d0e90001     LDRD.W          R0, R1, [R0]
0xD2ACC  0f91         STR             R1, [SP,#0x40+var_8+4]
0xD2ACE  0e90         STR             R0, [SP,#0x40+var_8]
0xD2AD0  6068         LDR             R0, [R4,#4]
0xD2AD2  a0f01ff9     BL              sub_172D14
0xD2AD6  0546         MOV             R5, R0
0xD2AD8  a9f044ff     BL              sub_17C964
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
int __fastcall sub_D2AB8(int a1, __int32 a2)
{
  float32x2_t v2; // d0
  float32x2_t v3; // d1
  float32x2_t v4; // d2
  float32x2_t v5; // d3
  int v8; // r5
  int v9; // r1
  int v10; // r0
  float32x2_t v11; // d16
  float32x2_t v12; // d18
  float32x2_t v13; // d20
  float32x2_t v14; // d0
  float32x2_t v15; // d16
  float32x2_t v16; // d18
  float32x2_t v17; // d20
  const void *v18; // r0
  int v19; // r1
  void *v20; // r6
  int v21; // r0
```

## References found in the code

Strings referenced: none

Globals referenced: `byte_21BE78`, `dword_21BDF4`, `flt_214FCC`, `flt_21BDF0`, `flt_D2D84`, `flt_D2D88`, `flt_D2D8C`, `flt_D2D90`, `flt_D2D94`, `flt_D2D98`

Callers (with call sites): `0xD2904`@`0xD29A2`

Callees: `0xC62DC`, `0xD2EB8`, `0xD2F4C`, `0xD2F9C`, `0x166414`, `0x16669C`, `0x172BD0`, `0x172C78`, `0x172D14`, `0x172D50`, `0x173A98`, `0x173B50`, `0x17C71C`, `0x17C964`, `0x1B76A8`

## Unity translation

- No Unity code comment cites this address: **unmapped** (not proof the behaviour is absent from the port).

## Validation

- No test is individually linked to this function.

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
