# sub_78BC8: a2b.part_child_binding

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-078BC8` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x78BC8` |
| RVA (EA - 0x4000) | `0x74BC8` |
| File offset | `0x74BC8` |
| Size | 24 bytes, 9 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_78BC8` (IDA automatic name) |
| Bytes SHA-256 | `670c0530a85ef3b4ca8014d299d14565f489b567045c38d6f4cbd0c3d5306c9f` |
| Subsystem | animation-rendering |
| Direct callers | 70 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `a2b.part_child_binding`: PROVEN; `a2b.weapon_effect_attachment_subsystem`: PROVEN |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `a2b.part_child_binding` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): Core body-part binding between CASprite records and pre-existing CAObjSpriteCallBack children is POSITIONAL INDEX correspondence within sub_76A20 (Nth CASprite in the active frame's own vector <-> Nth child in actorAnimState+112's own array), NOT name-based lo

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0x78BC8  90e80900     LDM.W           R0, {R0,R3}
0x78BCC  1a1a         SUBS            R2, R3, R0
0x78BCE  9210         ASRS            R2, R2, #2
0x78BD0  8a42         CMP             R2, R1
0x78BD2  82bf         ITTT HI
0x78BD4  50f82100     LDRHI.W         R0, [R0,R1,LSL#2]
0x78BD8  0121         MOVHI           R1, #1
0x78BDA  80f82810     STRBHI.W        R1, [R0,#0x28]
0x78BDE  7047         BX              LR
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
int __fastcall sub_78BC8(int *a1, unsigned int a2)
{
  int result; // r0

  result = *a1;
  if ( (a1[1] - result) >> 2 > a2 )
  {
    result = *(_DWORD *)(result + 4 * a2);
    *(_BYTE *)(result + 40) = 1;
  }
  return result;
}
```

## References found in the code

Strings referenced: none

Globals referenced: none among the named globals

Callers (with call sites): `0x2A8A8`@`0x3CEC6`, `0x2A8A8`@`0x3CF4E`, `0x2A8A8`@`0x3CF82`, `0x2A8A8`@`0x3D0D6`, `0x2A8A8`@`0x3D13A`, `0x2A8A8`@`0x3D1AC`, `0x2A8A8`@`0x3D234`, `0x2A8A8`@`0x3D268`, `0x2A8A8`@`0x3D3BC`, `0x2A8A8`@`0x3D420`, `0x2A8A8`@`0x3D4AE`, `0x2A8A8`@`0x3D536`

Callees: none

## Unity translation

- No Unity code comment cites this address: **unmapped** (not proof the behaviour is absent from the port).

## Validation

- No test is individually linked to this function.

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
