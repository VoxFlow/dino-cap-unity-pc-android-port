# sub_16CC58: citygrind.pause.moving_particles_freeze

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-16CC58` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x16CC58` |
| RVA (EA - 0x4000) | `0x168C58` |
| File offset | `0x168C58` |
| Size | 34 bytes, 14 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_16CC58` (IDA automatic name) |
| Bytes SHA-256 | `e5ab33d938424a4f72148a0ae920fb556fe8237062e240136fd5f16b5906f57e` |
| Subsystem | pause-time |
| Direct callers | 1 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `citygrind.pause.moving_particles_freeze`: PROVEN_NATIVE_MECHANISM / IMPLEMENTED (Unity vessel = pause gate) / AUTOMATED_FOCUSED_PASS |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `citygrind.pause.moving_particles_freeze` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): Native pause is a scene TIME SCALE of 0, not per-object freezing. Triniti2D scene update sub_17BA5C computes scaledDt = dt * scene[+0xB74] (a1[733]) and feeds scaledDt to: the timer queue (sub_182058 -> PriorityQueue<TimerInfo>::Update), every enabled GameObje

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0x16CC58  b0b5         PUSH            {R4,R5,R7,LR}
0x16CC5A  02af         ADD             R7, SP, #8
0x16CC5C  046b         LDR             R4, [R0,#0x30]
0x16CC5E  00f12805     ADD.W           R5, R0, #0x28 ; '('
0x16CC62  06e0         B               loc_16CC72
0x16CC64  6069         LDR             R0, [R4,#0x14]
0x16CC66  2af025fd     BL              sub_1976B4
0x16CC6A  2046         MOV             R0, R4; _Rb_tree_node_base *
0x16CC6C  4af0bcec     BLX             __ZSt18_Rb_tree_incrementPSt18_Rb_tree_node_base; std::_Rb_tree_increment(std::
0x16CC70  0446         MOV             R4, R0
0x16CC72  ac42         CMP             R4, R5
0x16CC74  08bf         IT EQ
0x16CC76  b0bd         POPEQ           {R4,R5,R7,PC}
0x16CC78  f4e7         B               loc_16CC64
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
int __fastcall sub_16CC58(int result)
{
  _Rb_tree_node_base *v1; // r4
  _Rb_tree_node_base *v2; // r5

  v1 = *(_Rb_tree_node_base **)(result + 48);
  v2 = (_Rb_tree_node_base *)(result + 40);
  while ( v1 != v2 )
  {
    sub_1976B4(v1[1]._M_parent);
    result = std::_Rb_tree_increment(v1);
    v1 = (_Rb_tree_node_base *)result;
  }
  return result;
}
```

## References found in the code

Strings referenced: none

Globals referenced: none among the named globals

Callers (with call sites): `0x17BA5C`@`0x17BE48`

Callees: `0x1976B4`, `0x1B75E8`

## Unity translation

- No Unity code comment cites this address: **unmapped** (not proof the behaviour is absent from the port).

## Validation

- Tests/DC1BloodBurstPauseRepro.cs
- Tests/DC1PausePointerRepro.cs

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
