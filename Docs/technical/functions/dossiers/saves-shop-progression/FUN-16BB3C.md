# sub_16BB3C: save_file.schema.dinocap_sav

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-16BB3C` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x16BB3C` |
| RVA (EA - 0x4000) | `0x167B3C` |
| File offset | `0x167B3C` |
| Size | 288 bytes, 101 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_16BB3C` (IDA automatic name) |
| Bytes SHA-256 | `9b2b840a25cb856cc3ac4742c3e496c8c7fef94ed57b686a84f526b18d7965df` |
| Subsystem | saves-shop-progression |
| Direct callers | 4 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `save_file.schema.dinocap_sav`: PROVEN |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `save_file.schema.dinocap_sav` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): Native persists a single raw binary blob named "dinocap.sav" with fixed byte offsets: offset 0 = dword_214FB4 (max level reached), offset 4 = dword_214FB0 (current level), offset 8 = dword_21BDF8 (money/exp pool), offset 12 = flt_21BDF0 (player HP), offset 16 

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0x16BB3C  f0b5         PUSH            {R4-R7,LR}
0x16BB3E  03af         ADD             R7, SP, #0xC
0x16BB40  2de9000d     PUSH.W          {R8,R10,R11}
0x16BB44  adf14004     SUB.W           R4, SP, #0x40 ; '@'
0x16BB48  24f00f04     BIC.W           R4, R4, #0xF
0x16BB4C  a546         MOV             SP, R4
0x16BB4E  04f9ed82     VST1.64         {D8-D11}, [R4@128]!
0x16BB52  04f9efc2     VST1.64         {D12-D15}, [R4@128]
0x16BB56  94b0         SUB             SP, SP, #0x50
0x16BB58  0c46         MOV             R4, R1
0x16BB5A  40f2d041c0f20901 MOV             R1, #(___gxx_personality_sj0_ptr - 0x16BB68)
0x16BB62  3e48         LDR             R0, =(unk_1D38CC - 0x16BB6A)
0x16BB64  7944         ADD             R1, PC; ___gxx_personality_sj0_ptr
0x16BB66  7844         ADD             R0, PC; unk_1D38CC
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
int __fastcall sub_16BB3C(int a1, int a2)
{
  __int64 v2; // d8
  __int64 v3; // d9
  __int64 v4; // d10
  __int64 v5; // d11
  __int64 v6; // d12
  __int64 v7; // d13
  __int64 v8; // d14
  __int64 v9; // d15
  int v10; // r6
  unsigned int *v12; // r1
  int v13; // r2
  _BYTE v14[4]; // [sp+Ch] [bp-9Ch] BYREF
  int v15; // [sp+10h] [bp-98h] BYREF
  __int64 v16; // [sp+50h] [bp-58h]
  __int64 v17; // [sp+58h] [bp-50h]
  __int64 v18; // [sp+60h] [bp-48h]
  __int64 v19; // [sp+68h] [bp-40h]
  __int64 v20; // [sp+70h] [bp-38h]
```

## References found in the code

Strings referenced: none

Globals referenced: none among the named globals

Callers (with call sites): `0x7E298`@`0x7E2CC`, `0x7E868`@`0x7E89C`, `0x7E970`@`0x7E9A4`, `0xEB9E4`@`0xEBA5E`

Callees: `0x1A6668`, `0x1B7478`, `0x1B7508`, `0x1B7778`, `0x1B7788`, `0x1B7798`

## Unity translation

- Code comments citing this address (1): `Runtime/CityGridController.cs:1896`

## Validation

- No test is individually linked to this function.

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
