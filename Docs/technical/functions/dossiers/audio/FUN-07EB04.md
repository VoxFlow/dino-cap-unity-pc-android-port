# sub_7EB04: Play a sound if effects are on

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-07EB04` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x7EB04` |
| RVA (EA - 0x4000) | `0x7AB04` |
| File offset | `0x7AB04` |
| Size | 322 bytes, 111 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_7EB04` (IDA automatic name) |
| Bytes SHA-256 | `6a36a854f2306c555ef1342d72b58ee996f3826fc8f95a88c84de3a354bfe5a0` |
| Subsystem | audio |
| Direct callers | 71 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `audio.dino_death_sound_shares_the_28_voice_cap`: PROVEN_NATIVE_ARM / SOURCE_FAITHFUL (Unity already routes every City sound through NativeA; `citygrind.dino.death_audio_synchronous_no_falling_state`: PROVEN; `combat.f3_audio.retrigger_mechanism_and_cadence_gap`: PROVEN_NATIVE / IMPLEMENTED / AUTOMATED_FOCUSED_PASS / DEVICE_AB_PENDING; `dino.death_sfx.per_variant_native_contract`: PROVEN; `weapon_swap.audio.hard_asset_absence`: PROVEN |
| Confidence of this dossier | PROVEN (registry audio.dino_death_sound_shares_the_28_voice_cap) |
| Depth | deep dossier (hand-written notes) |

## What it does

Tests the effects-on byte (byte_214FBC), then calls AudioManager::Play (sub_165298) with the sound name. Death, fire and UI sounds all go through it.

## Native instructions (annotated excerpts)

Address, raw bytes, disassembly in IDA 9.2's rendering. The bytes are exact; the notes are interpretation.

**Prologue and the start of the effects-on test and call.**

```
0x7EB04  f0b5         PUSH            {R4-R7,LR}
0x7EB06  03af         ADD             R7, SP, #0xC
0x7EB08  2de9000d     PUSH.W          {R8,R10,R11}
0x7EB0C  adf14004     SUB.W           R4, SP, #0x40 ; '@'
0x7EB10  24f00f04     BIC.W           R4, R4, #0xF
0x7EB14  a546         MOV             SP, R4
0x7EB16  04f9ed82     VST1.64         {D8-D11}, [R4@128]!
0x7EB1A  04f9efc2     VST1.64         {D12-D15}, [R4@128]
0x7EB1E  94b0         SUB             SP, SP, #0x50
0x7EB20  0292         STR             R2, [SP,#0x90+var_88]
0x7EB22  0446         MOV             R4, R0
0x7EB24  0191         STR             R1, [SP,#0x90+var_8C]
0x7EB26  46f28241c0f21901 MOV             R1, #(byte_214FBC - 0x7EB3A)
0x7EB2E  4df2fc42c0f21702 MOV             R2, #(___gxx_personality_sj0_ptr - 0x7EB3C)
0x7EB36  7944         ADD             R1, PC; byte_214FBC
0x7EB38  7a44         ADD             R2, PC; ___gxx_personality_sj0_ptr
0x7EB3A  4348         LDR             R0, =(unk_1D0ACC - 0x7EB44)
0x7EB3C  0d78         LDRB            R5, [R1]
0x7EB3E  1168         LDR             R1, [R2]; ___gxx_personality_sj0
0x7EB40  7844         ADD             R0, PC; unk_1D0ACC
0x7EB42  0d91         STR             R1, [SP,#0x90+fctx.personality]
0x7EB44  4149         LDR             R1, =(loc_7EBE4 - 0x7EB58)
0x7EB46  0e90         STR             R0, [SP,#0x90+fctx.lsda]
0x7EB48  07a8         ADD             R0, SP, #0x90+fctx; lpfctx
0x7EB4A  41f00101     ORR.W           R1, R1, #1
0x7EB4E  0f97         STR             R7, [SP,#0x90+fctx.jbuf]
0x7EB50  cdf844d0     STR.W           SP, [SP,#0x90+var_4C]
0x7EB54  7944         ADD             R1, PC; loc_7EBE4
0x7EB56  1091         STR             R1, [SP,#0x90+var_50]
0x7EB58  38f10eee     BLX             __Unwind_SjLj_Register
0x7EB5C  05b3         CBZ             R5, loc_7EBA0
0x7EB5E  4df2e650c0f21700 MOV             R0, #(off_1FC150 - 0x7EB6A)
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
void __fastcall sub_7EB04(int a1, int a2, int a3)
{
  __int64 v3; // d8
  __int64 v4; // d9
  __int64 v5; // d10
  __int64 v6; // d11
  __int64 v7; // d12
  __int64 v8; // d13
  __int64 v9; // d14
  __int64 v10; // d15
  unsigned int *v11; // r1
  int v12; // r2
  int v13; // [sp+0h] [bp-A8h]
  _BYTE v16[4]; // [sp+Ch] [bp-9Ch] BYREF
  int v17; // [sp+10h] [bp-98h] BYREF
  __int64 v18; // [sp+50h] [bp-58h]
  __int64 v19; // [sp+58h] [bp-50h]
  __int64 v20; // [sp+60h] [bp-48h]
  __int64 v21; // [sp+68h] [bp-40h]
  __int64 v22; // [sp+70h] [bp-38h]
```

## References found in the code

Strings referenced: none

Globals referenced: `byte_214FBC`, `dword_21C710`

Callers (with call sites): `0xCF14`@`0xCFE2`, `0xCFE8`@`0xD000`, `0xD004`@`0xD100`, `0x10294`@`0x103B2`, `0x10554`@`0x10B86`, `0x1141C`@`0x1175C`, `0x11980`@`0x1204C`, `0x13B10`@`0x13CD0`, `0x17AE4`@`0x18014`, `0x1B13C`@`0x1B226`, `0x1B13C`@`0x1B2D6`, `0x1B13C`@`0x1B334`

Callees: `0x165298`, `0x1B7478`, `0x1B7508`, `0x1B7778`, `0x1B7788`, `0x1B7798`

## Unity translation

- sound gate: `Assets/DinoCap/PortV2/Runtime/NativeAudioSessionGate.cs` line 102
- Code comments citing this address (3): `Runtime/CombatSandboxV2.cs:1153`, `Runtime/CombatSandboxV2.cs:1170`, `Runtime/CombatSandboxV2.cs:3727`

## Validation

- Tests (audio gate tests)

## Open / limits

The 28-voice admission compare is inside sub_165298 and is not shown in the excerpt.
