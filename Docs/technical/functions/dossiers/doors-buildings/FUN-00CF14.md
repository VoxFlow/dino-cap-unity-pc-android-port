# sub_CF14: citygrind.building.player_entry_exit_no_root_transform_motion

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-00CF14` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0xCF14` |
| RVA (EA - 0x4000) | `0x8F14` |
| File offset | `0x8F14` |
| Size | 210 bytes, 68 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_CF14` (IDA automatic name) |
| Bytes SHA-256 | `fd543c85410d0d4aa88fdb4ad28c4d1e44ef0f8aafc31285fdd307f831af31d2` |
| Subsystem | doors-buildings |
| Direct callers | 1 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `citygrind.building.loot_grant_timing`: PARTIALLY_SUPERSEDED_BY_DIRECT_NATIVE_EVIDENCE; `citygrind.building.player_entry_exit_no_root_transform_motion`: PROVEN; `citygrind.building_loot_timing.ub_race_policy`: PROVEN (mechanism, unchanged from N.2.36.9) / POLICY_CLASSIFIED (INTENTIONAL_SHIPPED_BUG_C |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `citygrind.building.player_entry_exit_no_root_transform_motion` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): Native's own entry/exit freeze handler (sub_68CD8) and its sole caller (sub_CF14) never write the player's own position/velocity during building entry or exit - the player freezes in place (fixed X AND Y) while the door animates. No jump-named clip exists anyw

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0xCF14  f0b5         PUSH            {R4-R7,LR}
0xCF16  03af         ADD             R7, SP, #0xC
0xCF18  81b0         SUB             SP, SP, #4
0xCF1A  0446         MOV             R4, R0
0xCF1C  4cf6cd43     MOVW            R3, #0xCCCD
0xCF20  d4f88800     LDR.W           R0, [R4,#0x88]
0xCF24  c3f6cc43     MOVT            R3, #0x3CCC
0xCF28  0021         MOVS            R1, #0
0xCF2A  0022         MOVS            R2, #0
0xCF2C  0028         CMP             R0, #0
0xCF2E  04bf         ITT EQ
0xCF30  0120         MOVEQ           R0, #1
0xCF32  c4f88800     STREQ.W         R0, [R4,#0x88]
0xCF36  4ff0ff30     MOV.W           R0, #0xFFFFFFFF
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
int __fastcall sub_CF14(int a1)
{
  int v2; // r0
  const void *v3; // r0
  _DWORD *v4; // r0
  const void *v5; // r0
  _DWORD *v6; // r5

  if ( !*(_DWORD *)(a1 + 136) )
    *(_DWORD *)(a1 + 136) = 1;
  sub_76980(a1, 0, 0, 1020054733, -1);
  v2 = sub_172D14(*(_DWORD *)(a1 + 4));
  v3 = (const void *)sub_17C71C(v2);
  v4 = __dynamic_cast(
         v3,
         (const struct __class_type_info *)&`typeinfo for'Triniti2D::SceneCallBack,
         (const struct __class_type_info *)&`typeinfo for'DinoCap::mainSceneiPad,
         -1);
  v5 = (const void *)sub_172D18(v4[8]);
  v6 = 0;
```

## References found in the code

Strings referenced: `close` (`0x1D49A1`), `OpenDoorAudio` (`0x1D49A7`)

Globals referenced: none among the named globals

Callers (with call sites): `0x56118`@`0x56328`

Callees: `0x68CD8`, `0x76980`, `0x172D14`, `0x172D18`, `0x172DEC`, `0x17C71C`, `0x1B76A8`

## Unity translation

- Code comments citing this address (2): `Runtime/CityGridController.cs:199`, `Runtime/CityGridController.cs:4428`

## Validation

- No test is individually linked to this function.

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
