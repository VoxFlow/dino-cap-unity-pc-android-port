# sub_7D81C: a2b.part_child_binding

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-07D81C` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x7D81C` |
| RVA (EA - 0x4000) | `0x7981C` |
| File offset | `0x7981C` |
| Size | 2682 bytes, 710 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_7D81C` (IDA automatic name) |
| Bytes SHA-256 | `b47711ae2ffd653d307e7f941eaabc19bcae7698d8c4b3d882f3040301155a86` |
| Subsystem | doors-buildings |
| Direct callers | 17 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `a2b.part_child_binding`: PROVEN; `a2b.weapon_effect_attachment_subsystem`: PROVEN; `citygrind.house.e_family_bazooka_overlaps_body_in_original`: PROVEN_USER_OBSERVATION_SHIPPED_QUIRK; `citygrind.player.house_family_full_c2b_merge`: PROVEN |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `a2b.part_child_binding` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): Core body-part binding between CASprite records and pre-existing CAObjSpriteCallBack children is POSITIONAL INDEX correspondence within sub_76A20 (Nth CASprite in the active frame's own vector <-> Nth child in actorAnimState+112's own array), NOT name-based lo

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0x7D81C  f0b5         PUSH            {R4-R7,LR}; int
0x7D81E  03af         ADD             R7, SP, #0xC
0x7D820  0628         CMP             R0, #6; switch 7 cases
0x7D822  00f24682     BHI.W           def_7D826; jumptable 0007D826 default case
0x7D826  dfe810f0     TBH.W           [PC,R0,LSL#1]; switch jump
0x7D838  0329         CMP             R1, #3; jumptable 0007D826 case 0
0x7D83A  00f23a82     BHI.W           def_7D826; jumptable 0007D826 default case
0x7D83E  dfe811f0     TBH.W           [PC,R1,LSL#1]; switch jump
0x7D84A  4df6de45c0f21905 MOV             R5, #(unk_21B53C - 0x7D85E); jumptable 0007D83E case 0
0x7D852  4df6bc31c0f21901 MOV             R1, #(unk_21B41C - 0x7D860)
0x7D85A  7d44         ADD             R5, PC; unk_21B53C
0x7D85C  7944         ADD             R1, PC; unk_21B41C ; int
0x7D85E  2846         MOV             R0, R5; int
0x7D860  fbf742f8     BL              sub_788E8
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
int __fastcall sub_7D81C(int result, int a2, int a3, int a4)
{
  void *v4; // r4
  int v5; // r2
  int v6; // r3
  int v7; // r2
  int v8; // r3
  int v9; // r1
  int v10; // r2
  int v11; // r3
  int v12; // r2
  int v13; // r3
  int v14; // r2
  int v15; // r3
  void *v16; // r5
  void *v17; // r1
  int v18; // r2
  int v19; // r3
  int v20; // r2
  int v21; // r3
```

## References found in the code

Strings referenced: none

Globals referenced: none among the named globals

Callers (with call sites): `0x13FD4`@`0x14962`, `0x1B13C`@`0x1B286`, `0x1B13C`@`0x1B496`, `0x1B13C`@`0x1B4E2`, `0x1B13C`@`0x1B53A`, `0x2A8A8`@`0x2AB76`, `0x2A8A8`@`0x3DB38`, `0x6F748`@`0x71CA6`, `0x7EEC8`@`0x7EFBA`, `0x8161C`@`0x81EEC`, `0x88360`@`0x884AA`, `0x88360`@`0x886C0`

Callees: `0x788E8`, `0x78A38`, `0x78B44`, `0x78BC8`, `0x78BE0`, `0x78D64`

## Unity translation

- Code comments citing this address (1): `Runtime/Power2DSourceTruthRenderer.cs:575`

## Validation

- No test is individually linked to this function.

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
