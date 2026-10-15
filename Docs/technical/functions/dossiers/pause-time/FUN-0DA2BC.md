# sub_DA2BC: citygrind.pause_menu.items_are_uimovectrl_with_pressed_shrink

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-0DA2BC` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0xDA2BC` |
| RVA (EA - 0x4000) | `0xD62BC` |
| File offset | `0xD62BC` |
| Size | 94 bytes, 30 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_DA2BC` (IDA automatic name) |
| Bytes SHA-256 | `b2bbe8fccd127fb1518d4d349e390458e752c260b07d26d27605d1f54306684d` |
| Subsystem | pause-time |
| Direct callers | 15 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `citygrind.pause_menu.items_are_uimovectrl_with_pressed_shrink`: PROVEN_NATIVE / IMPLEMENTED / AUTOMATED_FOCUSED_PASS / DEVICE_AB_PENDING |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `citygrind.pause_menu.items_are_uimovectrl_with_pressed_shrink` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): sub_64694 (pauseCtrliPad::Init) constructs all 7 pause-menu items with sub_DA2BC and stores them in vector<DinoCap::UIMoveCtrl*>: the same control class as the main menu buttons (DA6D8): strict original bounds, half-size pressed presentation about the centre, 

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0xDA2BC  90b5         PUSH            {R4,R7,LR}
0xDA2BE  01af         ADD             R7, SP, #4
0xDA2C0  0446         MOV             R4, R0
0xDA2C2  a9f0d5fc     BL              sub_183C70
0xDA2C6  44f2e640     MOVW            R0, #:lower16:(_ZTVN7DinoCap10UIMoveCtrlE - 0xDA2DA); `vtable for'DinoCap::UIMov
0xDA2CA  c0ef5000     VMOV.I32        Q8, #0
0xDA2CE  c0f21200     MOVT            R0, #:upper16:(_ZTVN7DinoCap10UIMoveCtrlE - 0xDA2DA); `vtable for'DinoCap::UIMov
0xDA2D2  04f15401     ADD.W           R1, R4, #0x54 ; 'T'
0xDA2D6  7844         ADD             R0, PC; `vtable for'DinoCap::UIMoveCtrl
0xDA2D8  0830         ADDS            R0, #(off_1FE7C8 - 0x1FE7C0)
0xDA2DA  2060         STR             R0, [R4]
0xDA2DC  04f14000     ADD.W           R0, R4, #0x40 ; '@'
0xDA2E0  40f98f0a     VST1.32         {D16-D17}, [R0]
0xDA2E4  4ff0ff30     MOV.W           R0, #0xFFFFFFFF
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
int __fastcall sub_DA2BC(int a1)
{
  sub_183C70();
  *(_DWORD *)a1 = &off_1FE7C8;
  *(_QWORD *)(a1 + 64) = 0;
  *(_QWORD *)(a1 + 72) = 0;
  *(_DWORD *)(a1 + 80) = -1;
  *(_QWORD *)(a1 + 84) = 0;
  *(_QWORD *)(a1 + 92) = 0;
  *(_DWORD *)(a1 + 100) = -1;
  *(_QWORD *)(a1 + 104) = 0;
  *(_QWORD *)(a1 + 112) = 0;
  *(_DWORD *)(a1 + 120) = -1;
  *(_QWORD *)(a1 + 148) = 0;
  *(_QWORD *)(a1 + 156) = 0;
  *(_BYTE *)(a1 + 132) = 0;
  *(_DWORD *)(a1 + 136) = 0;
  *(_BYTE *)(a1 + 144) = 0;
  *(_DWORD *)(a1 + 140) = 1;
  return a1;
```

## References found in the code

Strings referenced: none

Globals referenced: none among the named globals

Callers (with call sites): `0x193B4`@`0x19470`, `0x64694`@`0x64768`, `0x64694`@`0x647FE`, `0x64694`@`0x64886`, `0x64694`@`0x6490E`, `0x64694`@`0x6498A`, `0x64694`@`0x64A06`, `0x64694`@`0x64A82`, `0xD1444`@`0xD151A`, `0xD1444`@`0xD15B0`, `0xD1444`@`0xD1638`, `0xD1444`@`0xD16C0`

Callees: `0x183C70`

## Unity translation

- No Unity code comment cites this address: **unmapped** (not proof the behaviour is absent from the port).

## Validation

- Tests/DC1BloodBurstPauseRepro.cs
- Tests/DC1PausePointerRepro.cs

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
