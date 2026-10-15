# sub_C8E38: actor.scale_xy.runtime

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-0C8E38` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0xC8E38` |
| RVA (EA - 0x4000) | `0xC4E38` |
| File offset | `0xC4E38` |
| Size | 88 bytes, 30 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_C8E38` (IDA automatic name) |
| Bytes SHA-256 | `90fb1ee81c858cf1dbb1ae01286d8bc988c5ba976926e21b1cdd7c0974ec2fce` |
| Subsystem | animation-rendering |
| Direct callers | 0 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `actor.scale_xy.runtime`: PROVEN |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `actor.scale_xy.runtime` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): actorAnimState+84 is a SINGLE UNIFORM SCALAR (not a separate X/Y pair as N.2.47R's own report had framed it), applied identically to CASprite.X, CASprite.Y, and the CASprite+8/+12 fields inside sub_76A20. Its shared base-class constructor (sub_764C4, exhaustiv

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0xC8E38  90b5         PUSH            {R4,R7,LR}
0xC8E3A  01af         ADD             R7, SP, #4
0xC8E3C  2ded028b     VPUSH           {D8}
0xC8E40  86b0         SUB             SP, SP, #0x18
0xC8E42  0446         MOV             R4, R0
0xC8E44  adf7ecfd     BL              sub_76A20
0xC8E48  6068         LDR             R0, [R4,#4]
0xC8E4A  a9f015ff     BL              sub_172C78
0xC8E4E  90ed008a     VLDR            S16, [R0]
0xC8E52  6068         LDR             R0, [R4,#4]
0xC8E54  a9f07cff     BL              sub_172D50
0xC8E58  0146         MOV             R1, R0
0xC8E5A  04a8         ADD             R0, SP, #0x18+var_8
0xC8E5C  aaf02efe     BL              sub_173ABC
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
_QWORD *__fastcall sub_C8E38(int a1, __int32 a2)
{
  float v3; // s16
  int v4; // r0
  int v5; // r0
  int v6; // r0
  _QWORD *result; // r0
  __int64 v8; // [sp+0h] [bp-24h] BYREF
  int v9; // [sp+10h] [bp-14h] BYREF

  sub_76A20(a1, a2);
  v3 = *(float *)sub_172C78(*(_DWORD *)(a1 + 4));
  v4 = sub_172D50(*(_DWORD *)(a1 + 4));
  sub_173ABC(&v9, v4);
  v5 = sub_172D14(*(_DWORD *)(a1 + 4));
  v6 = sub_17C964(v5);
  result = sub_16669C(&v8, v6);
  if ( v3 > 340.0 )
    return (_QWORD *)sub_172BC8(*(_DWORD *)(a1 + 4), 0);
  return result;
```

## References found in the code

Strings referenced: none

Globals referenced: `flt_C8E90`

Callers (with call sites): none by direct reference

Callees: `0x76A20`, `0x16669C`, `0x172BC8`, `0x172C78`, `0x172D14`, `0x172D50`, `0x173ABC`, `0x17C964`

## Unity translation

- No Unity code comment cites this address: **unmapped** (not proof the behaviour is absent from the port).

## Validation

- No test is individually linked to this function.

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
