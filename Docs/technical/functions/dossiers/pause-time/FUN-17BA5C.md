# sub_17BA5C: Scene update: scaled delta time

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-17BA5C` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x17BA5C` |
| RVA (EA - 0x4000) | `0x177A5C` |
| File offset | `0x177A5C` |
| Size | 1306 bytes, 492 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_17BA5C` (IDA automatic name) |
| Bytes SHA-256 | `f45fd277ab535089ad07600d54f757449b1bd9c75f1c278f92a594733ca9a8a0` |
| Subsystem | pause-time |
| Direct callers | 2 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `citygrind.pause.moving_particles_freeze`: PROVEN_NATIVE_MECHANISM / IMPLEMENTED (Unity vessel = pause gate) / AUTOMATED_FOCUSED_PASS; `combat.f3_audio.retrigger_mechanism_and_cadence_gap`: PROVEN_NATIVE / IMPLEMENTED / AUTOMATED_FOCUSED_PASS / DEVICE_AB_PENDING |
| Confidence of this dossier | PROVEN (consumer list in the registry); only the excerpted instructions are shown here |
| Depth | deep dossier (hand-written notes) |

## What it does

Computes scaledDt = dt * scene[+0xB74] and feeds it to the timer queue, every enabled GameObject tick and physics; UI uses the raw dt.

## Native instructions (annotated excerpts)

Address, raw bytes, disassembly in IDA 9.2's rendering. The bytes are exact; the notes are interpretation.

**Address-of the +0xB74 time-scale field and neighbouring scene fields.**

```
0x17BA8A  dff8ec04     LDR.W           R0, =(unk_1D3E34 - 0x17BAA2)
0x17BA8E  0132         ADDS            R2, #1
0x17BA90  c6f87c2b     STR.W           R2, [R6,#0xB7C]
0x17BA94  06f67432     ADDW            R2, R6, #0xB74
0x17BA98  c6f8804b     STR.W           R4, [R6,#0xB80]
0x17BA9C  0968         LDR             R1, [R1]; ___gxx_personality_sj0
0x17BA9E  7844         ADD             R0, PC; unk_1D3E34
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
void __fastcall sub_17BA5C(
        _DWORD *a1,
        int a2,
        int a3,
        int a4,
        int a5,
        int a6,
        int a7,
        int a8,
        int a9,
        int a10,
        int a11,
        void *a12,
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

Globals referenced: none among the named globals

Callers (with call sites): `0x170308`@`0x1705E0`, `0x17BA5C`@`0x17BEE6`

Callees: `0x164B78`, `0x16669C`, `0x166874`, `0x16CC58`, `0x172324`, `0x1724CC`, `0x17251C`, `0x172554`, `0x172580`, `0x172BC4`, `0x172BCC`, `0x1750AC`, `0x1755D4`, `0x17566C`, `0x176840`, `0x176858`

## Unity translation

- scene clock and pause gate: `Assets/DinoCap/PortV2/Runtime/CityGridController.cs` line 1377
- renderer freeze: `Assets/DinoCap/PortV2/Runtime/Power2DSourceTruthRenderer.cs` line 784
- Code comments citing this address (1): `Runtime/Power2DSourceTruthRenderer.cs:422`

## Validation

- Tests/DC1BloodBurstPauseRepro.cs

## Open / limits

The full consumer list was established by earlier analysis (registry citygrind.pause.moving_particles_freeze); this excerpt shows only where the field is addressed.
