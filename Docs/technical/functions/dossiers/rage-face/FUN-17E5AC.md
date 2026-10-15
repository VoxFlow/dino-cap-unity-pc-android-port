# sub_17E5AC: rendering.player_face_suffix_composes_with_each_clips_own_head_ref

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-17E5AC` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x17E5AC` |
| RVA (EA - 0x4000) | `0x17A5AC` |
| File offset | `0x17A5AC` |
| Size | 32 bytes, 12 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_17E5AC` (IDA automatic name) |
| Bytes SHA-256 | `d7acc0d2db0be21cfb4b6b73b97acb001197cdd47044b1980fbb23827125a8b3` |
| Subsystem | rage-face |
| Direct callers | 4 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `rendering.player_face_suffix_composes_with_each_clips_own_head_ref`: PROVEN_NATIVE (sub_76A20 decompile + manifest head refs) / IMPLEMENTED / AUTOMATED_FOCUSED |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `rendering.player_face_suffix_composes_with_each_clips_own_head_ref` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): The rage face is a text suffix on the player (+108, '_face1/2/3'), not a head sprite override. In the animation draw sub_76A20, for every part whose animation entry is a sprite (not a nested anim), the sprite name is built as part ref + actor suffix (+27*4 = +

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0x17E5AC  90b5         PUSH            {R4,R7,LR}
0x17E5AE  0446         MOV             R4, R0
0x17E5B0  04f19c00     ADD.W           R0, R4, #0x9C; int
0x17E5B4  01af         ADD             R7, SP, #4
0x17E5B6  38f090eb     BLX             j___ZNSt8_Rb_treeISsSt4pairIKSsiESt10_Select1stIS2_ESt4lessISsESaIS2_EE4findERS
0x17E5BA  04f1a002     ADD.W           R2, R4, #0xA0
0x17E5BE  4ff0ff31     MOV.W           R1, #0xFFFFFFFF
0x17E5C2  9042         CMP             R0, R2
0x17E5C4  18bf         IT NE
0x17E5C6  4169         LDRNE           R1, [R0,#0x14]
0x17E5C8  0846         MOV             R0, R1
0x17E5CA  90bd         POP             {R4,R7,PC}
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
int __fastcall sub_17E5AC(int a1, std::string *a2)
{
  int v3; // r0
  int v4; // r1

  v3 = std::_Rb_tree<std::string,std::pair<std::string const,int>,std::_Select1st<std::pair<std::string const,int>>,std::less<std::string>,s
         a1 + 156,
         a2);
  v4 = -1;
  if ( v3 != a1 + 160 )
    return *(_DWORD *)(v3 + 20);
  return v4;
}
```

## References found in the code

Strings referenced: none

Globals referenced: none among the named globals

Callers (with call sites): `0x76A20`@`0x76F3C`, `0x77A28`@`0x77CA4`, `0x79064`@`0x79164`, `0x1745A8`@`0x1745C6`

Callees: `0x1B6CD8`

## Unity translation

- No Unity code comment cites this address: **unmapped** (not proof the behaviour is absent from the port).

## Validation

- No test is individually linked to this function.

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
