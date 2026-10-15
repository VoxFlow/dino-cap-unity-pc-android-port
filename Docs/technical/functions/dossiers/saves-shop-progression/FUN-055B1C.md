# sub_55B1C: citygrind.shop_screen.entry_trigger_and_layout

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-055B1C` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x55B1C` |
| RVA (EA - 0x4000) | `0x51B1C` |
| File offset | `0x51B1C` |
| Size | 1084 bytes, 355 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_55B1C` (IDA automatic name) |
| Bytes SHA-256 | `465dd84991b21edb1bd4809b98c39d7edae78810f51e3e0e2c01d69e100aaa74` |
| Subsystem | saves-shop-progression |
| Direct callers | 0 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `citygrind.shop_screen.entry_trigger_and_layout`: PROVEN (fade mechanism/duration, relative layout) / HARD_BLOCKED (literal entry-trigger ca; `upgrade_shop_scene.owner_and_hp_refill`: PROVEN |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `citygrind.shop_screen.entry_trigger_and_layout` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): Shop entry's own scene-state=5 write is reachable via 2 independent candidates - sub_56118's own case18 (unconditional, no control-identity check) and sub_7334C (an identical single-instruction state write, found via the shop-exit-trigger family) - BOTH proven

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0x55B1C  f0b5         PUSH            {R4-R7,LR}
0x55B1E  03af         ADD             R7, SP, #0xC
0x55B20  2de9000d     PUSH.W          {R8,R10,R11}
0x55B24  adf14004     SUB.W           R4, SP, #0x40 ; '@'
0x55B28  24f00f04     BIC.W           R4, R4, #0xF
0x55B2C  a546         MOV             SP, R4
0x55B2E  04f9ed82     VST1.64         {D8-D11}, [R4@128]!
0x55B32  04f9efc2     VST1.64         {D12-D15}, [R4@128]
0x55B36  a0b0         SUB             SP, SP, #0x80
0x55B38  0546         MOV             R5, R0
0x55B3A  0091         STR             R1, [SP,#0xC0+var_C0]
0x55B3C  6868         LDR             R0, [R5,#4]
0x55B3E  26f121ff     BL              sub_17C984
0x55B42  46f2e441     MOVW            R1, #:lower16:(___gxx_personality_sj0_ptr - 0x55B54)
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
void __fastcall sub_55B1C(_DWORD *a1, __int32 a2)
{
  float32x2_t v2; // d0
  __int64 v3; // d8
  __int64 v4; // d9
  __int64 v5; // d10
  __int64 v6; // d11
  __int64 v7; // d12
  __int64 v8; // d13
  __int64 v9; // d14
  __int64 v10; // d15
  int v12; // r4
  int v13; // r6
  float32x2_t v14; // d16
  float32x2_t v15; // d16
  float32x2_t v16; // d0
  int v17; // r0
  int v18; // r0
  float32x2_t v19; // d16
  float32x2_t v20; // d0
```

## References found in the code

Strings referenced: `whiteFrame` (`0x1D4939`), `AdSceneiPad` (`0x1D4E40`), `upgradeSceneiPad` (`0x1D4E4C`)

Globals referenced: `dword_21C6F8`, `flt_55F5C`, `flt_55F60`, `flt_55F64`

Callers (with call sites): none by direct reference

Callees: `0x55F6C`, `0x56030`, `0x1706D4`, `0x17C968`, `0x17C970`, `0x17C984`, `0x183D54`, `0x183D94`, `0x185F70`, `0x194E6C`, `0x1B73B8`, `0x1B7478`, `0x1B7508`, `0x1B7778`, `0x1B7788`, `0x1B7798`

## Unity translation

- Code comments citing this address (6): `Runtime/CityGridController.cs:3274`, `Runtime/CityGridController.cs:3290`, `Runtime/CityGridController.cs:3386`, `Runtime/CityGridController.cs:3397`, `Runtime/CityGridController.cs:7757`, `Runtime/CityGridController.cs:8021`

## Validation

- No test is individually linked to this function.

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
