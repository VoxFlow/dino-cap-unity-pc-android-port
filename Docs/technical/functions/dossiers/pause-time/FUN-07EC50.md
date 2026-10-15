# sub_7EC50: citygrind.pause.backdrop_alpha_and_bgm_ownership

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-07EC50` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x7EC50` |
| RVA (EA - 0x4000) | `0x7AC50` |
| File offset | `0x7AC50` |
| Size | 334 bytes, 115 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_7EC50` (IDA automatic name) |
| Bytes SHA-256 | `f6540d96adee8e4c12c9cc0b4b7eced74de832256f84441f0e836bc5e170db8a` |
| Subsystem | pause-time |
| Direct callers | 15 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `citygrind.pause.backdrop_alpha_and_bgm_ownership`: PROVEN; `citygrind.pause_menu.backdrop_alpha_and_bgm_ownership`: PROVEN |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `citygrind.pause.backdrop_alpha_and_bgm_ownership` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): sub_64694 (pause-menu-contents constructor) sets its whiteFrame backdrop tint via sub_183D94(obj,&v56) with v56=-1073741824 (0xC0000000 ARGB) - alpha byte 0xC0=192, giving a PROVEN alpha fraction of 192/255 (~0.753), replacing a disclosed 0.6 placeholder. Nati

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0x7EC50  f0b5         PUSH            {R4-R7,LR}
0x7EC52  03af         ADD             R7, SP, #0xC
0x7EC54  2de9000d     PUSH.W          {R8,R10,R11}
0x7EC58  adf14004     SUB.W           R4, SP, #0x40 ; '@'
0x7EC5C  24f00f04     BIC.W           R4, R4, #0xF
0x7EC60  a546         MOV             SP, R4
0x7EC62  04f9ed82     VST1.64         {D8-D11}, [R4@128]!
0x7EC66  04f9efc2     VST1.64         {D12-D15}, [R4@128]
0x7EC6A  94b0         SUB             SP, SP, #0x50
0x7EC6C  0292         STR             R2, [SP,#0x90+var_88]
0x7EC6E  0446         MOV             R4, R0
0x7EC70  0191         STR             R1, [SP,#0x90+var_8C]
0x7EC72  46f23231c0f21901 MOV             R1, #(byte_214FB8 - 0x7EC86)
0x7EC7A  4df2b032c0f21702 MOV             R2, #(___gxx_personality_sj0_ptr - 0x7EC88)
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
void __fastcall sub_7EC50(int a1, int a2, const char *a3)
{
  __int64 v3; // d8
  __int64 v4; // d9
  __int64 v5; // d10
  __int64 v6; // d11
  __int64 v7; // d12
  __int64 v8; // d13
  __int64 v9; // d14
  __int64 v10; // d15
  unsigned int *v12; // r1
  int v13; // r2
  int v14; // [sp+0h] [bp-A8h]
  _BYTE v17[4]; // [sp+Ch] [bp-9Ch] BYREF
  int v18; // [sp+10h] [bp-98h] BYREF
  __int64 v19; // [sp+50h] [bp-58h]
  __int64 v20; // [sp+58h] [bp-50h]
  __int64 v21; // [sp+60h] [bp-48h]
  __int64 v22; // [sp+68h] [bp-40h]
  __int64 v23; // [sp+70h] [bp-38h]
```

## References found in the code

Strings referenced: none

Globals referenced: `byte_214FB8`, `dword_21C710`

Callers (with call sites): `0x13FD4`@`0x15086`, `0x53330`@`0x542EC`, `0x59DEC`@`0x5B0F0`, `0x60198`@`0x602F6`, `0x640C4`@`0x642AE`, `0x6A4A8`@`0x6B9FA`, `0x8161C`@`0x82690`, `0xC2A70`@`0xC39F8`, `0xC8C6C`@`0xC8C94`, `0xC8C6C`@`0xC8CC0`, `0xC8C6C`@`0xC8CDA`, `0xC91D0`@`0xC9AAA`

Callees: `0x165298`, `0x175B44`, `0x1B7478`, `0x1B7508`, `0x1B7778`, `0x1B7788`, `0x1B7798`

## Unity translation

- Code comments citing this address (2): `Runtime/CityGridController.cs:1762`, `Runtime/CityGridController.cs:1783`

## Validation

- Tests/DC1BloodBurstPauseRepro.cs
- Tests/DC1PausePointerRepro.cs

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
