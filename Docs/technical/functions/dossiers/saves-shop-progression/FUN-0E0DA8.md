# sub_E0DA8: shop.price_text.real_font_not_numeral_sprite

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-0E0DA8` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0xE0DA8` |
| RVA (EA - 0x4000) | `0xDCDA8` |
| File offset | `0xDCDA8` |
| Size | 9414 bytes, 3297 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_E0DA8` (IDA automatic name) |
| Bytes SHA-256 | `93a6896136bc037a6501ba72da26db217f9e24af714731bb3ee48fb802ea8a57` |
| Subsystem | saves-shop-progression |
| Direct callers | 1 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `shop.price_text.real_font_not_numeral_sprite`: PROVEN |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `shop.price_text.real_font_not_numeral_sprite` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): Shop price/cost/level text (HP-refill price, weapon-tier numbers, shop cash readout) is rendered via a REAL FONT (Harlow Solid Italic, point size 22), not the icon_num0-9Frame numeral-sprite system an earlier FIFTH_REOPEN pass had assumed. Full decompile of su

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0xE0DA8  f0b5         PUSH            {R4-R7,LR}
0xE0DAA  03af         ADD             R7, SP, #0xC
0xE0DAC  2de9000d     PUSH.W          {R8,R10,R11}
0xE0DB0  adf14004     SUB.W           R4, SP, #0x40 ; '@'
0xE0DB4  24f00f04     BIC.W           R4, R4, #0xF
0xE0DB8  a546         MOV             SP, R4
0xE0DBA  04f9ed82     VST1.64         {D8-D11}, [R4@128]!
0xE0DBE  04f9efc2     VST1.64         {D12-D15}, [R4@128]
0xE0DC2  adf5387d     SUB.W           SP, SP, #0x2E0
0xE0DC6  0290         STR             R0, [SP,#0x320+var_318]
0xE0DC8  4bf28021c0f21101 MOV             R1, #(___stack_chk_guard_ptr - 0xE0DD8)
0xE0DD0  4bf25e20     MOVW            R0, #:lower16:(off_1FC03C - 0xE0DDE)
0xE0DD4  7944         ADD             R1, PC; ___stack_chk_guard_ptr
0xE0DD6  c0f21100     MOVT            R0, #:upper16:(off_1FC03C - 0xE0DDE)
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
int __fastcall sub_E0DA8(_DWORD *a1)
{
  __int64 v1; // d8
  __int64 v2; // d9
  __int64 v3; // d10
  __int64 v4; // d11
  __int64 v5; // d12
  __int64 v6; // d13
  __int64 v7; // d14
  __int64 v8; // d15
  int v9; // r6
  int v10; // r2
  unsigned int i; // r3
  unsigned int *v12; // r2
  int v13; // r1
  unsigned int *v14; // r2
  int v15; // r1
  unsigned int *v16; // r2
  int v17; // r1
  unsigned int *v18; // r2
```

## References found in the code

Strings referenced: `icon_bloodFrame` (`0x1D5BD4`), `icon_A%dFrame` (`0x1D7391`), `icon_B%dFrame` (`0x1D739F`), `icon_C%dFrame` (`0x1D73AD`), `icon_D%dFrame` (`0x1D73BB`), `icon_E%dFrame` (`0x1D73C9`), `icon_F%dFrame` (`0x1D73D7`), `icon_G%dFrame` (`0x1D73E5`)

Globals referenced: `dword_21BE00`, `dword_21BE0C`, `dword_21BE18`

Callers (with call sites): `0xDBC40`@`0xDE0F8`

Callees: `0x172BC8`, `0x172C0C`, `0x172D1C`, `0x172DB8`, `0x1745A8`, `0x18BB60`, `0x18BBC4`, `0x1B7478`, `0x1B7508`, `0x1B7778`, `0x1B7788`, `0x1B7798`, `0x1B7888`, `0x1B7FE8`

## Unity translation

- No Unity code comment cites this address: **unmapped** (not proof the behaviour is absent from the port).

## Validation

- No test is individually linked to this function.

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
