# sub_68A8C: combat.f2_burn.target_cardinality

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-068A8C` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x68A8C` |
| RVA (EA - 0x4000) | `0x64A8C` |
| File offset | `0x64A8C` |
| Size | 580 bytes, 201 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_68A8C` (IDA automatic name) |
| Bytes SHA-256 | `1f74a0332a52811d07fffb5f114d569bab89e9ce63670f1d981a2c131f8028e1` |
| Subsystem | input-combat |
| Direct callers | 2 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `combat.f2_burn.target_cardinality`: PROVEN; `combat.f3_audio.retrigger_mechanism_and_cadence_gap`: PROVEN_NATIVE / IMPLEMENTED / AUTOMATED_FOCUSED_PASS / DEVICE_AB_PENDING; `combat.f3_electrocution.city_grid_implementation`: PROVEN; `combat.f3_light_touch.emergent_mechanism`: PROVEN |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `combat.f2_burn.target_cardinality` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): Native F2 (flamethrower) burn mechanism is MULTI_TARGET, not single-target. sub_68A8C (the per-tick F2 cone-sweep dispatcher, called every tick while F2 is held) loops over the ENTIRE scene's dino container (std::vector-style), independently range-checking, ar

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0x68A8C  f0b5         PUSH            {R4-R7,LR}
0x68A8E  03af         ADD             R7, SP, #0xC
0x68A90  2de9000d     PUSH.W          {R8,R10,R11}
0x68A94  adf14004     SUB.W           R4, SP, #0x40 ; '@'
0x68A98  24f00f04     BIC.W           R4, R4, #0xF
0x68A9C  a546         MOV             SP, R4
0x68A9E  04f9ed82     VST1.64         {D8-D11}, [R4@128]!
0x68AA2  04f9efc2     VST1.64         {D12-D15}, [R4@128]
0x68AA6  a0b0         SUB             SP, SP, #0x80
0x68AA8  0646         MOV             R6, R0
0x68AAA  7068         LDR             R0, [R6,#4]
0x68AAC  0496         STR             R6, [SP,#0xC0+var_B0]
0x68AAE  0af1e3f8     BL              sub_172C78
0x68AB2  43f27851     MOVW            R1, #:lower16:(off_1FC03C - 0x68AC4)
```

**Around 0x68A8C (an address cited by a recovered rule)**

```
0x68A8C  f0b5         PUSH            {R4-R7,LR}
0x68A8E  03af         ADD             R7, SP, #0xC
0x68A90  2de9000d     PUSH.W          {R8,R10,R11}
0x68A94  adf14004     SUB.W           R4, SP, #0x40 ; '@'
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
void __fastcall sub_68A8C(
        int a1,
        int a2,
        int a3,
        int a4,
        int a5,
        int a6,
        int a7,
        struct _Unwind_Exception *lpuexcpt,
        int a9,
        int a10,
        char a11,
        int a12,
        int a13,
        int a14,
        int a15,
        int a16,
        int a17,
        int a18,
        int a19,
```

## References found in the code

Strings referenced: none

Globals referenced: `dword_21BE00`, `dword_21BE0C`

Callers (with call sites): `0x679F4`@`0x67C30`, `0x67CE8`@`0x67FC6`

Callees: `0xCA84`, `0x12884`, `0x12C64`, `0x77498`, `0xE885C`, `0x172C78`, `0x172D14`, `0x172D18`, `0x17C71C`, `0x1B7528`, `0x1B76A8`, `0x1B7778`, `0x1B7788`, `0x1B7798`

## Unity translation

- Code comments citing this address (11): `Runtime/CityGridController.cs:648`, `Runtime/CityGridController.cs:659`, `Runtime/CityGridController.cs:662`, `Runtime/CityGridController.cs:4336`, `Runtime/CityGridController.cs:5762`, `Runtime/CityGridController.cs:5936`, `Runtime/CityGridController.cs:6226`, `Runtime/CombatSandboxV2.cs:411` ...

## Validation

- Tests/Task027N236CityGridRepro.cs
- Tests/DC1EightDefectRepro.cs

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
