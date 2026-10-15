# sub_54FA0: citygrind.building.composite_body_native_x_offset

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-054FA0` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x54FA0` |
| RVA (EA - 0x4000) | `0x50FA0` |
| File offset | `0x50FA0` |
| Size | 2402 bytes, 817 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_54FA0` (IDA automatic name) |
| Bytes SHA-256 | `4c925f2c7de7a11aa4ff45aa0602353b7cd50c59008a287fef6faf0a8b208092` |
| Subsystem | doors-buildings |
| Direct callers | 1 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `citygrind.building.composite_body_native_x_offset`: PROVEN_NATIVE_IMPLEMENTED; `citygrind.building.composite_sprite_draw_layer10`: PROVEN; `citygrind.building.draw_key_zero_from_base_ctor`: FALSIFIED; `citygrind.building_construction_scale.native_contract`: PROVEN; `citygrind.decoration_prop_worldy.native_contract`: PROVEN |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `citygrind.building.composite_body_native_x_offset` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): A City building is a composite whose parts are positioned from the building's OBJECT ORIGIN (the x set by sub_54FA0: 853 + 1365*i + int(rand*200)). The body (a2b com1) is NOT centred on the origin: its centre x is the IR's com1 x (house1 -160.0, house2-type sk

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0x54FA0  f0b5         PUSH            {R4-R7,LR}
0x54FA2  03af         ADD             R7, SP, #0xC
0x54FA4  2de9000d     PUSH.W          {R8,R10,R11}
0x54FA8  adf14004     SUB.W           R4, SP, #0x40 ; '@'
0x54FAC  24f00f04     BIC.W           R4, R4, #0xF
0x54FB0  a546         MOV             SP, R4
0x54FB2  04f9ed82     VST1.64         {D8-D11}, [R4@128]!
0x54FB6  04f9efc2     VST1.64         {D12-D15}, [R4@128]
0x54FBA  bcb0         SUB             SP, SP, #0xF0
0x54FBC  0446         MOV             R4, R0
0x54FBE  dff84409     LDR.W           R0, =(unk_1CFE0C - 0x54FD8)
0x54FC2  1294         STR             R4, [SP,#0x130+var_E8]
0x54FC4  47f26601c0f21a01 MOV             R1, #(___gxx_personality_sj0_ptr - 0x54FD2)
0x54FCC  0025         MOVS            R5, #0
```

**Around 0x5562E (an address cited by a recovered rule)**

```
0x55622  40ff990d     VMUL.F32        D16, D16, D9
0x55626  bbff2007     VCVT.S32.F32    D0, D16
0x5562A  10ee101a     VMOV            R1, S0
0x5562E  90ed010a     VLDR            S0, [R0,#4]
0x55632  1598         LDR             R0, [SP,#0x130+var_DC]
0x55634  40ff1e0d     VMUL.F32        D16, D0, D14
0x55638  4fef202d     VADD.F32        D18, D15, D16
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
void __fastcall sub_54FA0(int a1)
{
  float32x2_t v1; // d8
  float32x2_t v2; // d9
  __int64 v3; // d10
  float32x2_t v4; // d11
  float32x2_t v5; // d12
  __int64 v6; // d13
  __int64 v7; // d14
  float32x2_t v8; // d15
  unsigned int *v9; // r2
  int v10; // r1
  int32x2_t v11; // d16
  unsigned __int32 v12; // s0
  int32x2_t v13; // d18
  int v14; // r0
  float v15; // s26
  bool v16; // zf
  int v17; // r0
  unsigned int v18; // r3
```

## References found in the code

Strings referenced: `itemClassiPad` (`0x1D4E32`), `zhuangshi1Frame` (`0x1D58F5`), `zhuangshi2Frame` (`0x1D5905`), `zhuangshi3Frame` (`0x1D5915`), `houseClassiPad` (`0x1D6EEE`), `house1` (`0x1D6EFD`), `house2` (`0x1D6F04`), `house3` (`0x1D6F0B`)

Globals referenced: `flt_214FD4`, `flt_553B0`, `flt_553B4`, `flt_553B8`, `flt_553BC`, `flt_553C0`, `flt_553C4`, `flt_553C8`

Callers (with call sites): `0x53330`@`0x54240`

Callees: `0x7677C`, `0x76980`, `0x172C0C`, `0x172C90`, `0x172CC0`, `0x172D18`, `0x172D1C`, `0x1745A8`, `0x17C720`, `0x1B46F8`, `0x1B4798`, `0x1B7478`, `0x1B7508`, `0x1B7648`, `0x1B76A8`, `0x1B7778`

## Unity translation

- Code comments citing this address (6): `Runtime/CityGridController.cs:381`, `Runtime/CityGridController.cs:858`, `Runtime/CityGridController.cs:1164`, `Runtime/CityGridController.cs:1182`, `Runtime/CityGridController.cs:1201`, `Runtime/CityGridController.cs:3291`

## Validation

- No test is individually linked to this function.

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
