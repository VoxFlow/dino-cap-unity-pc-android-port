# sub_16BC64: save_file.schema.dinocap_sav

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-16BC64` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x16BC64` |
| RVA (EA - 0x4000) | `0x167C64` |
| File offset | `0x167C64` |
| Size | 406 bytes, 145 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_16BC64` (IDA automatic name) |
| Bytes SHA-256 | `b1806bb0946d0e3b8854dfcbb3b8a97da45238652561c2db47b29aab289adaf1` |
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
0x16BC64  f0b5         PUSH            {R4-R7,LR}
0x16BC66  03af         ADD             R7, SP, #0xC
0x16BC68  2de9000d     PUSH.W          {R8,R10,R11}
0x16BC6C  adf14004     SUB.W           R4, SP, #0x40 ; '@'
0x16BC70  24f00f04     BIC.W           R4, R4, #0xF
0x16BC74  a546         MOV             SP, R4
0x16BC76  04f9ed82     VST1.64         {D8-D11}, [R4@128]!
0x16BC7A  04f9efc2     VST1.64         {D12-D15}, [R4@128]
0x16BC7E  98b0         SUB             SP, SP, #0x60
0x16BC80  0490         STR             R0, [SP,#0xA0+var_90]
0x16BC82  0c46         MOV             R4, R1
0x16BC84  0592         STR             R2, [SP,#0xA0+var_8C]
0x16BC86  40f2a431c0f20901 MOV             R1, #(___gxx_personality_sj0_ptr - 0x16BC94)
0x16BC8E  5b48         LDR             R0, =(unk_1D38D4 - 0x16BC96)
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
int __fastcall sub_16BC64(int a1, int a2, _DWORD *a3)
{
  __int64 v3; // d8
  __int64 v4; // d9
  __int64 v5; // d10
  __int64 v6; // d11
  __int64 v7; // d12
  __int64 v8; // d13
  __int64 v9; // d14
  __int64 v10; // d15
  int v11; // r8
  int v12; // r5
  int v13; // r4
  unsigned int *v15; // r1
  int v16; // r2
  int v19; // [sp+18h] [bp-A0h] BYREF
  _BYTE v20[4]; // [sp+1Ch] [bp-9Ch] BYREF
  int v21; // [sp+20h] [bp-98h] BYREF
  __int64 v22; // [sp+60h] [bp-58h]
  __int64 v23; // [sp+68h] [bp-50h]
```

## References found in the code

Strings referenced: none

Globals referenced: none among the named globals

Callers (with call sites): `0x7E298`@`0x7E31E`, `0x7E868`@`0x7E8EE`, `0x7E970`@`0x7E9F6`, `0xEB9E4`@`0xEBA7A`

Callees: `0x195CB0`, `0x1A681C`, `0x1B49C8`, `0x1B7478`, `0x1B7508`, `0x1B7778`, `0x1B7788`, `0x1B7798`

## Unity translation

- Code comments citing this address (1): `Runtime/CityGridController.cs:1895`

## Validation

- No test is individually linked to this function.

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
