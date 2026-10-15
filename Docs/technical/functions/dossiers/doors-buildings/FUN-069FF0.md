# sub_69FF0: citygrind.house.door_icon_touch_click_on_release_no_own_proximity

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-069FF0` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x69FF0` |
| RVA (EA - 0x4000) | `0x65FF0` |
| File offset | `0x65FF0` |
| Size | 314 bytes, 104 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_69FF0` (IDA automatic name) |
| Bytes SHA-256 | `d32adb3837d1af60ef7c2546e94d3d87be24b6ecd1ca8841f503e5b6fc49e94c` |
| Subsystem | doors-buildings |
| Direct callers | 0 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `citygrind.house.door_icon_touch_click_on_release_no_own_proximity`: PROVEN_NATIVE_ARM / IMPLEMENTED / AUTOMATED_FOCUSED_PASS / DEVICE_AB_PENDING |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `citygrind.house.door_icon_touch_click_on_release_no_own_proximity` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): The pulsing door icon (houseCtrl) has NO proximity logic of its own. Its touch handler sub_69FF0 tests only the control rect (obj+64..79): phase 0 (begin) inside -> arms byte +85; phase 1 (move) outside -> disarms; phase 2 (end) armed and inside -> disarms and

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0x69FF0  80b5         PUSH            {R7,LR}
0x69FF2  6f46         MOV             R7, SP
0x69FF4  81b0         SUB             SP, SP, #4
0x69FF6  97ed020a     VLDR            S0, [R7,#arg_0]
0x69FFA  43ec303b     VMOV            D16, R3, R3
0x69FFE  bbff2016     VCVT.F32.S32    D1, D16
0x6A002  0146         MOV             R1, R0
0x6A004  bbff0006     VCVT.F32.S32    D0, D0
0x6A008  022a         CMP             R2, #2
0x6A00A  2cd0         BEQ             loc_6A066
0x6A00C  012a         CMP             R2, #1
0x6A00E  60d1         BNE             loc_6A0D2
0x6A010  91f85520     LDRB.W          R2, [R1,#0x55]
0x6A014  0020         MOVS            R0, #0
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
int __fastcall sub_69FF0(int a1, int a2, int a3, int a4, float a5)
{
  int32x2_t v5; // d0
  float32x2_t v6; // d2
  float32x2_t v7; // d3
  float32x2_t v8; // d4
  int32x2_t v9; // d16
  float32x2_t v10; // d1
  int result; // r0

  *(float *)v5.i32 = a5;
  v9.i32[0] = a4;
  v9.i32[1] = a4;
  v10 = vcvt_f32_s32(v9);
  v5.i32[0] = vcvt_f32_s32(v5).u32[0];
  if ( a3 == 2 )
  {
    result = 0;
    if ( *(_BYTE *)(a1 + 85) )
    {
```

## References found in the code

Strings referenced: none

Globals referenced: none among the named globals

Callers (with call sites): none by direct reference

Callees: none

## Unity translation

- Code comments citing this address (3): `Runtime/CityGridController.cs:2691`, `Runtime/CityGridController.cs:6372`, `Runtime/CityGridController.cs:7495`

## Validation

- No test is individually linked to this function.

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
