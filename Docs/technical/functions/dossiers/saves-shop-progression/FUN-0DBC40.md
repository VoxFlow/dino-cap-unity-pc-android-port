# sub_DBC40: shop.price_text.real_font_not_numeral_sprite

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-0DBC40` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0xDBC40` |
| RVA (EA - 0x4000) | `0xD7C40` |
| File offset | `0xD7C40` |
| Size | 20840 bytes, 6529 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_DBC40` (IDA automatic name) |
| Bytes SHA-256 | `628ef9e6a4e9ca4e1c6bf872ce4797973c9ac720413940cb6ab897deb9685dfd` |
| Subsystem | saves-shop-progression |
| Direct callers | 0 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `shop.price_text.real_font_not_numeral_sprite`: PROVEN |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `shop.price_text.real_font_not_numeral_sprite` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): Shop price/cost/level text (HP-refill price, weapon-tier numbers, shop cash readout) is rendered via a REAL FONT (Harlow Solid Italic, point size 22), not the icon_num0-9Frame numeral-sprite system an earlier FIFTH_REOPEN pass had assumed. Full decompile of su

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0xDBC40  f0b5         PUSH            {R4-R7,LR}
0xDBC42  03af         ADD             R7, SP, #0xC
0xDBC44  2de9000d     PUSH.W          {R8,R10,R11}
0xDBC48  adf14004     SUB.W           R4, SP, #0x40 ; '@'
0xDBC4C  24f00f04     BIC.W           R4, R4, #0xF
0xDBC50  a546         MOV             SP, R4
0xDBC52  04f9ed82     VST1.64         {D8-D11}, [R4@128]!
0xDBC56  04f9efc2     VST1.64         {D12-D15}, [R4@128]
0xDBC5A  adf5e26d     SUB.W           SP, SP, #0x710
0xDBC5E  0b90         STR             R0, [SP,#0x750+var_724]; int
0xDBC60  4068         LDR             R0, [R0,#4]
0xDBC62  a0f07ffe     BL              sub_17C964
0xDBC66  0021         MOVS            R1, #0
0xDBC68  0022         MOVS            R2, #0
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
void __fastcall sub_DBC40(int *a1)
{
  float32x2_t v1; // d0
  float32x2_t v2; // d1
  __int64 v3; // d8
  float32x2_t v4; // d9
  float32x2_t v5; // d10
  __int64 v6; // d11
  __int64 v7; // d12
  __int64 v8; // d13
  __int64 v9; // d14
  __int64 v10; // d15
  int v11; // r0
  int v12; // r0
  int v13; // r0
  int v14; // r0
  int v15; // r0
  int v16; // r0
  int v17; // r0
  int v18; // r0
```

## References found in the code

Strings referenced: `iap_3wFrame` (`0x1D4905`), `iap_3w_dFrame` (`0x1D4911`), `iap_8wFrame` (`0x1D491F`), `iap_8w_dFrame` (`0x1D492B`), `whiteFrame` (`0x1D4939`), `icon_goldFrame` (`0x1D4AF7`), `bg_bloodFrame` (`0x1D4CD8`), `bar_bloodFrame` (`0x1D4CE6`)

Globals referenced: `byte_21B400`, `byte_21BE6C`, `byte_21BE70`, `byte_21BE74`, `byte_21BE78`, `dword_214FC4`, `dword_21A4E4`, `dword_21A4F4`, `dword_21A504`, `dword_21A514`, `dword_21A534`, `dword_21A544`

Callers (with call sites): none by direct reference

Callees: `0x7EC50`, `0x7F008`, `0xC4668`, `0xC46E0`, `0xD2E64`, `0xE0DA8`, `0x166414`, `0x171AA0`, `0x172BC8`, `0x172C0C`, `0x172C90`, `0x172D18`, `0x172D1C`, `0x172D84`, `0x172DB8`, `0x1745A8`

## Unity translation

- No Unity code comment cites this address: **unmapped** (not proof the behaviour is absent from the port).

## Validation

- No test is individually linked to this function.

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
