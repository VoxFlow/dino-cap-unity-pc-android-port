# sub_DA87C: citygrind.pause.backdrop_alpha_and_bgm_ownership

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-0DA87C` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0xDA87C` |
| RVA (EA - 0x4000) | `0xD687C` |
| File offset | `0xD687C` |
| Size | 90 bytes, 26 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_DA87C` (IDA automatic name) |
| Bytes SHA-256 | `b9fd294c8ff690cf4b10b95184368b93e7094c56714076f3b271a8c9add3a41d` |
| Subsystem | pause-time |
| Direct callers | 106 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `citygrind.pause.backdrop_alpha_and_bgm_ownership`: PROVEN; `citygrind.player_death.timing`: PROVEN |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `citygrind.pause.backdrop_alpha_and_bgm_ownership` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): sub_64694 (pause-menu-contents constructor) sets its whiteFrame backdrop tint via sub_183D94(obj,&v56) with v56=-1073741824 (0xC0000000 ARGB) - alpha byte 0xC0=192, giving a PROVEN alpha fraction of 192/255 (~0.753), replacing a disclosed 0.6 placeholder. Nati

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0xDA87C  61f98f0a     VLD1.32         {D16-D17}, [R1]
0xDA880  d1f81090     LDR.W           R9, [R1,#0x10]
0xDA884  00f14001     ADD.W           R1, R0, #0x40 ; '@'
0xDA888  9ded000a     VLDR            S0, [SP,#arg_0]
0xDA88C  41f98f0a     VST1.32         {D16-D17}, [R1]
0xDA890  c0f85090     STR.W           R9, [R0,#0x50]
0xDA894  62f98f0a     VLD1.32         {D16-D17}, [R2]
0xDA898  1169         LDR             R1, [R2,#0x10]
0xDA89A  00f16802     ADD.W           R2, R0, #0x68 ; 'h'
0xDA89E  9ded012a     VLDR            S4, [SP,#arg_4]
0xDA8A2  42f98f0a     VST1.32         {D16-D17}, [R2]
0xDA8A6  0022         MOVS            R2, #0
0xDA8A8  8167         STR             R1, [R0,#0x78]
0xDA8AA  1969         LDR             R1, [R3,#0x10]
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
int __fastcall sub_DA87C(int result, int a2, int a3, __int64 *a4, float a5, float a6)
{
  __int64 v6; // d17
  int v7; // r9
  __int64 v8; // d17
  int v9; // r1
  __int64 v10; // d16
  __int64 v11; // d17

  v6 = *(_QWORD *)(a2 + 8);
  v7 = *(_DWORD *)(a2 + 16);
  *(_QWORD *)(result + 64) = *(_QWORD *)a2;
  *(_QWORD *)(result + 72) = v6;
  *(_DWORD *)(result + 80) = v7;
  v8 = *(_QWORD *)(a3 + 8);
  v9 = *(_DWORD *)(a3 + 16);
  *(_QWORD *)(result + 104) = *(_QWORD *)a3;
  *(_QWORD *)(result + 112) = v8;
  *(_DWORD *)(result + 120) = v9;
  v10 = *a4;
```

## References found in the code

Strings referenced: none

Globals referenced: none among the named globals

Callers (with call sites): `0x166FC`@`0x1686E`, `0x16928`@`0x16A1A`, `0x17480`@`0x1759E`, `0x5590C`@`0x55A5E`, `0x55F6C`@`0x5601C`, `0x569D0`@`0x56AC0`, `0x56ACC`@`0x56C08`, `0x56ACC`@`0x56D0C`, `0x58954`@`0x58AA8`, `0x58E58`@`0x58F98`, `0x5905C`@`0x591F8`, `0x5DC3C`@`0x5DDCC`

Callees: none

## Unity translation

- Code comments citing this address (1): `Runtime/MainMenuController.cs:105`

## Validation

- Tests/DC1BloodBurstPauseRepro.cs
- Tests/DC1PausePointerRepro.cs

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
