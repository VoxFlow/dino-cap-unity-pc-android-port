# sub_6CADC: citygrind.shop_screen.entry_trigger_and_layout

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-06CADC` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x6CADC` |
| RVA (EA - 0x4000) | `0x68ADC` |
| File offset | `0x68ADC` |
| Size | 9406 bytes, 3285 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_6CADC` (IDA automatic name) |
| Bytes SHA-256 | `03bc320a78ce3da7c0c42fe61b9c91582b4bdf35004956b7a220fb2545e74411` |
| Subsystem | saves-shop-progression |
| Direct callers | 1 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `citygrind.shop_screen.entry_trigger_and_layout`: PROVEN (fade mechanism/duration, relative layout) / HARD_BLOCKED (literal entry-trigger ca; `citygrind.shop_screen.weapon_icon_anchors`: PROVEN; `shop.price_text.real_font_not_numeral_sprite`: PROVEN |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `citygrind.shop_screen.entry_trigger_and_layout` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): Shop entry's own scene-state=5 write is reachable via 2 independent candidates - sub_56118's own case18 (unconditional, no control-identity check) and sub_7334C (an identical single-instruction state write, found via the shop-exit-trigger family) - BOTH proven

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0x6CADC  f0b5         PUSH            {R4-R7,LR}
0x6CADE  03af         ADD             R7, SP, #0xC
0x6CAE0  2de9000d     PUSH.W          {R8,R10,R11}
0x6CAE4  adf14004     SUB.W           R4, SP, #0x40 ; '@'
0x6CAE8  24f00f04     BIC.W           R4, R4, #0xF
0x6CAEC  a546         MOV             SP, R4
0x6CAEE  04f9ed82     VST1.64         {D8-D11}, [R4@128]!
0x6CAF2  04f9efc2     VST1.64         {D12-D15}, [R4@128]
0x6CAF6  adf5387d     SUB.W           SP, SP, #0x2E0
0x6CAFA  0290         STR             R0, [SP,#0x320+var_318]
0x6CAFC  4ff24c51c0f21801 MOV             R1, #(___stack_chk_guard_ptr - 0x6CB0C)
0x6CB04  4ff22a50     MOVW            R0, #:lower16:(off_1FC03C - 0x6CB12)
0x6CB08  7944         ADD             R1, PC; ___stack_chk_guard_ptr
0x6CB0A  c0f21800     MOVT            R0, #:upper16:(off_1FC03C - 0x6CB12)
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
int __fastcall sub_6CADC(_DWORD *a1)
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

Callers (with call sites): `0x6A4A8`@`0x6B9B2`

Callees: `0x172BC8`, `0x172C0C`, `0x172D1C`, `0x172DB8`, `0x1745A8`, `0x18BB60`, `0x18BBC4`, `0x1B7478`, `0x1B7508`, `0x1B7778`, `0x1B7788`, `0x1B7798`, `0x1B7888`, `0x1B7FE8`

## Unity translation

- Code comments citing this address (6): `Runtime/CityGridController.cs:3569`, `Runtime/CityGridController.cs:3581`, `Runtime/CityGridController.cs:3604`, `Runtime/CityGridController.cs:7008`, `Runtime/CityGridController.cs:8108`, `Runtime/CityGridController.cs:8255`

## Validation

- No test is individually linked to this function.

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
