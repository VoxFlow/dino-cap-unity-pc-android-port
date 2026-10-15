# sub_6A4A8: citygrind.building.player_entry_exit_no_root_transform_motion

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-06A4A8` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x6A4A8` |
| RVA (EA - 0x4000) | `0x664A8` |
| File offset | `0x664A8` |
| Size | 9780 bytes, 3400 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_6A4A8` (IDA automatic name) |
| Bytes SHA-256 | `028d48f8611e3cb9a18d630067db9859dbdf6d237c3e179b778469990eeb1377` |
| Subsystem | saves-shop-progression |
| Direct callers | 0 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `citygrind.building.player_entry_exit_no_root_transform_motion`: PROVEN; `citygrind.shop_screen.entry_trigger_and_layout`: PROVEN (fade mechanism/duration, relative layout) / HARD_BLOCKED (literal entry-trigger ca; `shop.price_text.real_font_not_numeral_sprite`: PROVEN; `weapon_swap.audio.hard_asset_absence`: PROVEN |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `citygrind.building.player_entry_exit_no_root_transform_motion` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): Native's own entry/exit freeze handler (sub_68CD8) and its sole caller (sub_CF14) never write the player's own position/velocity during building entry or exit - the player freezes in place (fixed X AND Y) while the door animates. No jump-named clip exists anyw

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0x6A4A8  f0b5         PUSH            {R4-R7,LR}
0x6A4AA  03af         ADD             R7, SP, #0xC
0x6A4AC  2de9000d     PUSH.W          {R8,R10,R11}
0x6A4B0  adf14004     SUB.W           R4, SP, #0x40 ; '@'
0x6A4B4  24f00f04     BIC.W           R4, R4, #0xF
0x6A4B8  a546         MOV             SP, R4
0x6A4BA  04f9ed82     VST1.64         {D8-D11}, [R4@128]!
0x6A4BE  04f9efc2     VST1.64         {D12-D15}, [R4@128]
0x6A4C2  adf5387d     SUB.W           SP, SP, #0x2E0
0x6A4C6  0a90         STR             R0, [SP,#0x320+var_2F8]; int
0x6A4C8  4cf69560c0f21600 MOV             R0, #(aIapimage - 0x6A4DC); "iapImage"
0x6A4D0  4cf69c61c0f21601 MOV             R1, #(aImageIapIapPng - 0x6A4DE); "Image/iap/iap.png"
0x6A4D8  7844         ADD             R0, PC; "iapImage"
0x6A4DA  7944         ADD             R1, PC; "Image/iap/iap.png"
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
void __fastcall sub_6A4A8(int *a1)
{
  float32x2_t v1; // d0
  float32x2_t v2; // d1
  float32x2_t v3; // d8
  float32x2_t v4; // d9
  float32x2_t v5; // d10
  __int64 v6; // d11
  __int64 v7; // d12
  __int64 v8; // d13
  __int64 v9; // d14
  __int64 v10; // d15
  int v11; // r0
  int v12; // r0
  int v13; // r0
  int v14; // r0
  int v15; // r0
  int v16; // r0
  int v17; // r0
  int v18; // r0
```

## References found in the code

Strings referenced: `iap_3wFrame` (`0x1D4905`), `iap_3w_dFrame` (`0x1D4911`), `iap_8wFrame` (`0x1D491F`), `iap_8w_dFrame` (`0x1D492B`), `icon_goldFrame` (`0x1D4AF7`), `effectClassiPad` (`0x1D4C48`), `bg_bloodFrame` (`0x1D4CD8`), `bar_bloodFrame` (`0x1D4CE6`)

Globals referenced: `byte_21B400`, `byte_21BE6C`, `byte_21BE70`, `byte_21BE74`, `byte_21BE78`, `dword_214FC4`, `dword_21A0C4`, `dword_21A0D4`, `dword_21A0E4`, `dword_21A0F4`, `dword_21A104`, `dword_21A114`

Callers (with call sites): none by direct reference

Callees: `0x54D94`, `0x54DEC`, `0x661D8`, `0x6CADC`, `0x7EC50`, `0x7F008`, `0x171AA0`, `0x172BC8`, `0x172C0C`, `0x172C90`, `0x172D18`, `0x172D1C`, `0x172D84`, `0x172DB8`, `0x1745A8`, `0x17C720`

## Unity translation

- Code comments citing this address (9): `Runtime/CityGridController.cs:3513`, `Runtime/CityGridController.cs:3526`, `Runtime/CityGridController.cs:3552`, `Runtime/CityGridController.cs:3570`, `Runtime/CityGridController.cs:3588`, `Runtime/CityGridController.cs:3717`, `Runtime/CityGridController.cs:3889`, `Runtime/CityGridController.cs:8029` ...

## Validation

- No test is individually linked to this function.

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
