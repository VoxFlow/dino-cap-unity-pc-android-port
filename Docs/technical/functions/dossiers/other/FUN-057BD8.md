# sub_57BD8: n255.jump_in.side_is_the_side_the_player_faces

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-057BD8` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x57BD8` |
| RVA (EA - 0x4000) | `0x53BD8` |
| File offset | `0x53BD8` |
| Size | 2230 bytes, 662 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_57BD8` (IDA automatic name) |
| Bytes SHA-256 | `46af5f57cedcbf765752bff945147486548e800209ea3ead4efb60f09eb288c1` |
| Subsystem | other |
| Direct callers | 3 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `n255.jump_in.side_is_the_side_the_player_faces`: PROVEN_NATIVE_IMPLEMENTED |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `n255.jump_in.side_is_the_side_the_player_faces` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): Jump-in dinos (species 1 and 2, native entry roll >=75 of 99) always spawn on the side the player FACES. Player byte +0x60 is the facing flag: the player movement update sub_65D1C sets +0x60=1 (and +0x7D=1, with a NEGATIVE x velocity = -dword_21BDF4) when the 

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0x57BD8  f0b5         PUSH            {R4-R7,LR}
0x57BDA  03af         ADD             R7, SP, #0xC
0x57BDC  2de9000d     PUSH.W          {R8,R10,R11}
0x57BE0  adf14004     SUB.W           R4, SP, #0x40 ; '@'
0x57BE4  24f00f04     BIC.W           R4, R4, #0xF
0x57BE8  a546         MOV             SP, R4
0x57BEA  04f9ed82     VST1.64         {D8-D11}, [R4@128]!
0x57BEE  04f9efc2     VST1.64         {D12-D15}, [R4@128]
0x57BF2  a8b0         SUB             SP, SP, #0xA0
0x57BF4  0024         MOVS            R4, #0
0x57BF6  8046         MOV             R8, R0
0x57BF8  1894         STR             R4, [SP,#0xE0+var_80]
0x57BFA  d8f82000     LDR.W           R0, [R8,#0x20]
0x57BFE  0a91         STR             R1, [SP,#0xE0+var_B8]; int
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
int __fastcall sub_57BD8(_DWORD *a1, int a2)
{
  float32x2_t v2; // d0
  float32x2_t v3; // d1
  float32x2_t v4; // d2
  __int64 v5; // d8
  __int64 v6; // d9
  __int64 v7; // d10
  __int64 v8; // d11
  __int64 v9; // d12
  __int64 v10; // d13
  __int64 v11; // d14
  __int64 v12; // d15
  int v13; // r4
  _DWORD *v14; // r8
  const void *v15; // r0
  int v16; // r1
  int v17; // r0
  const void *v18; // r0
  _DWORD *v19; // r4
```

## References found in the code

Strings referenced: `dinosaurClassiPad` (`0x1D4F18`), `Audio_enemyAudio` (`0x1D692B`)

Globals referenced: `dword_214FB0`, `dword_214FC0`, `flt_214FD4`, `flt_21B9CC`, `flt_21B9D0`, `flt_5802C`, `flt_58030`, `flt_58034`, `flt_58038`, `flt_5803C`, `flt_58040`, `flt_58044`

Callers (with call sites): `0x5744C`@`0x5753A`, `0x5744C`@`0x57618`, `0x5744C`@`0x576AE`

Callees: `0xFCFC`, `0xFF68`, `0x12D04`, `0x76954`, `0x7EB04`, `0x16669C`, `0x172BC8`, `0x172BD0`, `0x172D18`, `0x172D50`, `0x173A98`, `0x173ABC`, `0x173B50`, `0x17C720`, `0x17C964`, `0x1B46F8`

## Unity translation

- Code comments citing this address (7): `Runtime/CityGridController.cs:4007`, `Runtime/CityGridController.cs:4011`, `Runtime/CityGridController.cs:4047`, `Runtime/CityGridController.cs:4194`, `Runtime/CityGridController.cs:6477`, `Runtime/CombatSandboxV2.cs:4116`, `Runtime/JungleSpawnPlacement.cs:40`

## Validation

- No test is individually linked to this function.

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
