# sub_59DEC: player.lives_counter.dword_21bdfc

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-059DEC` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x59DEC` |
| RVA (EA - 0x4000) | `0x55DEC` |
| File offset | `0x55DEC` |
| Size | 12832 bytes, 3993 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_59DEC` (IDA automatic name) |
| Bytes SHA-256 | `5adcef97ab1640b13e0726c3fec67398879cb04548f35114c8e46cb1df44e5ab` |
| Subsystem | saves-shop-progression |
| Direct callers | 0 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `player.lives_counter.dword_21bdfc`: PROVEN; `save_file.schema.dinocap_sav`: PROVEN |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `player.lives_counter.dword_21bdfc` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): dword_21BDFC is the player's lives/continue counter. Decremented by exactly 1 at player-death-state entry (sub_66550, case 4 of a switch on *(a1+180), the player's own state field - guarded on a mainSceneiPad-object internal field != 3). Consumed by the 'DieEn

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0x59DEC  f0b5         PUSH            {R4-R7,LR}
0x59DEE  03af         ADD             R7, SP, #0xC
0x59DF0  2de9000d     PUSH.W          {R8,R10,R11}
0x59DF4  adf14004     SUB.W           R4, SP, #0x40 ; '@'
0x59DF8  24f00f04     BIC.W           R4, R4, #0xF
0x59DFC  a546         MOV             SP, R4
0x59DFE  04f9ed82     VST1.64         {D8-D11}, [R4@128]!
0x59E02  04f9efc2     VST1.64         {D12-D15}, [R4@128]
0x59E06  adf5ac6d     SUB.W           SP, SP, #0x560; float
0x59E0A  0146         MOV             R1, R0
0x59E0C  4868         LDR             R0, [R1,#4]
0x59E0E  8a46         MOV             R10, R1
0x59E10  22f1aafd     BL              sub_17C968
0x59E14  42f20f71     MOVW            R1, #0x270F
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
void __fastcall sub_59DEC(int a1)
{
  __int64 v1; // d8
  __int64 v2; // d9
  __int64 v3; // d10
  __int64 v4; // d11
  __int64 v5; // d12
  __int64 v6; // d13
  __int64 v7; // d14
  __int64 v8; // d15
  int v10; // r0
  int v11; // r4
  void (__fastcall *v12)(int, _DWORD *); // r2
  int v13; // r4
  unsigned int *v14; // r1
  int v15; // r2
  unsigned int *v16; // r1
  int v17; // r2
  unsigned int *v18; // r1
  int v19; // r2
```

## References found in the code

Strings referenced: `Music_menuAudio` (`0x1D5783`), `ui0Image` (`0x1D6F50`), `Image/iPad/ui0.png` (`0x1D6F59`), `menu_bgFrame` (`0x1D6F6C`), `menu_logoFrame` (`0x1D6F79`), `text_newgameFrame` (`0x1D6F88`), `text_newgame_dFrame` (`0x1D6F9A`), `text_resume_dFrame` (`0x1D6FAE`)

Globals referenced: `dword_214FC4`, `dword_21B1A8`, `dword_21BDFC`, `dword_21C708`, `flt_21BDF0`

Callers (with call sites): none by direct reference

Callees: `0x5D00C`, `0x5D064`, `0x7E298`, `0x7E868`, `0x7E970`, `0x7EC50`, `0x7F008`, `0x172C0C`, `0x172C90`, `0x172D1C`, `0x1745A8`, `0x17C720`, `0x17C870`, `0x17C968`, `0x17E390`, `0x190028`

## Unity translation

- Code comments citing this address (3): `Runtime/CityGridController.cs:4808`, `Runtime/MainMenuController.cs:11`, `Runtime/MainMenuController.cs:215`

## Validation

- No test is individually linked to this function.

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
