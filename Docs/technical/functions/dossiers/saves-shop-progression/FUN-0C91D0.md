# sub_C91D0: player.lives_counter.dword_21bdfc

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-0C91D0` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0xC91D0` |
| RVA (EA - 0x4000) | `0xC51D0` |
| File offset | `0xC51D0` |
| Size | 5526 bytes, 1858 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_C91D0` (IDA automatic name) |
| Bytes SHA-256 | `9e0c3a4a87dded4d30fc45847354f8d9a00cc6e1dd783bffc6b614054313752e` |
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
0xC91D0  f0b5         PUSH            {R4-R7,LR}
0xC91D2  03af         ADD             R7, SP, #0xC
0xC91D4  2de9000d     PUSH.W          {R8,R10,R11}
0xC91D8  adf14004     SUB.W           R4, SP, #0x40 ; '@'
0xC91DC  24f00f04     BIC.W           R4, R4, #0xF
0xC91E0  a546         MOV             SP, R4
0xC91E2  04f9ed82     VST1.64         {D8-D11}, [R4@128]!
0xC91E6  04f9efc2     VST1.64         {D12-D15}, [R4@128]
0xC91EA  adf5287d     SUB.W           SP, SP, #0x2A0; float
0xC91EE  0446         MOV             R4, R0
0xC91F0  6068         LDR             R0, [R4,#4]
0xC91F2  b3f0b7fb     BL              sub_17C964
0xC91F6  0021         MOVS            R1, #0
0xC91F8  40f2000a     MOVW            R10, #0
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
void __fastcall sub_C91D0(int a1)
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
  int v11; // r8
  int v12; // r0
  int v13; // r4
  void (__fastcall *v14)(int, _DWORD *); // r2
  float *v15; // r4
  unsigned int *v16; // r1
  int v17; // r2
  unsigned int *v18; // r1
  int v19; // r2
```

## References found in the code

Strings referenced: `Music_menuAudio` (`0x1D5783`), `ui0Image` (`0x1D6F50`), `menu_bgFrame` (`0x1D6F6C`), `text_newgameFrame` (`0x1D6F88`), `text_resumeFrame` (`0x1D6FC1`), `icon_openfeitsFrame` (`0x1D6FD2`), `helpFrame` (`0x1D7002`), `text_optionFrame` (`0x1D701F`)

Globals referenced: `dword_214FC4`, `dword_21BDFC`, `dword_21C3C8`, `dword_21C708`, `flt_21BDF0`

Callers (with call sites): none by direct reference

Callees: `0x7E298`, `0x7E868`, `0x7E970`, `0x7EC50`, `0x7F008`, `0xCA768`, `0xCA7C0`, `0x166414`, `0x172C0C`, `0x172C90`, `0x172D1C`, `0x1745A8`, `0x17C720`, `0x17C870`, `0x17C964`, `0x17C968`

## Unity translation

- Code comments citing this address (1): `Runtime/CityGridController.cs:4809`

## Validation

- No test is individually linked to this function.

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
