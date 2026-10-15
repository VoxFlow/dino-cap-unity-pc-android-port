# sub_16BE04: save_file.schema.dinocap_sav

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-16BE04` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x16BE04` |
| RVA (EA - 0x4000) | `0x167E04` |
| File offset | `0x167E04` |
| Size | 592 bytes, 219 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_16BE04` (IDA automatic name) |
| Bytes SHA-256 | `cc76e413ca9934573eaa30200c4b9963ac932068f8756e10689e5b69592738e8` |
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
0x16BE04  f0b5         PUSH            {R4-R7,LR}
0x16BE06  03af         ADD             R7, SP, #0xC
0x16BE08  2de9000d     PUSH.W          {R8,R10,R11}
0x16BE0C  adf14004     SUB.W           R4, SP, #0x40 ; '@'
0x16BE10  24f00f04     BIC.W           R4, R4, #0xF
0x16BE14  a546         MOV             SP, R4
0x16BE16  04f9ed82     VST1.64         {D8-D11}, [R4@128]!
0x16BE1A  04f9efc2     VST1.64         {D12-D15}, [R4@128]
0x16BE1E  a4b0         SUB             SP, SP, #0x90
0x16BE20  0d46         MOV             R5, R1
0x16BE22  0690         STR             R0, [SP,#0xD0+var_B8]
0x16BE24  40f20421     MOVW            R1, #:lower16:(___gxx_personality_sj0_ptr - 0x16BE34)
0x16BE28  1646         MOV             R6, R2
0x16BE2A  c0f20901     MOVT            R1, #:upper16:(___gxx_personality_sj0_ptr - 0x16BE34)
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
int __fastcall sub_16BE04(int a1, int a2, _DWORD *a3)
{
  __int64 v3; // d8
  __int64 v4; // d9
  __int64 v5; // d10
  __int64 v6; // d11
  __int64 v7; // d12
  __int64 v8; // d13
  __int64 v9; // d14
  __int64 v10; // d15
  int v13; // r0
  int v14; // r6
  unsigned int *v16; // r1
  int v17; // r2
  unsigned int *v18; // r1
  int v19; // r2
  int v20; // [sp+10h] [bp-D8h]
  int v21; // [sp+14h] [bp-D4h]
  _BYTE v23[4]; // [sp+24h] [bp-C4h] BYREF
  int v24; // [sp+28h] [bp-C0h] BYREF
```

## References found in the code

Strings referenced: none

Globals referenced: none among the named globals

Callers (with call sites): `0x7E394`@`0x7E43C`, `0x7E498`@`0x7E6E0`, `0x7E748`@`0x7E80C`, `0xEB9E4`@`0xEBAA2`

Callees: `0x195A10`, `0x1A6A14`, `0x1B49C8`, `0x1B56A8`, `0x1B7478`, `0x1B7508`, `0x1B7648`, `0x1B7778`, `0x1B7788`, `0x1B7798`

## Unity translation

- Code comments citing this address (1): `Runtime/CityGridController.cs:1894`

## Validation

- No test is individually linked to this function.

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
