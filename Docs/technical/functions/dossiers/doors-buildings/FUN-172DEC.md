# sub_172DEC: citygrind.building_loot_timing.ub_race_policy

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-172DEC` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x172DEC` |
| RVA (EA - 0x4000) | `0x16EDEC` |
| File offset | `0x16EDEC` |
| Size | 58 bytes, 23 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_172DEC` (IDA automatic name) |
| Bytes SHA-256 | `90f6c8ff6184815f2d56e7341f246abfdb9a5fc997ec7a0082e99d4b1181ed97` |
| Subsystem | doors-buildings |
| Direct callers | 17 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `citygrind.building.loot_grant_timing`: PARTIALLY_SUPERSEDED_BY_DIRECT_NATIVE_EVIDENCE; `citygrind.building_loot_timing.ub_race_policy`: PROVEN (mechanism, unchanged from N.2.36.9) / POLICY_CLASSIFIED (INTENTIONAL_SHIPPED_BUG_C; `citygrind.debris_chunk.no_ground_contact_audio`: PROVEN; `citygrind.dino.death_audio_synchronous_no_falling_state`: PROVEN; `citygrind.rage_face.native_fireup_timer_and_erase_events`: PROVEN_NATIVE_ARM / IMPLEMENTED (City Grind) / AUTOMATED_FOCUSED_PASS / DEVICE_AB_PENDING |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `citygrind.building_loot_timing.ub_race_policy` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): Native's door-close/loot-grant timing is a race between a deterministic 1.5s scene-level scheduled timer (sub_CF14->sub_172DEC) and a cancellation event (sub_D108/vtable+40) governed by PROVEN uninitialized heap memory (house+20/+36, exhaustively re-verified t

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0x172DEC  f0b5         PUSH            {R4-R7,LR}
0x172DEE  03af         ADD             R7, SP, #0xC
0x172DF0  4df8048d     PUSH.W          {R8}
0x172DF4  82b0         SUB             SP, SP, #8
0x172DF6  1c46         MOV             R4, R3
0x172DF8  9046         MOV             R8, R2
0x172DFA  0e46         MOV             R6, R1
0x172DFC  0546         MOV             R5, R0
0x172DFE  1bf0c1f8     BL              sub_18DF84
0x172E02  2946         MOV             R1, R5
0x172E04  3246         MOV             R2, R6
0x172E06  4346         MOV             R3, R8
0x172E08  0094         STR             R4, [SP,#8+var_8]
0x172E0A  1bf04bfa     BL              sub_18E2A4
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
int __fastcall sub_172DEC(int a1, int a2, int a3, int a4)
{
  int v8; // r0
  int v10; // [sp+4h] [bp-14h] BYREF

  v8 = sub_18DF84();
  v10 = sub_18E2A4(v8, a1, a2, a3, a4);
  *(_BYTE *)std::map<int,bool>::operator[](a1 + 76, &v10) = a4;
  return v10;
}
```

## References found in the code

Strings referenced: none

Globals referenced: none among the named globals

Callers (with call sites): `0xCF14`@`0xCFC0`, `0x10B90`@`0x10D5E`, `0x13B10`@`0x13CBE`, `0x1AD48`@`0x1AE58`, `0x1AD48`@`0x1AF46`, `0x1B13C`@`0x1B36E`, `0x65A08`@`0x65C2E`, `0x663A0`@`0x664CA`, `0x68148`@`0x68318`, `0x73AA0`@`0x73B4C`, `0x7A9AC`@`0x7AB7A`, `0x81158`@`0x81306`

Callees: `0x18DF84`, `0x18E2A4`, `0x1B5B68`

## Unity translation

- Code comments citing this address (2): `Runtime/CityGridController.cs:199`, `Runtime/CityGridController.cs:5308`

## Validation

- No test is individually linked to this function.

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
