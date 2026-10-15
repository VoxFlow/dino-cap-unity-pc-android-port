# sub_65E2C: camera.follow.one_frame_blip_when_turning_away_from_clamped_edge

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-065E2C` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x65E2C` |
| RVA (EA - 0x4000) | `0x61E2C` |
| File offset | `0x61E2C` |
| Size | 714 bytes, 224 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_65E2C` (IDA automatic name) |
| Bytes SHA-256 | `554a6e3dc52b3e6e7ac5e1186bb121c99ed79ad3e4cae4f9d654e3f2b202865a` |
| Subsystem | animation-rendering |
| Direct callers | 1 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `camera.follow.one_frame_blip_when_turning_away_from_clamped_edge`: PROVEN_NATIVE_QUIRK (original bug kept, mark for writeup) / widescreen adapter kept (blip ; `camera.screen_mapping.ipad`: PROVEN; `citygrind.viewport_aspect_policy.native_contract_and_unity_proof`: PROVEN; `combat.f3_audio.retrigger_mechanism_and_cadence_gap`: PROVEN_NATIVE / IMPLEMENTED / AUTOMATED_FOCUSED_PASS / DEVICE_AB_PENDING; `player.melee_flag.survives_bite_and_restarts_melee_after_hurt`: PROVEN_NATIVE_ARM (every link) / IMPLEMENTED_IN_UNITY / AUTOMATED_FOCUSED_PASS / USER_UNIT |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `camera.follow.one_frame_blip_when_turning_away_from_clamped_edge` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): Player follow-camera sub_65E2C (per frame): facing flag +96 (true = facing left, k=0.55; false = facing right, k=0.45), catch-up flag +125, pan speed +128 (315 u/s at the 1024 reference), world bounds scene+28/+29. On a facing flip it sets catch-up, steps the 

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0x65E2C  f0b5         PUSH            {R4-R7,LR}
0x65E2E  03af         ADD             R7, SP, #0xC
0x65E30  90b0         SUB             SP, SP, #0x40
0x65E32  0446         MOV             R4, R0
0x65E34  0e46         MOV             R6, R1
0x65E36  6068         LDR             R0, [R4,#4]
0x65E38  0cf11eff     BL              sub_172C78
0x65E3C  d0e90001     LDRD.W          R0, R1, [R0]
0x65E40  0f91         STR             R1, [SP,#0x40+var_8+4]
0x65E42  0e90         STR             R0, [SP,#0x40+var_8]
0x65E44  6068         LDR             R0, [R4,#4]
0x65E46  0cf165ff     BL              sub_172D14
0x65E4A  0546         MOV             R5, R0
0x65E4C  16f18afd     BL              sub_17C964
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
int __fastcall sub_65E2C(int a1, __int32 a2)
{
  float32x2_t v2; // d0
  float32x2_t v3; // d1
  float32x2_t v4; // d2
  float32x2_t v5; // d3
  int v8; // r5
  int v9; // r1
  int v10; // r0
  float32x2_t v11; // d16
  float32x2_t v12; // d18
  float32x2_t v13; // d20
  float32x2_t v14; // d0
  float32x2_t v15; // d16
  float32x2_t v16; // d18
  float32x2_t v17; // d20
  const void *v18; // r0
  int v19; // r1
  void *v20; // r6
  int v21; // r0
```

## References found in the code

Strings referenced: none

Globals referenced: `byte_21BE78`, `dword_21BDF4`, `flt_214FCC`, `flt_21BDF0`, `flt_660F8`, `flt_660FC`, `flt_66100`, `flt_66104`, `flt_66108`, `flt_6610C`

Callers (with call sites): `0x65C78`@`0x65D16`

Callees: `0x56E6C`, `0x6622C`, `0x662C0`, `0x66310`, `0x166414`, `0x16669C`, `0x172BD0`, `0x172C78`, `0x172D14`, `0x172D50`, `0x173A98`, `0x173B50`, `0x17C71C`, `0x17C964`, `0x1B76A8`

## Unity translation

- Code comments citing this address (4): `Runtime/CityGridController.cs:496`, `Runtime/CityGridController.cs:502`, `Runtime/CityGridController.cs:1541`, `Runtime/CityGridController.cs:3410`

## Validation

- No test is individually linked to this function.

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
