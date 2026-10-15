# sub_5EEEC: citygrind.level_counter.anchor_and_caller

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-05EEEC` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x5EEEC` |
| RVA (EA - 0x4000) | `0x5AEEC` |
| File offset | `0x5AEEC` |
| Size | 1734 bytes, 558 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_5EEEC` (IDA automatic name) |
| Bytes SHA-256 | `6a3646aab1cd570da9743c852bd1cb3d077faf8ddecc4a8de88391571ea54154` |
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
0x5EEEC  f0b5         PUSH            {R4-R7,LR}
0x5EEEE  03af         ADD             R7, SP, #0xC
0x5EEF0  2de9000d     PUSH.W          {R8,R10,R11}
0x5EEF4  adf14004     SUB.W           R4, SP, #0x40 ; '@'
0x5EEF8  24f00f04     BIC.W           R4, R4, #0xF
0x5EEFC  a546         MOV             SP, R4
0x5EEFE  04f9ed82     VST1.64         {D8-D11}, [R4@128]!
0x5EF02  04f9efc2     VST1.64         {D12-D15}, [R4@128]
0x5EF06  b0b0         SUB             SP, SP, #0xC0
0x5EF08  c0ef5000     VMOV.I32        Q8, #0
0x5EF0C  19a9         ADD             R1, SP, #0x100+var_9C
0x5EF0E  4ff0ff32     MOV.W           R2, #0xFFFFFFFF
0x5EF12  0690         STR             R0, [SP,#0x100+var_E8]
0x5EF14  41f98f0a     VST1.32         {D16-D17}, [R1]
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
void __fastcall sub_5EEEC(int a1)
{
  float32x2_t v1; // d0
  float32x2_t v2; // d1
  float32x2_t v3; // d4
  float32x2_t v4; // d8
  float32x2_t v5; // d9
  __int64 v6; // d10
  __int64 v7; // d11
  __int64 v8; // d12
  __int64 v9; // d13
  __int64 v10; // d14
  __int64 v11; // d15
  int v12; // r0
  float32x2_t v13; // d16
  float32x2_t v14; // d1
  float32x2_t v15; // d2
  float32x2_t v16; // d4
  int v17; // r0
  float32x2_t v18; // d16
```

## References found in the code

Strings referenced: `level_rightFrame` (`0x1D7085`), `level_leftFrame` (`0x1D7096`), `level_goFrame` (`0x1D70A6`), `level_backFrame` (`0x1D70B4`)

Globals referenced: `flt_21A004`, `flt_21A008`, `flt_21A014`, `flt_21A018`, `flt_21A024`, `flt_21A028`, `flt_21A034`, `flt_21A038`, `flt_21A044`, `flt_21A048`, `flt_5F090`, `flt_5F094`

Callers (with call sites): `0x5D380`@`0x5DB50`

Callees: `0x69B48`, `0x69BA4`, `0xDA87C`, `0x17C968`, `0x183D54`, `0x1B4708`, `0x1B4768`, `0x1B7478`, `0x1B7508`, `0x1B7778`, `0x1B7788`, `0x1B7798`

## Unity translation

- Code comments citing this address (6): `Runtime/CityGridController.cs:6351`, `Runtime/CityGridController.cs:7991`, `Runtime/CityGridController.cs:8598`, `Runtime/CityGridController.cs:8600`, `Runtime/CityGridController.cs:8604`, `Runtime/MainMenuController.cs:26`

## Validation

- No test is individually linked to this function.

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
