# sub_68CD8: citygrind.building.player_entry_exit_no_root_transform_motion

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-068CD8` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x68CD8` |
| RVA (EA - 0x4000) | `0x64CD8` |
| File offset | `0x64CD8` |
| Size | 190 bytes, 65 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_68CD8` (IDA automatic name) |
| Bytes SHA-256 | `a80b115e30a6dfc2e588f691df375903e630594c1414c7ccbdb7577999a8cba8` |
| Subsystem | doors-buildings |
| Direct callers | 2 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `citygrind.building.player_entry_exit_no_root_transform_motion`: PROVEN; `citygrind.building.player_entry_exit_no_sprite_level_presentation_either`: FALSIFIED; `citygrind.player.house_entry_exit.no_dedicated_animation`: SUPERSEDED by citygrind.player.house_family_full_c2b_merge (sub_68CD8 entry plays the fami; `citygrind.player.house_family_full_c2b_merge`: PROVEN; `citygrind.rage_face.native_fireup_timer_and_erase_events`: PROVEN_NATIVE_ARM / IMPLEMENTED (City Grind) / AUTOMATED_FOCUSED_PASS / DEVICE_AB_PENDING; `player.melee_flag.survives_bite_and_restarts_melee_after_hurt`: PROVEN_NATIVE_ARM (every link) / IMPLEMENTED_IN_UNITY / AUTOMATED_FOCUSED_PASS / USER_UNIT |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `citygrind.building.player_entry_exit_no_root_transform_motion` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): Native's own entry/exit freeze handler (sub_68CD8) and its sole caller (sub_CF14) never write the player's own position/velocity during building entry or exit - the player freezes in place (fixed X AND Y) while the door animates. No jump-named clip exists anyw

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0x68CD8  b0b5         PUSH            {R4,R5,R7,LR}; int
0x68CDA  02af         ADD             R7, SP, #8
0x68CDC  83b0         SUB             SP, SP, #0xC; int
0x68CDE  0446         MOV             R4, R0
0x68CE0  0129         CMP             R1, #1
0x68CE2  44d1         BNE             loc_68D6E
0x68CE4  0620         MOVS            R0, #6
0x68CE6  0025         MOVS            R5, #0
0x68CE8  c4f8b400     STR.W           R0, [R4,#0xB4]
0x68CEC  84f8bd50     STRB.W          R5, [R4,#0xBD]
0x68CF0  94f8bc00     LDRB.W          R0, [R4,#0xBC]
0x68CF4  68b1         CBZ             R0, loc_68D12
0x68CF6  2046         MOV             R0, R4
0x68CF8  fdf70afb     BL              sub_66310
```

**Around 0x68D22 (an address cited by a recovered rule)**

```
0x68D16  4ef23662c0f21602 MOV             R2, #(aHouseanim - 0x68D24); "houseAnim"
0x68D1E  2046         MOV             R0, R4; int
0x68D20  7a44         ADD             R2, PC; "houseAnim"
0x68D22  0df02bfd     BL              sub_7677C
0x68D26  4ff2ea42     MOVW            R2, #:lower16:(asc_1D8220 - 0x68D36); ""
0x68D2A  2046         MOV             R0, R4; int
0x68D2C  c0f21602     MOVT            R2, #:upper16:(asc_1D8220 - 0x68D36); ""
```

**Around 0x68D4A (an address cited by a recovered rule)**

```
0x68D44  2046         MOV             R0, R4
0x68D46  0021         MOVS            R1, #0
0x68D48  0022         MOVS            R2, #0
0x68D4A  0df019fe     BL              sub_76980
0x68D4E  6068         LDR             R0, [R4,#4]
0x68D50  09f1feff     BL              sub_172D50
0x68D54  c0ef1000     VMOV.I32        D16, #0
```

**Around 0x68D8A (an address cited by a recovered rule)**

```
0x68D84  0021         MOVS            R1, #0
0x68D86  0322         MOVS            R2, #3
0x68D88  0025         MOVS            R5, #0
0x68D8A  0df0f9fd     BL              sub_76980
0x68D8E  c4f8e050     STR.W           R5, [R4,#0xE0]
0x68D92  03b0         ADD             SP, SP, #0xC
0x68D94  b0bd         POP             {R4,R5,R7,PC}
```

## Decompiler excerpt

Hex-Rays produced no output for this function in the scripted export.

## References found in the code

Strings referenced: `houseAnim` (`0x1D735A`)

Globals referenced: none among the named globals

Callers (with call sites): `0xCF14`@`0xCF9E`, `0xD108`@`0xD16A`

Callees: `0x6622C`, `0x66310`, `0x7677C`, `0x76954`, `0x76980`, `0x172D50`, `0x173A98`, `0x1B74E8`

## Unity translation

- Code comments citing this address (7): `Runtime/CityGridController.cs:1471`, `Runtime/CityGridController.cs:2802`, `Runtime/CityGridController.cs:2823`, `Runtime/CityGridController.cs:2824`, `Runtime/CityGridController.cs:2835`, `Runtime/CityGridController.cs:4010`, `Runtime/CityGridController.cs:6601`

## Validation

- No test is individually linked to this function.

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
