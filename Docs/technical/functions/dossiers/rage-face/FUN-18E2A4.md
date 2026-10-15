# sub_18E2A4: citygrind.rage_face.native_fireup_timer_and_erase_events

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-18E2A4` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x18E2A4` |
| RVA (EA - 0x4000) | `0x18A2A4` |
| File offset | `0x18A2A4` |
| Size | 52 bytes, 21 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_18E2A4` (IDA automatic name) |
| Bytes SHA-256 | `bdd41b20def6b616e29954bc2a93584f56ce5a2b50df7fc8870d6c01034a1a3c` |
| Subsystem | rage-face |
| Direct callers | 1 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `citygrind.rage_face.native_fireup_timer_and_erase_events`: PROVEN_NATIVE_ARM / IMPLEMENTED (City Grind) / AUTOMATED_FOCUSED_PASS / DEVICE_AB_PENDING |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `citygrind.rage_face.native_fireup_timer_and_erase_events` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): Rage face = player string at +108 ('_face1/2/3') set by sub_66DCC on every C/D (families 2/3) discharge from the counter at +132 (counter after increment 10..29 -> _face1, 30..49 -> _face2, >=50 -> _face3, plus a once-only 'dinocap.a8' award at 50). The counte

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0x18E2A4  f0b5         PUSH            {R4-R7,LR}
0x18E2A6  03af         ADD             R7, SP, #0xC
0x18E2A8  82b0         SUB             SP, SP, #8
0x18E2AA  0c46         MOV             R4, R1
0x18E2AC  40f21b01     MOVW            R1, #:lower16:(sub_18E2D8+1 - 0x18E2BE)
0x18E2B0  0546         MOV             R5, R0
0x18E2B2  c0f20001     MOVT            R1, #:upper16:(sub_18E2D8+1 - 0x18E2BE)
0x18E2B6  55f8340b     LDR.W           R0, [R5],#0x34
0x18E2BA  7944         ADD             R1, PC; sub_18E2D8
0x18E2BC  be68         LDR             R6, [R7,#-0xC+arg_0]
0x18E2BE  0096         STR             R6, [SP,#8+var_8]
0x18E2C0  f3f79afe     BL              sub_181FF8
0x18E2C4  01a9         ADD             R1, SP, #8+var_4
0x18E2C6  0190         STR             R0, [SP,#8+var_4]
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
int __fastcall sub_18E2A4(_DWORD *a1, int a2, int a3, int a4, char a5)
{
  _DWORD *v6; // r5
  int v7; // r0
  int v9; // [sp+4h] [bp-10h] BYREF

  v6 = a1 + 13;
  v9 = sub_181FF8(*a1, sub_18E2D8);
  v7 = std::map<int,std::pair<Triniti2D::GameObject *,bool>>::operator[](v6, &v9);
  *(_DWORD *)v7 = a2;
  *(_BYTE *)(v7 + 4) = a5;
  return v9;
}
```

## References found in the code

Strings referenced: none

Globals referenced: none among the named globals

Callers (with call sites): `0x172DEC`@`0x172E0A`

Callees: `0x181FF8`, `0x1B6B38`

## Unity translation

- Code comments citing this address (1): `Runtime/CityGridController.cs:5308`

## Validation

- No test is individually linked to this function.

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
