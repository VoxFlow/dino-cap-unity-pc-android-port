# sub_666BC: Melee press handler (player)

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-0666BC` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x666BC` |
| RVA (EA - 0x4000) | `0x626BC` |
| File offset | `0x626BC` |
| Size | 242 bytes, 79 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_666BC` (IDA automatic name) |
| Bytes SHA-256 | `40f313dd44eea70bc6b26668ce5d572ab86dc38f994103f014359b2939840934` |
| Subsystem | input-combat |
| Direct callers | 3 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `citygrind.melee_ranged.arbitration_no_queue_melee_wins`: PROVEN_NATIVE_ARM / IMPLEMENTED (City Grind) / AUTOMATED_FOCUSED_PASS / DEVICE_AB_PENDING; `citygrind.rage_face.native_fireup_timer_and_erase_events`: PROVEN_NATIVE_ARM / IMPLEMENTED (City Grind) / AUTOMATED_FOCUSED_PASS / DEVICE_AB_PENDING; `player.melee_flag.survives_bite_and_restarts_melee_after_hurt`: PROVEN_NATIVE_ARM (every link) / IMPLEMENTED_IN_UNITY / AUTOMATED_FOCUSED_PASS / USER_UNIT |
| Confidence of this dossier | PROVEN_NATIVE_ARM (gate and flag-clear) |
| Depth | deep dossier (hand-written notes) |

## What it does

Accepts a melee press only in player states <= 2 with the action-allowed byte (+0xBD) set; on success starts the swing and clears the melee flag byte (byte_21BE74). A rejected press branches past the clearing store.

## Native instructions (annotated excerpts)

Address, raw bytes, disassembly in IDA 9.2's rendering. The bytes are exact; the notes are interpretation.

**State gate: state (+0xB4) compared with 2 (BHI to the reject label), then +0xBD tested.**

```
0x666C2  0446         MOV             R4, R0
0x666C4  d4f8b400     LDR.W           R0, [R4,#0xB4]
0x666C8  0228         CMP             R0, #2
0x666CA  6ed8         BHI             loc_667AA
0x666CC  94f8bd00     LDRB.W          R0, [R4,#0xBD]
0x666D0  0028         CMP             R0, #0
```

**Reject label 0x667AA is after the store at 0x667A8; the flag-clear store is skipped when the press is rejected.**

```
0x667A0  04f19801     ADD.W           R1, R4, #0x98
0x667A4  41f98f2a     VST1.32         {D18-D19}, [R1]
0x667A8  0670         STRB            R6, [R0]
0x667AA  03b0         ADD             SP, SP, #0xC
0x667AC  f0bd         POP             {R4-R7,PC}
```

## Decompiler excerpt

Hex-Rays produced no output for this function in the scripted export.

## References found in the code

Strings referenced: `playerHitAnimation` (`0x1D72E0`)

Globals referenced: `byte_21BE74`, `dword_21BE00`, `dword_21BE0C`

Callers (with call sites): `0x17AE4`@`0x17DB4`, `0x56E94`@`0x57166`, `0x66550`@`0x66696`

Callees: `0x6622C`, `0x66310`, `0x7677C`, `0x76954`, `0x76980`, `0x172D50`, `0x173A98`, `0x1B74C8`, `0x1B74E8`

## Unity translation

- melee start: `Assets/DinoCap/PortV2/Runtime/CityGridController.cs` line 5101
- melee press flag: `Assets/DinoCap/PortV2/Runtime/CityGridController.cs` line 5411
- Code comments citing this address (7): `Runtime/CityGridController.cs:1515`, `Runtime/CityGridController.cs:2368`, `Runtime/CityGridController.cs:5104`, `Runtime/CityGridController.cs:5140`, `Runtime/CityGridController.cs:5143`, `Runtime/CityGridController.cs:5406`, `Runtime/CombatSandboxV2.cs:1362`

## Validation

- Tests/Task027N236CityGridRepro.cs (melee tests)
- Tests/DC1EightDefectRepro.cs

## Open / limits

The exact full branch structure of the success path was recorded in the registry; only gate windows are excerpted.
