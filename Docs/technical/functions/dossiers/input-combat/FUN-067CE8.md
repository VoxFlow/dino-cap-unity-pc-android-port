# sub_67CE8: combat.f2_burn.target_cardinality

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-067CE8` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x67CE8` |
| RVA (EA - 0x4000) | `0x63CE8` |
| File offset | `0x63CE8` |
| Size | 906 bytes, 303 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_67CE8` (IDA automatic name) |
| Bytes SHA-256 | `f52f53af60ed2048b395a020d6d6e41bc0d3b5a6723b507e78f56545e0b5ff63` |
| Subsystem | input-combat |
| Direct callers | 1 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `combat.f2_burn.target_cardinality`: PROVEN; `combat.f3_audio.retrigger_mechanism_and_cadence_gap`: PROVEN_NATIVE / IMPLEMENTED / AUTOMATED_FOCUSED_PASS / DEVICE_AB_PENDING; `combat.f3_electrocution.city_grid_implementation`: PROVEN; `combat.f3_laser.held_loop_4_6_infinite_no_reseed`: PROVEN_NATIVE_ARM / IMPLEMENTED / AUTOMATED_FOCUSED_PASS / DEVICE_AB_PENDING |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `combat.f2_burn.target_cardinality` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): Native F2 (flamethrower) burn mechanism is MULTI_TARGET, not single-target. sub_68A8C (the per-tick F2 cone-sweep dispatcher, called every tick while F2 is held) loops over the ENTIRE scene's dino container (std::vector-style), independently range-checking, ar

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0x67CE8  f0b5         PUSH            {R4-R7,LR}
0x67CEA  03af         ADD             R7, SP, #0xC
0x67CEC  2de9000d     PUSH.W          {R8,R10,R11}
0x67CF0  adf14004     SUB.W           R4, SP, #0x40 ; '@'
0x67CF4  24f00f04     BIC.W           R4, R4, #0xF
0x67CF8  a546         MOV             SP, R4
0x67CFA  04f9ed82     VST1.64         {D8-D11}, [R4@128]!
0x67CFE  04f9efc2     VST1.64         {D12-D15}, [R4@128]
0x67D02  9cb0         SUB             SP, SP, #0x70
0x67D04  0791         STR             R1, [SP,#0xB0+var_94]; int
0x67D06  4cf69774c0f21604 MOV             R4, #(aYyj5 - 0x67D14); "yyj5"
0x67D0E  0646         MOV             R6, R0
0x67D10  7c44         ADD             R4, PC; "yyj5"
0x67D12  2146         MOV             R1, R4
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
void __fastcall sub_67CE8(int a1, int a2)
{
  float32x2_t v2; // d0
  float32x2_t v3; // d1
  float32x2_t v4; // d2
  float32x2_t v5; // d3
  __int64 v6; // d8
  __int64 v7; // d9
  __int64 v8; // d10
  __int64 v9; // d11
  __int64 v10; // d12
  __int64 v11; // d13
  __int64 v12; // d14
  __int64 v13; // d15
  int v14; // r6
  int v15; // r0
  __int32 *v16; // r0
  float32x2_t v17; // d16
  float32x2_t v18; // d2
  float32x2_t v19; // d1
```

## References found in the code

Strings referenced: `effectClassiPad` (`0x1D4C48`), `Attack_F3Audio` (`0x1D6A3A`), `JiguangAnim` (`0x1D734E`)

Globals referenced: `byte_21BE78`, `flt_68074`

Callers (with call sites): `0x66CE8`@`0x66DB6`

Callees: `0x68A8C`, `0x7677C`, `0x76980`, `0x77498`, `0x77708`, `0x7EB04`, `0x171AA0`, `0x172BC8`, `0x172BCC`, `0x172BD0`, `0x172C78`, `0x172CC0`, `0x172D14`, `0x172D18`, `0x172D84`, `0x17C840`

## Unity translation

- Code comments citing this address (9): `Runtime/CityGridController.cs:663`, `Runtime/Combat/F3LaserVisual.cs:6`, `Runtime/Combat/F3LaserVisual.cs:24`, `Runtime/Combat/F3LaserVisual.cs:99`, `Runtime/CombatSandboxV2.cs:428`, `Runtime/CombatSandboxV2.cs:1152`, `Runtime/CombatSandboxV2.cs:2577`, `Runtime/CombatSandboxV2.cs:3726` ...

## Validation

- Tests/Task027N236CityGridRepro.cs
- Tests/DC1EightDefectRepro.cs

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
