# sub_10ECC: citygrind.pause.moving_particles_freeze

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-010ECC` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x10ECC` |
| RVA (EA - 0x4000) | `0xCECC` |
| File offset | `0xCECC` |
| Size | 582 bytes, 175 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_10ECC` (IDA automatic name) |
| Bytes SHA-256 | `41ef1d189d2e144e3cb0810e36ff7492b6fbb0811b77cbc5db84be96c142ecae` |
| Subsystem | pause-time |
| Direct callers | 1 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `citygrind.pause.moving_particles_freeze`: PROVEN_NATIVE_MECHANISM / IMPLEMENTED (Unity vessel = pause gate) / AUTOMATED_FOCUSED_PASS |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `citygrind.pause.moving_particles_freeze` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): Native pause is a scene TIME SCALE of 0, not per-object freezing. Triniti2D scene update sub_17BA5C computes scaledDt = dt * scene[+0xB74] (a1[733]) and feeds scaledDt to: the timer queue (sub_182058 -> PriorityQueue<TimerInfo>::Update), every enabled GameObje

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0x10ECC  f0b5         PUSH            {R4-R7,LR}
0x10ECE  03af         ADD             R7, SP, #0xC
0x10ED0  2de90005     PUSH.W          {R8,R10}
0x10ED4  2ded02cb     VPUSH           {D12}
0x10ED8  2ded02eb     VPUSH           {D14}; int
0x10EDC  adf11804     SUB.W           R4, SP, #0x18
0x10EE0  24f00f04     BIC.W           R4, R4, #0xF
0x10EE4  a546         MOV             SP, R4; int
0x10EE6  04f9ef8a     VST1.64         {D8-D9}, [R4@128]
0x10EEA  84ed04ab     VSTR            D10, [R4,#0x10]
0x10EEE  88b0         SUB             SP, SP, #0x20; int
0x10EF0  8846         MOV             R8, R1
0x10EF2  43f69641c0f21c01 MOV             R1, #(aTou - 0x10F00); "tou"
0x10EFA  0446         MOV             R4, R0
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
int __fastcall sub_10ECC(int a1, int a2)
{
  float32x2_t v2; // d0
  float32x2_t v3; // d2
  float32x2_t v4; // d3
  __int64 v5; // d8
  __int64 v6; // d9
  float32x2_t v7; // d10
  int result; // r0
  __int32 *v11; // r0
  float32x2_t v12; // d12
  float32x2_t v13; // d8
  int32x2_t v14; // d18
  float32x2_t v15; // d16
  float32x2_t v16; // d10
  float32x2_t v17; // d16
  void *v18; // r5
  float32x2_t v19; // d0
  float32x2_t v20; // d1
  bool v21; // r6
```

## References found in the code

Strings referenced: `qxEffectAnimation` (`0x1D4C11`), `dxEffectAnimation` (`0x1D4C23`)

Globals referenced: `flt_11114`, `flt_11118`, `flt_1111C`, `flt_11120`

Callers (with call sites): `0x10554`@`0x10980`

Callees: `0x584D4`, `0x7677C`, `0x76980`, `0x77498`, `0x77708`, `0x172BC8`, `0x172BD0`, `0x172C78`, `0x172CC0`, `0x172D14`, `0x172D18`, `0x17C71C`, `0x17C840`, `0x1B76A8`, `0x1B7F18`

## Unity translation

- Code comments citing this address (17): `Runtime/Combat/DinoCapBloodBurst.cs:6`, `Runtime/Combat/DinoCapBloodBurst.cs:15`, `Runtime/Combat/DinoCapBloodBurst.cs:90`, `Runtime/Combat/DinoCapBloodBurst.cs:99`, `Runtime/CombatSandboxV2.cs:2483`, `Runtime/CombatSandboxV2.cs:2799`, `Runtime/CombatSandboxV2.cs:2807`, `Runtime/CombatSandboxV2.cs:2824` ...

## Validation

- Tests/DC1BloodBurstPauseRepro.cs
- Tests/DC1PausePointerRepro.cs

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
