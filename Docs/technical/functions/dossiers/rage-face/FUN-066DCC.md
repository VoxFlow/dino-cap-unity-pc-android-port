# sub_66DCC: citygrind.rage_face.native_fireup_timer_and_erase_events

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-066DCC` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x66DCC` |
| RVA (EA - 0x4000) | `0x62DCC` |
| File offset | `0x62DCC` |
| Size | 1660 bytes, 582 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_66DCC` (IDA automatic name) |
| Bytes SHA-256 | `e7751460fb44de512256098f544f5822e807212e49b880f8bd873516f80e5eb0` |
| Subsystem | rage-face |
| Direct callers | 1 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `citygrind.rage_face.native_fireup_timer_and_erase_events`: PROVEN_NATIVE_ARM / IMPLEMENTED (City Grind) / AUTOMATED_FOCUSED_PASS / DEVICE_AB_PENDING |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `citygrind.rage_face.native_fireup_timer_and_erase_events` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): Rage face = player string at +108 ('_face1/2/3') set by sub_66DCC on every C/D (families 2/3) discharge from the counter at +132 (counter after increment 10..29 -> _face1, 30..49 -> _face2, >=50 -> _face3, plus a once-only 'dinocap.a8' award at 50). The counte

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0x66DCC  f0b5         PUSH            {R4-R7,LR}
0x66DCE  03af         ADD             R7, SP, #0xC
0x66DD0  2de9000d     PUSH.W          {R8,R10,R11}
0x66DD4  adf14004     SUB.W           R4, SP, #0x40 ; '@'
0x66DD8  24f00f04     BIC.W           R4, R4, #0xF
0x66DDC  a546         MOV             SP, R4
0x66DDE  04f9ed82     VST1.64         {D8-D11}, [R4@128]!
0x66DE2  04f9efc2     VST1.64         {D12-D15}, [R4@128]
0x66DE6  a4b0         SUB             SP, SP, #0x90; int
0x66DE8  0790         STR             R0, [SP,#0xD0+var_B4]; int
0x66DEA  45f23222c0f21902 MOV             R2, #(__ZNSs4_Rep20_S_empty_rep_storageE_ptr - 0x66DFA)
0x66DF2  45f23421     MOVW            R1, #:lower16:(___gxx_personality_sj0_ptr - 0x66E04)
0x66DF6  7a44         ADD             R2, PC; __ZNSs4_Rep20_S_empty_rep_storageE_ptr
0x66DF8  c0f21901     MOVT            R1, #:upper16:(___gxx_personality_sj0_ptr - 0x66E04)
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
void __fastcall sub_66DCC(int a1, int a2, int a3, int a4)
{
  float32x2_t v4; // d0
  float32x2_t v5; // d1
  float32x2_t v6; // d2
  __int64 v7; // d8
  __int64 v8; // d9
  __int64 v9; // d10
  __int64 v10; // d11
  __int64 v11; // d12
  __int64 v12; // d13
  __int64 v13; // d14
  __int64 v14; // d15
  unsigned int v15; // r0
  int v16; // r1
  int v17; // r1
  unsigned int *v18; // r1
  int v19; // r2
  float32x2_t v20; // d16
  float32x2_t v21; // d0
```

## References found in the code

Strings referenced: `danke1Frame` (`0x1D5F78`), `danke2Frame` (`0x1D5F84`), `Attack_E1Audio` (`0x1D6AC6`), `Attack_DAudio` (`0x1D6AE9`), `Attack_CAudio` (`0x1D6B0A`), `Attack_BAudio` (`0x1D6B2B`), `Attack_AAudio` (`0x1D6B4C`), `_face1` (`0x1D7314`)

Globals referenced: `byte_21B400`, `dword_21BE00`, `dword_21BE0C`, `flt_6744C`, `flt_67450`

Callers (with call sites): `0x66CE8`@`0x66D7C`

Callees: `0x668FC`, `0x686F4`, `0x68998`, `0x77498`, `0x77708`, `0x7EB04`, `0x7F934`, `0x172CC0`, `0x172D10`, `0x1B7478`, `0x1B74B8`, `0x1B7508`, `0x1B7778`, `0x1B7788`, `0x1B7798`

## Unity translation

- Code comments citing this address (13): `Runtime/CityGridController.cs:4390`, `Runtime/CityGridController.cs:4444`, `Runtime/CityGridController.cs:5280`, `Runtime/CityGridController.cs:5775`, `Runtime/CombatSandboxV2.cs:1168`, `Runtime/CombatSandboxV2.cs:1184`, `Runtime/CombatSandboxV2.cs:2727`, `Runtime/CombatSandboxV2.cs:2988` ...

## Validation

- No test is individually linked to this function.

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
