# sub_77DE4: a2b.weapon_effect_attachment_subsystem

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-077DE4` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x77DE4` |
| RVA (EA - 0x4000) | `0x73DE4` |
| File offset | `0x73DE4` |
| Size | 74 bytes, 29 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_77DE4` (IDA automatic name) |
| Bytes SHA-256 | `f7da1b2941b5266ce7e89431345bd99fed3b4a7accc06fb45f95b1b80b6c9341` |
| Subsystem | input-combat |
| Direct callers | 1 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `a2b.weapon_effect_attachment_subsystem`: PROVEN |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `a2b.weapon_effect_attachment_subsystem` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): sub_7D554 lazily one-time-initializes a pool of ~6 static attachment-template slots (guarded by byte_21C3C4). sub_7D81C is a runtime per-request switch-dispatcher (17 exhaustive callers: weapon/effect functions incl. sub_A1518, sub_E4BA0, sub_8161C, plus the l

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0x77DE4  b0b5         PUSH            {R4,R5,R7,LR}
0x77DE6  0d46         MOV             R5, R1
0x77DE8  0446         MOV             R4, R0
0x77DEA  95e80300     LDM.W           R5, {R0,R1}
0x77DEE  02af         ADD             R7, SP, #8
0x77DF0  84e80300     STM.W           R4, {R0,R1}
0x77DF4  a868         LDR             R0, [R5,#8]
0x77DF6  e968         LDR             R1, [R5,#0xC]
0x77DF8  e160         STR             R1, [R4,#0xC]
0x77DFA  a060         STR             R0, [R4,#8]
0x77DFC  2869         LDR             R0, [R5,#0x10]
0x77DFE  6969         LDR             R1, [R5,#0x14]
0x77E00  6161         STR             R1, [R4,#0x14]
0x77E02  05f12001     ADD.W           R1, R5, #0x20 ; ' '; std::string *
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
int __fastcall sub_77DE4(_DWORD *a1, int *a2)
{
  int v4; // r0
  int v5; // r1
  int v6; // r0
  int v7; // r0
  int result; // r0

  v4 = *a2;
  v5 = a2[1];
  *a1 = v4;
  a1[1] = v5;
  v6 = a2[2];
  a1[3] = a2[3];
  a1[2] = v6;
  v7 = a2[4];
  a1[5] = a2[5];
  a1[4] = v7;
  a1[6] = a2[6];
  a1[7] = a2[7];
```

## References found in the code

Strings referenced: none

Globals referenced: none among the named globals

Callers (with call sites): `0x78398`@`0x78486`

Callees: `0x1B74C8`

## Unity translation

- No Unity code comment cites this address: **unmapped** (not proof the behaviour is absent from the port).

## Validation

- Tests/Task027N236CityGridRepro.cs
- Tests/DC1EightDefectRepro.cs

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
