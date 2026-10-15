# sub_FE50: Dinosaur attack predicate

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-00FE50` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0xFE50` |
| RVA (EA - 0x4000) | `0xBE50` |
| File offset | `0xBE50` |
| Size | 158 bytes, 49 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_FE50` (IDA automatic name) |
| Bytes SHA-256 | `62bdc7354920b522c0ce19114c83d18f22787d90a99dc9cad71dd71ca57a7ddd` |
| Subsystem | dinosaurs-spawning |
| Direct callers | 4 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) |  |
| Confidence of this dossier | PROVEN (Task024B report); excerpt shows the coordinate reads |
| Depth | deep dossier (hand-written notes) |

## What it does

Reads the player's and the dinosaur's root X coordinates (sub_172C78), subtracts them, and tests a facing-gated strict range against the dinosaur's attack distance.

## Native instructions (annotated excerpts)

Address, raw bytes, disassembly in IDA 9.2's rendering. The bytes are exact; the notes are interpretation.

**Two root-X reads and the subtraction (VSUB.F32).**

```
0xFE8E  d4f8a800     LDR.W           R0, [R4,#0xA8]
0xFE92  62f1f1fe     BL              sub_172C78
0xFE96  90ed008a     VLDR            S16, [R0]
0xFE9A  6068         LDR             R0, [R4,#4]
0xFE9C  62f1ecfe     BL              sub_172C78
0xFEA0  90ed000a     VLDR            S0, [R0]
0xFEA4  94f86000     LDRB.W          R0, [R4,#0x60]
0xFEA8  28ef000d     VSUB.F32        D0, D8, D0
0xFEAC  94ed251a     VLDR            S2, [R4,#0x94]
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
bool __fastcall sub_FE50(int a1)
{
  float32x2_t v1; // d0
  float32x2_t v2; // d1
  float32x2_t v3; // d8
  const void *v5; // r0
  _BOOL4 result; // r0

  v5 = (const void *)sub_172D18(*(_DWORD *)(a1 + 168));
  if ( (*((_DWORD *)__dynamic_cast(
                      v5,
                      (const struct __class_type_info *)&`typeinfo for'Triniti2D::GameObjectCallBack,
                      (const struct __class_type_info *)&`typeinfo for'DinoCap::playerClassiPad,
                      -1)
        + 45)
      | 2) == 6 )
    return 0;
  v3.i32[0] = *(_DWORD *)sub_172C78(*(_DWORD *)(a1 + 168));
  v1.i32[0] = *(_DWORD *)sub_172C78(*(_DWORD *)(a1 + 4));
  v1.i32[0] = vsub_f32(v3, v1).u32[0];
```

## References found in the code

Strings referenced: none

Globals referenced: none among the named globals

Callers (with call sites): `0xFD58`@`0xFDC2`, `0x101A0`@`0x101C0`, `0x10220`@`0x10228`, `0x10294`@`0x1032E`

Callees: `0x172C78`, `0x172D18`, `0x1B76A8`

## Unity translation

- attack cone: `Assets/DinoCap/PortV2/Runtime/CityGridController.cs` line 6640
- Code comments citing this address (3): `Runtime/CityGridController.cs:6597`, `Runtime/CityGridController.cs:6607`, `Runtime/CityGridController.cs:6609`

## Validation

- Tests/Task027N236CityGridRepro.cs
- Tests/Task027N22BiteBoundaryPredictive.cs

## Open / limits

The strict inequality and runtime attack distance (+0..19 jitter) come from the Task024B report, not from this excerpt.
