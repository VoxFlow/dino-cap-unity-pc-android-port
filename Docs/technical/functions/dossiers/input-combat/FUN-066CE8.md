# sub_66CE8: citygrind.ammo.family_a_starting_200_source_derived

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-066CE8` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x66CE8` |
| RVA (EA - 0x4000) | `0x62CE8` |
| File offset | `0x62CE8` |
| Size | 226 bytes, 79 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_66CE8` (IDA automatic name) |
| Bytes SHA-256 | `a1f88f59c66ba3461ae5ac41f9b627ec89616136d3e2b694685addb4db4fdb84` |
| Subsystem | input-combat |
| Direct callers | 0 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `citygrind.ammo.family_a_starting_200_source_derived`: PROVEN; `combat.f2_burn.target_cardinality`: PROVEN; `combat.f3_audio.retrigger_mechanism_and_cadence_gap`: PROVEN_NATIVE / IMPLEMENTED / AUTOMATED_FOCUSED_PASS / DEVICE_AB_PENDING |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `citygrind.ammo.family_a_starting_200_source_derived` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): Family A's starting City Grid ammo (dword_21BE24[0]) is unconditionally set to 200 by sub_7EEC8 (the City-Grind-start event), independently of the user's own runtime observation - Task004's own dedicated archaeology (evidence.json) decompiled this chain. Decre

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0x66CE8  f0b5         PUSH            {R4-R7,LR}
0x66CEA  0d46         MOV             R5, R1
0x66CEC  0446         MOV             R4, R0
0x66CEE  03af         ADD             R7, SP, #0xC
0x66CF0  032d         CMP             R5, #3
0x66CF2  38d1         BNE             loc_66D66
0x66CF4  45f23830c0f21900 MOV             R0, #(off_1FC03C - 0x66D04)
0x66CFC  d4f89010     LDR.W           R1, [R4,#0x90]
0x66D00  7844         ADD             R0, PC; off_1FC03C
0x66D02  0668         LDR             R6, [R0]; dword_21BE00
0x66D04  706a         LDR             R0, [R6,#(dword_21BE24 - 0x21BE00)]
0x66D06  50f82120     LDR.W           R2, [R0,R1,LSL#2]
0x66D0A  013a         SUBS            R2, #1
0x66D0C  002a         CMP             R2, #0
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
void __fastcall sub_66CE8(int a1, int a2, int a3, int a4)
{
  int v6; // r1
  int v7; // r0
  int v8; // r2
  int v9; // r0
  const void *v10; // r0
  void *v11; // r1
  unsigned int v12; // r0
  int v13; // r0

  if ( a2 == 3 )
  {
    v6 = *(_DWORD *)(a1 + 144);
    v7 = dword_21BE24;
    v8 = *(_DWORD *)(dword_21BE24 + 4 * v6) - 1;
    if ( v8 <= 0 )
    {
      sub_66310(a1);
      v8 = 0;
```

## References found in the code

Strings referenced: none

Globals referenced: `byte_21BE78`, `dword_21BE00`, `dword_21BE0C`, `dword_21BE24`

Callers (with call sites): none by direct reference

Callees: `0x54DEC`, `0x66310`, `0x172D14`, `0x17C71C`, `0x1B76A8`

## Unity translation

- Code comments citing this address (4): `Runtime/CityGridController.cs:5364`, `Runtime/CombatSandboxV2.cs:428`, `Runtime/CombatSandboxV2.cs:2577`, `Runtime/CombatSandboxV2.cs:3727`

## Validation

- Tests/Task027N236CityGridRepro.cs
- Tests/DC1EightDefectRepro.cs

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
