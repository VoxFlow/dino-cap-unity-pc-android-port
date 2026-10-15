# sub_18BB64: shop.price_text.real_font_not_numeral_sprite

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-18BB64` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x18BB64` |
| RVA (EA - 0x4000) | `0x187B64` |
| File offset | `0x187B64` |
| Size | 22 bytes, 8 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_18BB64` (IDA automatic name) |
| Bytes SHA-256 | `2b00691ce45555b98713465e95c3d261440400de95cfde559168600058563514` |
| Subsystem | saves-shop-progression |
| Direct callers | 4 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `shop.price_text.real_font_not_numeral_sprite`: PROVEN |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `shop.price_text.real_font_not_numeral_sprite` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): Shop price/cost/level text (HP-refill price, weapon-tier numbers, shop cash readout) is rendered via a REAL FONT (Harlow Solid Italic, point size 22), not the icon_num0-9Frame numeral-sprite system an earlier FIFTH_REOPEN pass had assumed. Full decompile of su

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0x18BB64  90b5         PUSH            {R4,R7,LR}
0x18BB66  0446         MOV             R4, R0
0x18BB68  04f12800     ADD.W           R0, R4, #0x28 ; '('; this
0x18BB6C  01af         ADD             R7, SP, #4
0x18BB6E  2bf0acec     BLX             __ZNSs6assignERKSs; std::string::assign(std::string const&)
0x18BB72  0120         MOVS            R0, #1
0x18BB74  84f84000     STRB.W          R0, [R4,#0x40]
0x18BB78  90bd         POP             {R4,R7,PC}
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
int __fastcall sub_18BB64(int a1, const std::string *a2)
{
  std::string::assign((std::string *)(a1 + 40), a2);
  *(_BYTE *)(a1 + 64) = 1;
  return 1;
}
```

## References found in the code

Strings referenced: none

Globals referenced: none among the named globals

Callers (with call sites): `0x6A4A8`@`0x6B798`, `0x6A4A8`@`0x6B92C`, `0xDBC40`@`0xDDEA2`, `0xDBC40`@`0xDE062`

Callees: `0x1B74C8`

## Unity translation

- Code comments citing this address (2): `Runtime/CityGridController.cs:7645`, `Runtime/CityGridController.cs:8112`

## Validation

- No test is individually linked to this function.

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
