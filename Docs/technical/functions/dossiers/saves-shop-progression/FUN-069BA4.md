# sub_69BA4: citygrind.level_counter.anchor_and_caller

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-069BA4` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x69BA4` |
| RVA (EA - 0x4000) | `0x65BA4` |
| File offset | `0x65BA4` |
| Size | 782 bytes, 281 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_69BA4` (IDA automatic name) |
| Bytes SHA-256 | `d882dd97993f22d42f115ea19789daaedb2b495d96797e522080f5c979d75eea` |
| Subsystem | saves-shop-progression |
| Direct callers | 4 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `citygrind.level_counter.anchor_and_caller`: PARTIALLY_SUPERSEDED_BY_DIRECT_NATIVE_EVIDENCE |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `citygrind.level_counter.anchor_and_caller` (HISTORICAL: that rule is superseded or falsified, so this is not a current-behaviour statement): sub_69BA4 (UILevelCtrliPad's own digit-render routine) has exactly 4 direct callers (xrefs_to, more:false): sub_5EEEC (main-menu-only, already ruled out), sub_5FBA8/sub_5FC00 (not CityGrid-relevant), and sub_56450 - the SOLE CityGrid-relevant caller, itself re

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0x69BA4  f0b5         PUSH            {R4-R7,LR}
0x69BA6  03af         ADD             R7, SP, #0xC
0x69BA8  2de9000d     PUSH.W          {R8,R10,R11}
0x69BAC  adf14004     SUB.W           R4, SP, #0x40 ; '@'
0x69BB0  24f00f04     BIC.W           R4, R4, #0xF
0x69BB4  a546         MOV             SP, R4
0x69BB6  04f9ed82     VST1.64         {D8-D11}, [R4@128]!
0x69BBA  04f9efc2     VST1.64         {D12-D15}, [R4@128]
0x69BBE  b0b0         SUB             SP, SP, #0xC0
0x69BC0  0646         MOV             R6, R0
0x69BC2  42f28840c0f21900 MOV             R0, #(___stack_chk_guard_ptr - 0x69BD0)
0x69BCA  0a46         MOV             R2, R1
0x69BCC  7844         ADD             R0, PC; ___stack_chk_guard_ptr
0x69BCE  2dac         ADD             R4, SP, #0x100+var_4C
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
int __fastcall sub_69BA4(_DWORD *a1, int a2)
{
  float32x2_t v2; // d0
  float32x2_t v3; // d8
  float32x2_t v4; // d9
  __int64 v5; // d10
  float32x2_t v6; // d11
  float32x2_t v7; // d12
  float32x2_t v8; // d13
  __int64 v9; // d14
  float32x2_t v10; // d15
  char v12; // r5
  float32x2_t v13; // d16
  float v14; // s16
  float32x2_t v15; // d9
  unsigned int v16; // r3
  unsigned int v17; // r4
  int v18; // r0
  unsigned int *v19; // r2
  int v20; // r1
```

## References found in the code

Strings referenced: `level%dFrame` (`0x1D7364`)

Globals referenced: `flt_69EB8`, `flt_69EBC`, `flt_69EC0`, `flt_69EC4`, `flt_69EC8`, `flt_69ECC`, `flt_69ED0`, `flt_69ED4`

Callers (with call sites): `0x56450`@`0x5654C`, `0x5EEEC`@`0x5F3C0`, `0x5FBA8`@`0x5FBFA`, `0x5FC00`@`0x5FC76`

Callees: `0x183C4C`, `0x183D54`, `0x1B4758`, `0x1B7478`, `0x1B7508`, `0x1B7648`, `0x1B7668`, `0x1B7778`, `0x1B7788`, `0x1B7798`, `0x1B7888`, `0x1B7FE8`

## Unity translation

- No Unity code comment cites this address: **unmapped** (not proof the behaviour is absent from the port).

## Validation

- No test is individually linked to this function.

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
