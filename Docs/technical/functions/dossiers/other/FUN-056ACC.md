# sub_56ACC: citygrind.player_death.dieend_timing

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-056ACC` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x56ACC` |
| RVA (EA - 0x4000) | `0x52ACC` |
| File offset | `0x52ACC` |
| Size | 872 bytes, 298 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_56ACC` (IDA automatic name) |
| Bytes SHA-256 | `8b8ee1052540a9ecdc940e0efa70709f5ec2fe4a4ac3cbdbe52161a3e528fd93` |
| Subsystem | other |
| Direct callers | 1 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `citygrind.player_death.dieend_timing`: PROVEN; `citygrind.player_death.timing`: PROVEN |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `citygrind.player_death.dieend_timing` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): Native player-death sequencing (mainSceneiPad's generic OnEvent switch, sub_56118) is two PROVEN phases: DieAnim/'p_die' plays to completion (31 frames @ 0.025s = 0.775s, already-established manifest data), then once scene-state reaches 9, event-type-16 uncond

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0x56ACC  f0b5         PUSH            {R4-R7,LR}
0x56ACE  03af         ADD             R7, SP, #0xC
0x56AD0  2de9000d     PUSH.W          {R8,R10,R11}
0x56AD4  adf14004     SUB.W           R4, SP, #0x40 ; '@'
0x56AD8  24f00f04     BIC.W           R4, R4, #0xF
0x56ADC  a546         MOV             SP, R4
0x56ADE  04f9ed82     VST1.64         {D8-D11}, [R4@128]!
0x56AE2  04f9efc2     VST1.64         {D12-D15}, [R4@128]
0x56AE6  b4b0         SUB             SP, SP, #0xD0
0x56AE8  0590         STR             R0, [SP,#0x110+var_FC]
0x56AEA  45f25e51c0f21a01 MOV             R1, #(___stack_chk_guard_ptr - 0x56AFA)
0x56AF2  c0ef5000     VMOV.I32        Q8, #0
0x56AF6  7944         ADD             R1, PC; ___stack_chk_guard_ptr
0x56AF8  4ff0ff32     MOV.W           R2, #0xFFFFFFFF
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
int __fastcall sub_56ACC(int a1)
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
  float32x2_t v13; // d16
  unsigned int *v15; // r1
  int v16; // r2
  unsigned int *v17; // r1
  int v18; // r2
  unsigned int v20; // [sp+14h] [bp-114h]
```

## References found in the code

Strings referenced: `icon_num%dFrame` (`0x1D4B09`), `die_iconFrame` (`0x1D527D`)

Globals referenced: `dword_1B98D0`, `dword_21BDFC`, `flt_21BDF0`, `flt_56E38`

Callers (with call sites): `0x56118`@`0x5629A`

Callees: `0xDA87C`, `0x17C968`, `0x183D54`, `0x1B4708`, `0x1B7478`, `0x1B7508`, `0x1B7778`, `0x1B7788`, `0x1B7798`, `0x1B7888`, `0x1B7FE8`

## Unity translation

- Code comments citing this address (1): `Runtime/CityGridController.cs:8505`

## Validation

- No test is individually linked to this function.

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
