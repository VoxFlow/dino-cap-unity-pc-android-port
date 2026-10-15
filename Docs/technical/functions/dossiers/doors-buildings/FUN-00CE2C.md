# sub_CE2C: House door proximity gate

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-00CE2C` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0xCE2C` |
| RVA (EA - 0x4000) | `0x8E2C` |
| File offset | `0x8E2C` |
| Size | 228 bytes, 69 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_CE2C` (IDA automatic name) |
| Bytes SHA-256 | `fea39735f134d09d7f6b5ae2653f7a3b0dccb9c74e1472cf13ca6d8ca3904ca9` |
| Subsystem | doors-buildings |
| Direct callers | 1 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `citygrind.building.player_entry_exit_no_sprite_level_presentation_either`: FALSIFIED; `citygrind.house.door_icon_range_com2_md`: PROVEN_NATIVE_ARM (gate formula) / LIKELY (child world-position equals root + part x) / IM; `citygrind.house.door_icon_touch_click_on_release_no_own_proximity`: PROVEN_NATIVE_ARM / IMPLEMENTED / AUTOMATED_FOCUSED_PASS / DEVICE_AB_PENDING |
| Confidence of this dossier | PROVEN_NATIVE_ARM (formula); LIKELY (child world position equals root + part x) |
| Depth | deep dossier (hand-written notes) |

## What it does

Finds the house composite's child named 'com2' and returns true when abs(player.x - com2.x) < com2.width * 0.7 + 5; refuses in player states 4 and 6.

## Native instructions (annotated excerpts)

Address, raw bytes, disassembly in IDA 9.2's rendering. The bytes are exact; the notes are interpretation.

**Look up the child named "com2" (sub_77708) and read two coordinates.**

```
0xCE42  47f64c31c0f21c01 MOV             R1, #(aCom2 - 0xCE50); "com2"
0xCE4A  0446         MOV             R4, R0
0xCE4C  7944         ADD             R1, PC; "com2"
0xCE4E  6af05bfc     BL              sub_77708
0xCE52  0546         MOV             R5, R0
0xCE54  65f110ff     BL              sub_172C78
0xCE58  90ed008a     VLDR            S16, [R0]
0xCE5C  2846         MOV             R0, R5
0xCE5E  65f12fff     BL              sub_172CC0
0xCE62  90ed009a     VLDR            S18, [R0]
```

**Load the player state at +0xB4, OR with 2, compare with 6; constant 0.7 loaded as a float.**

```
0xCEB8  d0f8b400     LDR.W           R0, [R0,#0xB4]
0xCEBC  9fed140a     VLDR            S0, =0.7
0xCEC0  40f00200     ORR.W           R0, R0, #2
0xCEC4  0628         CMP             R0, #6
0xCEC6  19d0         BEQ             loc_CEFC
0xCEC8  94f82800     LDRB.W          R0, [R4,#0x28]
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
bool __fastcall sub_CE2C(int a1)
{
  float32x2_t v1; // d0
  float32x2_t v2; // d1
  float32x2_t v3; // d8
  float32x2_t v4; // d9
  int v6; // r5
  int v7; // r0
  const void *v8; // r0
  int v9; // r5
  const void *v10; // r0
  float32x2_t v11; // d16

  v6 = sub_77708(a1, "com2");
  v3.i32[0] = *(_DWORD *)sub_172C78(v6);
  v4.i32[0] = *(_DWORD *)sub_172CC0(v6);
  v7 = sub_172D14(*(_DWORD *)(a1 + 4));
  v8 = (const void *)sub_17C71C(v7);
  v9 = *((_DWORD *)__dynamic_cast(
                     v8,
```

## References found in the code

Strings referenced: none

Globals referenced: `flt_CF10`

Callers (with call sites): `0x56030`@`0x560C0`

Callees: `0x77708`, `0x172C78`, `0x172CC0`, `0x172D14`, `0x172D18`, `0x17C71C`, `0x1B76A8`

## Unity translation

- door range: `Assets/DinoCap/PortV2/Runtime/CityGridController.cs` line 2751
- Code comments citing this address (2): `Runtime/CityGridController.cs:2693`, `Runtime/CityGridController.cs:2741`

## Validation

- Tests/Task027N236CityGridRepro.cs (door tests, e.g. N249R_75)

## Open / limits

The final addition of 5.0 is documented in the registry; it is beyond the excerpted window.
