# sub_18BBC4: shop.price_text.real_font_not_numeral_sprite

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-18BBC4` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x18BBC4` |
| RVA (EA - 0x4000) | `0x187BC4` |
| File offset | `0x187BC4` |
| Size | 304 bytes, 107 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_18BBC4` (IDA automatic name) |
| Bytes SHA-256 | `e5e268e906b49adabffc5d3687abd53db6e4f60e9fcb8a371594e02d2e8c5a04` |
| Subsystem | saves-shop-progression |
| Direct callers | 146 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `shop.price_text.real_font_not_numeral_sprite`: PROVEN |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `shop.price_text.real_font_not_numeral_sprite` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): Shop price/cost/level text (HP-refill price, weapon-tier numbers, shop cash readout) is rendered via a REAL FONT (Harlow Solid Italic, point size 22), not the icon_num0-9Frame numeral-sprite system an earlier FIFTH_REOPEN pass had assumed. Full decompile of su

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0x18BBC4  f0b5         PUSH            {R4-R7,LR}
0x18BBC6  03af         ADD             R7, SP, #0xC
0x18BBC8  2de9000d     PUSH.W          {R8,R10,R11}
0x18BBCC  adf14004     SUB.W           R4, SP, #0x40 ; '@'
0x18BBD0  24f00f04     BIC.W           R4, R4, #0xF
0x18BBD4  a546         MOV             SP, R4
0x18BBD6  04f9ed82     VST1.64         {D8-D11}, [R4@128]!
0x18BBDA  04f9efc2     VST1.64         {D12-D15}, [R4@128]
0x18BBDE  94b0         SUB             SP, SP, #0x50
0x18BBE0  0290         STR             R0, [SP,#0x90+var_88]
0x18BBE2  40f24842c0f20702 MOV             R2, #(___gxx_personality_sj0_ptr - 0x18BBF0)
0x18BBEA  0c68         LDR             R4, [R1]
0x18BBEC  7a44         ADD             R2, PC; ___gxx_personality_sj0_ptr
0x18BBEE  4148         LDR             R0, =(unk_1D42A4 - 0x18BBF6)
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
void __fastcall sub_18BBC4(int a1, _DWORD *a2)
{
  __int64 v2; // d8
  __int64 v3; // d9
  __int64 v4; // d10
  __int64 v5; // d11
  __int64 v6; // d12
  __int64 v7; // d13
  __int64 v8; // d14
  __int64 v9; // d15
  unsigned int *v10; // r1
  int v11; // r2
  _BYTE v13[4]; // [sp+Ch] [bp-9Ch] BYREF
  int v14; // [sp+10h] [bp-98h] BYREF
  __int64 v15; // [sp+50h] [bp-58h]
  __int64 v16; // [sp+58h] [bp-50h]
  __int64 v17; // [sp+60h] [bp-48h]
  __int64 v18; // [sp+68h] [bp-40h]
  __int64 v19; // [sp+70h] [bp-38h]
  __int64 v20; // [sp+78h] [bp-30h]
```

## References found in the code

Strings referenced: none

Globals referenced: none among the named globals

Callers (with call sites): `0x6A4A8`@`0x6B7D4`, `0x6A4A8`@`0x6B968`, `0x6CADC`@`0x6D0C6`, `0x6CADC`@`0x6D128`, `0x6CADC`@`0x6D236`, `0x6CADC`@`0x6D29C`, `0x6CADC`@`0x6D366`, `0x6CADC`@`0x6D3C6`, `0x6CADC`@`0x6D490`, `0x6CADC`@`0x6D4F0`, `0x6CADC`@`0x6D5B8`, `0x6CADC`@`0x6D618`

Callees: `0x1B6898`, `0x1B73E8`, `0x1B7408`, `0x1B7778`, `0x1B7788`, `0x1B7798`

## Unity translation

- Code comments citing this address (3): `Runtime/CityGridController.cs:7642`, `Runtime/CityGridController.cs:7644`, `Runtime/CityGridController.cs:8108`

## Validation

- No test is individually linked to this function.

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
