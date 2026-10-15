# sub_EFB4: citygrind.n251.lifecycle_visual_menu.popup

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-00EFB4` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0xEFB4` |
| RVA (EA - 0x4000) | `0xAFB4` |
| File offset | `0xAFB4` |
| Size | 414 bytes, 121 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_EFB4` (IDA automatic name) |
| Bytes SHA-256 | `be327e778a4a033d856d4c408a590f094f1e345e744c24975dfd556b189161ee` |
| Subsystem | doors-buildings |
| Direct callers | 0 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `building.loot_popup.removal_mechanism`: PARTIALLY_SUPERSEDED_BY_DIRECT_NATIVE_EVIDENCE; `citygrind.building.loot_grant_timing`: PARTIALLY_SUPERSEDED_BY_DIRECT_NATIVE_EVIDENCE; `citygrind.building.loot_popup_reused_slot_no_time_removal`: PARTIALLY_SUPERSEDED_BY_DIRECT_NATIVE_EVIDENCE; `citygrind.n251.lifecycle_visual_menu.popup`: PROVEN |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `citygrind.n251.lifecycle_visual_menu.popup` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): sub_D004 uses mainSceneiPad+0x84 as one scene-wide item slot, with world anchor from house com2. sub_F154 calls GetPhysics and sub_173A98 with linear velocity(0,50); sub_173B50 clears gravity. sub_EFB4 grows scalar0.5->1 at5/sec while reading the moving root. 

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0xEFB4  f0b5         PUSH            {R4-R7,LR}
0xEFB6  03af         ADD             R7, SP, #0xC
0xEFB8  2de90005     PUSH.W          {R8,R10}
0xEFBC  adf11004     SUB.W           R4, SP, #0x10
0xEFC0  24f00f04     BIC.W           R4, R4, #0xF
0xEFC4  a546         MOV             SP, R4
0xEFC6  04f9ef8a     VST1.64         {D8-D9}, [R4@128]
0xEFCA  88b0         SUB             SP, SP, #0x20
0xEFCC  c1ef140f     VMOV.F32        D16, #5.0
0xEFD0  0446         MOV             R4, R0
0xEFD2  41ec321b     VMOV            D18, R1, R1
0xEFD6  94ed080a     VLDR            S0, [R4,#0x20]
0xEFDA  42ffb02d     VMUL.F32        D18, D18, D16
0xEFDE  c7ef100f     VMOV.F32        D16, #1.0
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
int __fastcall sub_EFB4(int *a1, __int32 a2)
{
  float32x2_t v2; // d0
  float32x2_t v3; // d1
  float32x2_t v4; // d2
  float32x2_t v5; // d3
  float32x2_t v6; // d4
  float32x2_t v7; // d8
  float32x2_t v8; // d9
  float32x2_t v9; // d16
  float32x2_t v11; // d18
  float32x2_t v12; // d18
  float32x2_t v13; // d0
  __int32 *v14; // r0
  float32x2_t v15; // d3
  float32x2_t v16; // d16
  float32x2_t v17; // d0
  float32x2_t v18; // d2
  float32x2_t v19; // d1
  int v20; // r0
```

## References found in the code

Strings referenced: none

Globals referenced: none among the named globals

Callers (with call sites): none by direct reference

Callees: `0x172BCC`, `0x172BD0`, `0x172C78`, `0x172C7C`

## Unity translation

- Code comments citing this address (1): `Runtime/CityGridController.cs:3172`

## Validation

- No test is individually linked to this function.

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
