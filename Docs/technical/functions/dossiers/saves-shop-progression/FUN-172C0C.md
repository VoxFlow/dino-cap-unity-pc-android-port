# sub_172C0C: citygrind.decoration_prop_worldy.native_contract

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-172C0C` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x172C0C` |
| RVA (EA - 0x4000) | `0x16EC0C` |
| File offset | `0x16EC0C` |
| Size | 108 bytes, 38 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_172C0C` (IDA automatic name) |
| Bytes SHA-256 | `ac1b2ec63258f2d92bf47db201cffba5060ae54d06af61aa83dfe57ecefa2f29` |
| Subsystem | saves-shop-progression |
| Direct callers | 109 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `citygrind.decoration_prop_worldy.native_contract`: PROVEN; `citygrind.shop_screen.entry_trigger_and_layout`: PROVEN (fade mechanism/duration, relative layout) / HARD_BLOCKED (literal entry-trigger ca; `citygrind.shop_screen.weapon_icon_anchors`: PROVEN |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `citygrind.decoration_prop_worldy.native_contract` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): Native decoration (zhuangshi1/2/3, i.e. street-prop) Y placement is PROVEN via 4 total bounded IDA rounds this reopening: Y_native_center = flt_214FD4 (PROVEN -104.0 for this port's own iPad profile, native device-init sub_7EDA8, testApp::OnLoad) + perTypeBase

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0x172C0C  90b5         PUSH            {R4,R7,LR}
0x172C0E  adf11004     SUB.W           R4, SP, #0x10
0x172C12  01af         ADD             R7, SP, #4
0x172C14  24f00f04     BIC.W           R4, R4, #0xF
0x172C18  a546         MOV             SP, R4
0x172C1A  04f9ef8a     VST1.64         {D8-D9}, [R4@128]
0x172C1E  84b0         SUB             SP, SP, #0x10
0x172C20  0446         MOV             R4, R0
0x172C22  0b46         MOV             R3, R1
0x172C24  2146         MOV             R1, R4
0x172C26  42ec182b     VMOV            D8, R2, R2
0x172C2A  41f8203f     STR.W           R3, [R1,#0x20]!
0x172C2E  43ec193b     VMOV            D9, R3, R3
0x172C32  6262         STR             R2, [R4,#0x24]
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
int __fastcall sub_172C0C(float *a1, float a2, float a3)
{
  __int64 v3; // d8
  __int64 v4; // d9
  _DWORD *v8; // r1
  float v9; // s18
  int v10; // r0
  int v11; // r0
  int result; // r0
  float v13[2]; // [sp+0h] [bp-28h] BYREF
  float v14[2]; // [sp+8h] [bp-20h] BYREF
  __int64 v15; // [sp+14h] [bp-14h]
  __int64 v16; // [sp+1Ch] [bp-Ch]

  v15 = v3;
  v16 = v4;
  a1[8] = a2;
  v8 = a1 + 8;
  v9 = a2;
  a1[9] = a3;
```

## References found in the code

Strings referenced: none

Globals referenced: none among the named globals

Callers (with call sites): `0xD308`@`0xD3D2`, `0xDAA0`@`0xE190`, `0xEC70`@`0xED42`, `0xEC70`@`0xEE28`, `0x13918`@`0x139BE`, `0x139C8`@`0x13A40`, `0x13FD4`@`0x14A24`, `0x13FD4`@`0x14BFC`, `0x13FD4`@`0x150F4`, `0x1AD48`@`0x1AE32`, `0x1AD48`@`0x1AF32`, `0x53330`@`0x53C7E`

Callees: `0x1732DC`, `0x173A2C`, `0x18B5C4`

## Unity translation

- Code comments citing this address (9): `Runtime/CityGridController.cs:862`, `Runtime/CityGridController.cs:864`, `Runtime/CityGridController.cs:1204`, `Runtime/CityGridController.cs:3572`, `Runtime/CityGridController.cs:3591`, `Runtime/CityGridController.cs:3604`, `Runtime/CityGridController.cs:3891`, `Runtime/CityGridController.cs:8032` ...

## Validation

- No test is individually linked to this function.

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
