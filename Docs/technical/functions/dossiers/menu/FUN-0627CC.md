# sub_627CC: mainmenu.background_dino.class_identity

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-0627CC` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x627CC` |
| RVA (EA - 0x4000) | `0x5E7CC` |
| File offset | `0x5E7CC` |
| Size | 700 bytes, 231 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_627CC` (IDA automatic name) |
| Bytes SHA-256 | `f2c0883ffff0c26a3908ef663b53eac5b29b744bbe544712bd0b66c8bd744f05` |
| Subsystem | menu |
| Direct callers | 1 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `mainmenu.background_dino.class_identity`: PROVEN; `mainmenu.background_dino.scale`: PROVEN; `mainmenu.background_dino.spawn_and_roar`: PROVEN; `mainmenu.background_dino.variant_semantics`: PROVEN; `mainmenu.background_dino.velocity`: PROVEN |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `mainmenu.background_dino.class_identity` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): Menu uses a generic GameObject vessel with a specifically registered derived callback: menuObjiPad->599A0->vtable1FDB38, tick slot+24=59A54. Tick calls base76A20 animator then deactivates at strict worldX>768. Old generic-callback/no-native-cleanup assumption 

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0x627CC  f0b5         PUSH            {R4-R7,LR}
0x627CE  03af         ADD             R7, SP, #0xC
0x627D0  2de9000d     PUSH.W          {R8,R10,R11}
0x627D4  adf14004     SUB.W           R4, SP, #0x40 ; '@'
0x627D8  24f00f04     BIC.W           R4, R4, #0xF
0x627DC  a546         MOV             SP, R4
0x627DE  04f9ed82     VST1.64         {D8-D11}, [R4@128]!
0x627E2  04f9efc2     VST1.64         {D12-D15}, [R4@128]
0x627E6  a0b0         SUB             SP, SP, #0x80
0x627E8  49f64001     MOVW            R1, #:lower16:(___gxx_personality_sj0_ptr - 0x627F8)
0x627EC  8046         MOV             R8, R0
0x627EE  c0f21901     MOVT            R1, #:upper16:(___gxx_personality_sj0_ptr - 0x627F8)
0x627F2  a548         LDR             R0, =(unk_1D013C - 0x627FE)
0x627F4  7944         ADD             R1, PC; ___gxx_personality_sj0_ptr
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
void __fastcall sub_627CC(int a1)
{
  float32x2_t v1; // d0
  __int64 v2; // d8
  __int64 v3; // d9
  __int64 v4; // d10
  __int64 v5; // d11
  __int64 v6; // d12
  __int64 v7; // d13
  __int64 v8; // d14
  __int64 v9; // d15
  int v10; // r8
  int v11; // r4
  int v12; // r0
  const void *v13; // r0
  _DWORD *v14; // r4
  int v15; // r3
  int32x2_t v16; // d18
  _UNKNOWN **v17; // r0
  int v18; // r0
```

## References found in the code

Strings referenced: `Audio_Dino2Audio` (`0x1D57CD`), `menuObjiPad` (`0x1D7208`)

Globals referenced: `dword_21B1B0`, `dword_21B1E0`, `flt_62A8C`, `flt_62A90`

Callers (with call sites): `0x61AD4`@`0x61C08`

Callees: `0x7677C`, `0x76980`, `0x7EB04`, `0x16669C`, `0x172BC8`, `0x172BCC`, `0x172C0C`, `0x172D18`, `0x172D50`, `0x173A98`, `0x173B50`, `0x17C720`, `0x17C964`, `0x1B7478`, `0x1B7508`, `0x1B76A8`

## Unity translation

- Code comments citing this address (2): `Runtime/MainMenuController.cs:263`, `Runtime/MainMenuController.cs:304`

## Validation

- No test is individually linked to this function.

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
