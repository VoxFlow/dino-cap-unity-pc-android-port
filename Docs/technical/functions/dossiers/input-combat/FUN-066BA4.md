# sub_66BA4: player.manual_weapon_switch.clears_melee_and_fire_flags

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-066BA4` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x66BA4` |
| RVA (EA - 0x4000) | `0x62BA4` |
| File offset | `0x62BA4` |
| Size | 114 bytes, 38 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_66BA4` (IDA automatic name) |
| Bytes SHA-256 | `05f3e6dd05cccfa159510686b3040868eccf0c9c8adaccf52cbcb77b4cc18569` |
| Subsystem | input-combat |
| Direct callers | 1 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `player.manual_weapon_switch.clears_melee_and_fire_flags`: PROVEN_NATIVE (decompile of sub_68080, callers of sub_65A08) / IMPLEMENTED / AUTOMATED_FOC |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `player.manual_weapon_switch.clears_melee_and_fire_flags` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): The manual next-weapon function sub_68080 sets byte_21BE74 (melee flag) and byte_21BE78 (fire flag) to 0 at the top of its family-advance loop, then (if the fire-active byte +188 is set) calls sub_66310 and clears +188, advances the family (wrapping, skipping 

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0x66BA4  90b5         PUSH            {R4,R7,LR}
0x66BA6  0131         ADDS            R1, #1
0x66BA8  01af         ADD             R7, SP, #4
0x66BAA  0529         CMP             R1, #5
0x66BAC  21dc         BGT             loc_66BF2
0x66BAE  45f28242c0f21902 MOV             R2, #(off_1FC03C - 0x66BBA)
0x66BB6  7a44         ADD             R2, PC; off_1FC03C
0x66BB8  1368         LDR             R3, [R2]; dword_21BE00
0x66BBA  d3f800e0     LDR.W           LR, [R3]
0x66BBE  d3f80c90     LDR.W           R9, [R3,#(dword_21BE0C - 0x21BE00)]
0x66BC2  d3f824c0     LDR.W           R12, [R3,#(dword_21BE24 - 0x21BE00)]
0x66BC6  59f82130     LDR.W           R3, [R9,R1,LSL#2]
0x66BCA  002b         CMP             R3, #0
0x66BCC  0edb         BLT             loc_66BEC
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
int __fastcall sub_66BA4(int a1, int a2)
{
  int v2; // r1
  int v3; // r3

  v2 = a2 + 1;
  if ( v2 > 5 )
    return sub_66C18(a1, dword_21BE30);
  while ( 1 )
  {
    v3 = *(_DWORD *)(dword_21BE0C + 4 * v2);
    if ( v3 >= 0
      && v3 < (unsigned int)((*(_DWORD *)(dword_21BE00 + 12 * v3 + 4) - *(_DWORD *)(dword_21BE00 + 12 * v3)) >> 5)
      && *(int *)(dword_21BE24 + 4 * v2) >= 1 )
    {
      break;
    }
    if ( ++v2 >= 6 )
      return sub_66C18(a1, dword_21BE30);
  }
```

## References found in the code

Strings referenced: none

Globals referenced: `dword_21BE00`, `dword_21BE0C`, `dword_21BE24`, `dword_21BE30`

Callers (with call sites): `0x667B0`@`0x66842`

Callees: none

## Unity translation

- Code comments citing this address (1): `Runtime/CityGridController.cs:6435`

## Validation

- Tests/Task027N236CityGridRepro.cs
- Tests/DC1EightDefectRepro.cs

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
