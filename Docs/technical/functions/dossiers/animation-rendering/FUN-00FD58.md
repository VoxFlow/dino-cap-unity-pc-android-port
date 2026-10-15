# sub_FD58: actor.scale_xy.runtime

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-00FD58` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0xFD58` |
| RVA (EA - 0x4000) | `0xBD58` |
| File offset | `0xBD58` |
| Size | 130 bytes, 46 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_FD58` (IDA automatic name) |
| Bytes SHA-256 | `b4628820464f078428ae21a9cbe3cfac61782fc1ceac5da6e58522d653c1b24e` |
| Subsystem | animation-rendering |
| Direct callers | 0 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `actor.scale_xy.runtime`: PROVEN; `citygrind.dino.death_audio_synchronous_no_falling_state`: PROVEN |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `actor.scale_xy.runtime` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): actorAnimState+84 is a SINGLE UNIFORM SCALAR (not a separate X/Y pair as N.2.47R's own report had framed it), applied identically to CASprite.X, CASprite.Y, and the CASprite+8/+12 fields inside sub_76A20. Its shared base-class constructor (sub_764C4, exhaustiv

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0xFD58  90b5         PUSH            {R4,R7,LR}
0xFD5A  01af         ADD             R7, SP, #4
0xFD5C  82b0         SUB             SP, SP, #8
0xFD5E  0446         MOV             R4, R0
0xFD60  66f05efe     BL              sub_76A20
0xFD64  d4f8ac00     LDR.W           R0, [R4,#0xAC]
0xFD68  c11e         SUBS            R1, R0, #3
0xFD6A  0529         CMP             R1, #5
0xFD6C  01d2         BCS             loc_FD72
0xFD6E  02b0         ADD             SP, SP, #8
0xFD70  90bd         POP             {R4,R7,PC}
0xFD72  0828         CMP             R0, #8
0xFD74  1dd1         BNE             loc_FDB2
0xFD76  6068         LDR             R0, [R4,#4]
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
int __fastcall sub_FD58(_DWORD *a1)
{
  float32x2_t v1; // d0
  float32x2_t v2; // d1
  int result; // r0
  _DWORD v5[2]; // [sp+0h] [bp-Ch] BYREF

  sub_76A20();
  result = a1[43];
  if ( (unsigned int)(result - 3) >= 5 )
  {
    if ( result == 8 )
    {
      v1.i32[0] = 1131413504;
      v5[0] = *(_DWORD *)sub_172C78(a1[1]);
      v2.f32[0] = flt_214FD4;
      v5[1] = vadd_f32(v2, v1).u32[0];
      sub_172BD0(a1[1], v5);
      a1[21] = 1065353216;
    }
```

## References found in the code

Strings referenced: none

Globals referenced: `flt_214FD4`, `flt_FDDC`

Callers (with call sites): none by direct reference

Callees: `0xFDE0`, `0xFE50`, `0xFEF0`, `0xFF68`, `0x76A20`, `0x172BD0`, `0x172C78`

## Unity translation

- Code comments citing this address (13): `Runtime/CityGridController.cs:448`, `Runtime/CityGridController.cs:4319`, `Runtime/CityGridController.cs:6472`, `Runtime/CityGridController.cs:6568`, `Runtime/CityGridController.cs:6602`, `Runtime/CityGridController.cs:6606`, `Runtime/CityGridController.cs:6627`, `Runtime/CombatSandboxV2.cs:411` ...

## Validation

- No test is individually linked to this function.

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
