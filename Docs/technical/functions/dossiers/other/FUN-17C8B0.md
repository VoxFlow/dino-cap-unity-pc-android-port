# sub_17C8B0: citygrind.player_death.dieend_timing

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-17C8B0` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x17C8B0` |
| RVA (EA - 0x4000) | `0x1788B0` |
| File offset | `0x1788B0` |
| Size | 58 bytes, 23 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_17C8B0` (IDA automatic name) |
| Bytes SHA-256 | `67296bdfc10f0344649eb65aef1ac0b852e2ad28a143b9928ad1ab67addecfa2` |
| Subsystem | other |
| Direct callers | 118 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `citygrind.player_death.dieend_timing`: PROVEN; `citygrind.player_death.timing`: PROVEN |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `citygrind.player_death.dieend_timing` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): Native player-death sequencing (mainSceneiPad's generic OnEvent switch, sub_56118) is two PROVEN phases: DieAnim/'p_die' plays to completion (31 frames @ 0.025s = 0.775s, already-established manifest data), then once scene-state reaches 9, event-type-16 uncond

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0x17C8B0  f0b5         PUSH            {R4-R7,LR}
0x17C8B2  03af         ADD             R7, SP, #0xC
0x17C8B4  4df8048d     PUSH.W          {R8}
0x17C8B8  82b0         SUB             SP, SP, #8
0x17C8BA  1c46         MOV             R4, R3
0x17C8BC  9046         MOV             R8, R2
0x17C8BE  0e46         MOV             R6, R1
0x17C8C0  0546         MOV             R5, R0
0x17C8C2  11f05ffb     BL              sub_18DF84
0x17C8C6  2946         MOV             R1, R5
0x17C8C8  3246         MOV             R2, R6
0x17C8CA  4346         MOV             R3, R8
0x17C8CC  0094         STR             R4, [SP,#8+var_8]
0x17C8CE  11f031fc     BL              sub_18E134
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
int __fastcall sub_17C8B0(int a1, int a2, int a3, int a4)
{
  int v8; // r0
  int v10; // [sp+4h] [bp-14h] BYREF

  v8 = sub_18DF84();
  v10 = sub_18E134(v8, a1, a2, a3, a4);
  *(_BYTE *)std::map<int,bool>::operator[](a1 + 2908, &v10) = a4;
  return v10;
}
```

## References found in the code

Strings referenced: none

Globals referenced: none among the named globals

Callers (with call sites): `0x17650`@`0x178C2`, `0x17A08`@`0x17A38`, `0x17A08`@`0x17A74`, `0x17A08`@`0x17A86`, `0x17A08`@`0x17A98`, `0x17A08`@`0x17AAA`, `0x1802C`@`0x1817C`, `0x1802C`@`0x181E6`, `0x1802C`@`0x181FA`, `0x1802C`@`0x1820E`, `0x1802C`@`0x18222`, `0x1802C`@`0x18280`

Callees: `0x18DF84`, `0x18E134`, `0x1B5B68`

## Unity translation

- No Unity code comment cites this address: **unmapped** (not proof the behaviour is absent from the port).

## Validation

- No test is individually linked to this function.

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
