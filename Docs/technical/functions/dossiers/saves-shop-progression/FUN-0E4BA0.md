# sub_E4BA0: a2b.weapon_effect_attachment_subsystem

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-0E4BA0` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0xE4BA0` |
| RVA (EA - 0x4000) | `0xE0BA0` |
| File offset | `0xE0BA0` |
| Size | 14866 bytes, 4986 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_E4BA0` (IDA automatic name) |
| Bytes SHA-256 | `247cf92423262ed36562618cb92f1dadd87eb42cedff2be1ea27bc92c44e1842` |
| Subsystem | saves-shop-progression |
| Direct callers | 1 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `a2b.weapon_effect_attachment_subsystem`: PROVEN; `progression.upgrade_shop.city_grid_implementation`: PROVEN; `shop.price_text.real_font_not_numeral_sprite`: PROVEN; `upgrade_shop_scene.owner_and_hp_refill`: PROVEN |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `a2b.weapon_effect_attachment_subsystem` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): sub_7D554 lazily one-time-initializes a pool of ~6 static attachment-template slots (guarded by byte_21C3C4). sub_7D81C is a runtime per-request switch-dispatcher (17 exhaustive callers: weapon/effect functions incl. sub_A1518, sub_E4BA0, sub_8161C, plus the l

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0xE4BA0  f0b5         PUSH            {R4-R7,LR}
0xE4BA2  03af         ADD             R7, SP, #0xC
0xE4BA4  2de9000d     PUSH.W          {R8,R10,R11}
0xE4BA8  adf14004     SUB.W           R4, SP, #0x40 ; '@'
0xE4BAC  24f00f04     BIC.W           R4, R4, #0xF
0xE4BB0  a546         MOV             SP, R4
0xE4BB2  04f9ed82     VST1.64         {D8-D11}, [R4@128]!
0xE4BB6  04f9efc2     VST1.64         {D12-D15}, [R4@128]
0xE4BBA  adf5906d     SUB.W           SP, SP, #0x480
0xE4BBE  0546         MOV             R5, R0
0xE4BC0  47f28a40c0f21100 MOV             R0, #(___stack_chk_guard_ptr - 0xE4BCE)
0xE4BC8  0e46         MOV             R6, R1
0xE4BCA  7844         ADD             R0, PC; ___stack_chk_guard_ptr
0xE4BCC  0068         LDR             R0, [R0]; ___stack_chk_guard
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
int __fastcall sub_E4BA0(_DWORD *a1, int a2)
{
  float32x2_t v2; // d0
  float32x2_t v3; // d1
  __int64 v4; // d8
  __int64 v5; // d9
  __int64 v6; // d10
  __int64 v7; // d11
  __int64 v8; // d12
  __int64 v9; // d13
  __int64 v10; // d14
  __int64 v11; // d15
  float32x2_t v14; // d16
  int v16; // r0
  int v17; // r8
  int v18; // r4
  _QWORD *v19; // r5
  int v20; // r3
  __int64 v21; // d17
  __int64 v22; // d18
```

## References found in the code

Strings referenced: `Audio_Button1Audio` (`0x1D4EF3`), `Audio_button2Audio` (`0x1D6997`), `icon_A%dFrame` (`0x1D7391`), `icon_B%dFrame` (`0x1D739F`), `icon_C%dFrame` (`0x1D73AD`), `icon_D%dFrame` (`0x1D73BB`), `icon_E%dFrame` (`0x1D73C9`), `icon_F%dFrame` (`0x1D73D7`)

Globals referenced: `dword_21BDF8`, `dword_21BE00`, `dword_21BE0C`, `dword_21BE18`, `flt_21BDF0`, `flt_E4DEC`

Callers (with call sites): `0xE49E8`@`0xE4AF4`

Callees: `0xCA84`, `0x7D81C`, `0x7EB04`, `0x7F934`, `0xC4668`, `0xD2694`, `0xE85B4`, `0xE885C`, `0x172BC8`, `0x172D18`, `0x172D1C`, `0x172DB8`, `0x1745A8`, `0x18BB60`, `0x18BBC4`, `0x1B7478`

## Unity translation

- No Unity code comment cites this address: **unmapped** (not proof the behaviour is absent from the port).

## Validation

- No test is individually linked to this function.

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
