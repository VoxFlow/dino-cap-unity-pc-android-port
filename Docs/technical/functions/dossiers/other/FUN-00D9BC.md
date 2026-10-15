# sub_D9BC: citygrind.debris_chunk.no_ground_contact_audio

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-00D9BC` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0xD9BC` |
| RVA (EA - 0x4000) | `0x99BC` |
| File offset | `0x99BC` |
| Size | 218 bytes, 77 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_D9BC` (IDA automatic name) |
| Bytes SHA-256 | `750739f7ba56798f99768b845982574caecff0c7710fb47340f4a15d92446dbd` |
| Subsystem | other |
| Direct callers | 0 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `citygrind.debris_chunk.no_ground_contact_audio`: PROVEN |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `citygrind.debris_chunk.no_ground_contact_audio` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): Native's own debris-chunk death-particle system (BeginStandardEnemyDeath/sub_11980) has NO ground-contact audio trigger. Exhaustively proven across all 4 theoretically possible paths: (1) the death function itself registers a named-timer tag only for the DYING

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0xD9BC  f0b5         PUSH            {R4-R7,LR}
0xD9BE  03af         ADD             R7, SP, #0xC
0xD9C0  2de9000d     PUSH.W          {R8,R10,R11}
0xD9C4  adf14004     SUB.W           R4, SP, #0x40 ; '@'
0xD9C8  24f00f04     BIC.W           R4, R4, #0xF
0xD9CC  a546         MOV             SP, R4
0xD9CE  04f9ed82     VST1.64         {D8-D11}, [R4@128]!
0xD9D2  04f9efc2     VST1.64         {D12-D15}, [R4@128]
0xD9D6  90b0         SUB             SP, SP, #0x40
0xD9D8  1c20         MOVS            R0, #0x1C; unsigned int
0xD9DA  a9f146ee     BLX             __Znwm; operator new(ulong)
0xD9DE  4ef24a62     MOVW            R2, #:lower16:(___gxx_personality_sj0_ptr - 0xD9EE)
0xD9E2  2d49         LDR             R1, =(unk_1CEA74 - 0xD9F0)
0xD9E4  c0f21e02     MOVT            R2, #:upper16:(___gxx_personality_sj0_ptr - 0xD9EE)
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
int sub_D9BC()
{
  float32x2_t v0; // d0
  float32x2_t v1; // d16
  int v3; // [sp+8h] [bp-90h]

  v3 = operator new(0x1Cu);
  sub_188BB4(v3);
  v1.f32[0] = -10.0;
  v1.f32[1] = -10.0;
  *(_DWORD *)v3 = &off_1FD848;
  v0.f32[0] = flt_214FD4;
  *(_DWORD *)(v3 + 8) = vadd_f32(v0, v1).u32[0];
  *(_BYTE *)(v3 + 12) = 0;
  *(_DWORD *)(v3 + 16) = 1;
  return v3;
}
```

## References found in the code

Strings referenced: none

Globals referenced: `flt_214FD4`

Callers (with call sites): none by direct reference

Callees: `0x188BB4`, `0x1B7648`, `0x1B7668`, `0x1B7778`, `0x1B7788`, `0x1B7798`

## Unity translation

- No Unity code comment cites this address: **unmapped** (not proof the behaviour is absent from the port).

## Validation

- No test is individually linked to this function.

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
