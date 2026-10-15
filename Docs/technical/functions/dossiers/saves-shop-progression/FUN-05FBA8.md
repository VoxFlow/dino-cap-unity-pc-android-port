# sub_5FBA8: citygrind.level_counter.anchor_and_caller

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-05FBA8` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x5FBA8` |
| RVA (EA - 0x4000) | `0x5BBA8` |
| File offset | `0x5BBA8` |
| Size | 86 bytes, 32 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_5FBA8` (IDA automatic name) |
| Bytes SHA-256 | `dded0c8a7ee35939db59b0908e1f0bfd134306cd31d24a08080732c6050a6905` |
| Subsystem | saves-shop-progression |
| Direct callers | 1 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `citygrind.level_counter.anchor_and_caller`: PARTIALLY_SUPERSEDED_BY_DIRECT_NATIVE_EVIDENCE |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `citygrind.level_counter.anchor_and_caller` (HISTORICAL: that rule is superseded or falsified, so this is not a current-behaviour statement): sub_69BA4 (UILevelCtrliPad's own digit-render routine) has exactly 4 direct callers (xrefs_to, more:false): sub_5EEEC (main-menu-only, already ruled out), sub_5FBA8/sub_5FC00 (not CityGrid-relevant), and sub_56450 - the SOLE CityGrid-relevant caller, itself re

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0x5FBA8  90b5         PUSH            {R4,R7,LR}
0x5FBAA  0446         MOV             R4, R0
0x5FBAC  01af         ADD             R7, SP, #4
0x5FBAE  a068         LDR             R0, [R4,#8]
0x5FBB0  0128         CMP             R0, #1
0x5FBB2  08bf         IT EQ
0x5FBB4  90bd         POPEQ           {R4,R7,PC}
0x5FBB6  0538         SUBS            R0, #5
0x5FBB8  0128         CMP             R0, #1
0x5FBBA  b8bf         IT LT
0x5FBBC  0120         MOVLT           R0, #1
0x5FBBE  a060         STR             R0, [R4,#8]
0x5FBC0  6068         LDR             R0, [R4,#4]
0x5FBC2  1cf1d1fe     BL              sub_17C968
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
int __fastcall sub_5FBA8(int a1)
{
  int result; // r0
  int v3; // r0
  int v4; // r0
  const void *v5; // r0
  _DWORD *v6; // r2

  result = *(_DWORD *)(a1 + 8);
  if ( result != 1 )
  {
    v3 = result - 5;
    if ( v3 < 1 )
      v3 = 1;
    *(_DWORD *)(a1 + 8) = v3;
    v4 = sub_17C968(*(_DWORD *)(a1 + 4));
    v5 = (const void *)sub_185F78(v4, 6);
    v6 = 0;
    if ( v5 )
      v6 = __dynamic_cast(
```

## References found in the code

Strings referenced: none

Globals referenced: none among the named globals

Callers (with call sites): `0x5D380`@`0x5D68C`

Callees: `0x17C968`, `0x185F78`, `0x1B76A8`

## Unity translation

- Code comments citing this address (1): `Runtime/MainMenuController.cs:592`

## Validation

- No test is individually linked to this function.

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
