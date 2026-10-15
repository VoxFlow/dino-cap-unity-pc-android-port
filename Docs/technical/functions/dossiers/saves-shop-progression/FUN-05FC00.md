# sub_5FC00: citygrind.level_counter.anchor_and_caller

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-05FC00` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x5FC00` |
| RVA (EA - 0x4000) | `0x5BC00` |
| File offset | `0x5BC00` |
| Size | 122 bytes, 42 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_5FC00` (IDA automatic name) |
| Bytes SHA-256 | `50e8d5ccae0cb15a9238abf20c6845bef6b1852e7c7d6ea33e1a2e4869c8092e` |
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
0x5FC00  90b5         PUSH            {R4,R7,LR}
0x5FC02  0446         MOV             R4, R0
0x5FC04  4cf20840c0f21900 MOV             R0, #(off_1FC01C - 0x5FC14)
0x5FC0C  46f26761     MOVW            R1, #0x6667
0x5FC10  7844         ADD             R0, PC; off_1FC01C
0x5FC12  c6f26661     MOVT            R1, #0x6666
0x5FC16  01af         ADD             R7, SP, #4
0x5FC18  0068         LDR             R0, [R0]; dword_214FB4
0x5FC1A  0068         LDR             R0, [R0]
0x5FC1C  50fb01f1     SMMUL           R1, R0, R1
0x5FC20  a068         LDR             R0, [R4,#8]
0x5FC22  4a10         ASRS            R2, R1, #1
0x5FC24  02ebd171     ADD.W           R1, R2, R1,LSR#31
0x5FC28  01eb8101     ADD.W           R1, R1, R1,LSL#2
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
int __fastcall sub_5FC00(int a1)
{
  int result; // r0
  int v3; // r1
  int v4; // r0
  const void *v5; // r0
  void *v6; // r2

  result = *(_DWORD *)(a1 + 8);
  if ( result < 5 * (dword_214FB4 / 5) )
  {
    v3 = result + 5;
    if ( result == 1 )
      v3 = 5;
    *(_DWORD *)(a1 + 8) = v3;
    v4 = sub_17C968(*(_DWORD *)(a1 + 4));
    v5 = (const void *)sub_185F78(v4, 6);
    v6 = 0;
    if ( v5 )
      v6 = __dynamic_cast(
```

## References found in the code

Strings referenced: none

Globals referenced: `dword_214FB4`

Callers (with call sites): `0x5D380`@`0x5D6A0`

Callees: `0x17C968`, `0x185F78`, `0x1B76A8`

## Unity translation

- Code comments citing this address (1): `Runtime/MainMenuController.cs:592`

## Validation

- No test is individually linked to this function.

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
