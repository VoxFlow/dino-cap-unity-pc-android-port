# sub_D05AC: platform.asset_loading.native_loads_gameplay_assets_behind_the_loading_slide

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-0D05AC` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0xD05AC` |
| RVA (EA - 0x4000) | `0xCC5AC` |
| File offset | `0xCC5AC` |
| Size | 28 bytes, 10 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_D05AC` (IDA automatic name) |
| Bytes SHA-256 | `152903c53169adbd31adc4ee2fde87d5a487c2c5ede505001ad3717cf765a9ea` |
| Subsystem | engine-platform |
| Direct callers | 0 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `platform.asset_loading.native_loads_gameplay_assets_behind_the_loading_slide`: PROVEN_NATIVE_TRIGGER_LIKELY_SCOPE |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `platform.asset_loading.native_loads_gameplay_assets_behind_the_loading_slide` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): The original does NOT preload gameplay assets at launch. The menu scene's virtual sub_62A98 (vtable data xref 0x1FDB90) runs sub_2A8A8(this+16, [this+72]) only when its state field [this+12]==4, which is set when the 'Loading...' slide-in control (id 13, text_

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0xD05AC  80b5         PUSH            {R7,LR}
0xD05AE  c168         LDR             R1, [R0,#0xC]
0xD05B0  6f46         MOV             R7, SP
0xD05B2  0429         CMP             R1, #4
0xD05B4  04d1         BNE             loc_D05C0
0xD05B6  90f84810     LDRB.W          R1, [R0,#0x48]
0xD05BA  1030         ADDS            R0, #0x10
0xD05BC  d0f7acff     BL              sub_A1518
0xD05C0  bde88040     POP.W           {R7,LR}
0xD05C4  bff070bd     B.W             sub_1900A8
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
int __fastcall sub_D05AC(int a1)
{
  if ( *(_DWORD *)(a1 + 12) == 4 )
    a1 = sub_A1518(a1 + 16, *(unsigned __int8 *)(a1 + 72));
  return sub_1900A8(a1);
}
```

## References found in the code

Strings referenced: none

Globals referenced: none among the named globals

Callers (with call sites): none by direct reference

Callees: `0xA1518`

## Unity translation

- Code comments citing this address (2): `Runtime/CityGridController.cs:3084`, `Runtime/MainMenuController.cs:470`

## Validation

- No test is individually linked to this function.

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
