# sub_65D1C: n255.jump_in.side_is_the_side_the_player_faces

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-065D1C` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x65D1C` |
| RVA (EA - 0x4000) | `0x61D1C` |
| File offset | `0x61D1C` |
| Size | 270 bytes, 93 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_65D1C` (IDA automatic name) |
| Bytes SHA-256 | `c98ed5c02b24de60c0522077c1800bd336d17609fd94d43bb40d5e331aef38a1` |
| Subsystem | input-combat |
| Direct callers | 2 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `n255.jump_in.side_is_the_side_the_player_faces`: PROVEN_NATIVE_IMPLEMENTED; `player.melee_flag.survives_bite_and_restarts_melee_after_hurt`: PROVEN_NATIVE_ARM (every link) / IMPLEMENTED_IN_UNITY / AUTOMATED_FOCUSED_PASS / USER_UNIT |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `n255.jump_in.side_is_the_side_the_player_faces` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): Jump-in dinos (species 1 and 2, native entry roll >=75 of 99) always spawn on the side the player FACES. Player byte +0x60 is the facing flag: the player movement update sub_65D1C sets +0x60=1 (and +0x7D=1, with a NEGATIVE x velocity = -dword_21BDF4) when the 

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0x65D1C  b0b5         PUSH            {R4,R5,R7,LR}
0x65D1E  02af         ADD             R7, SP, #8
0x65D20  86b0         SUB             SP, SP, #0x18
0x65D22  0446         MOV             R4, R0
0x65D24  46f29030c0f21900 MOV             R0, #(off_1FC0C0 - 0x65D30)
0x65D2C  7844         ADD             R0, PC; off_1FC0C0
0x65D2E  0068         LDR             R0, [R0]; byte_21BE6C
0x65D30  0078         LDRB            R0, [R0]
0x65D32  50b3         CBZ             R0, loc_65D8A
0x65D34  d4f8b400     LDR.W           R0, [R4,#0xB4]
0x65D38  0028         CMP             R0, #0
0x65D3A  18bf         IT NE
0x65D3C  0328         CMPNE           R0, #3
0x65D3E  5cd0         BEQ             loc_65DFA
```

**Around 0x65D1C (an address cited by a recovered rule)**

```
0x65D1C  b0b5         PUSH            {R4,R5,R7,LR}
0x65D1E  02af         ADD             R7, SP, #8
0x65D20  86b0         SUB             SP, SP, #0x18
0x65D22  0446         MOV             R4, R0
```

**Around 0x65DE6 (an address cited by a recovered rule)**

```
0x65DE0  0021         MOVS            R1, #0
0x65DE2  0391         STR             R1, [SP,#0x18+var_C]
0x65DE4  02a9         ADD             R1, SP, #0x18+var_10
0x65DE6  0df157fe     BL              sub_173A98
0x65DEA  2046         MOV             R0, R4
0x65DEC  0121         MOVS            R1, #1
0x65DEE  0022         MOVS            R2, #0
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
unsigned int __fastcall sub_65D1C(int a1)
{
  float32x2_t v1; // d0
  unsigned int result; // r0
  bool v4; // zf
  int v5; // r0
  int v6; // r0
  float *v7; // r1
  int v8; // r0
  int v9; // r0
  int v10; // r0
  double v11; // [sp+0h] [bp-20h] BYREF
  _DWORD v12[2]; // [sp+8h] [bp-18h] BYREF
  float v13[2]; // [sp+10h] [bp-10h] BYREF

  if ( byte_21BE6C )
  {
    result = *(_DWORD *)(a1 + 180);
    v4 = result == 0;
    if ( result )
```

## References found in the code

Strings referenced: none

Globals referenced: `byte_21BE6C`, `byte_21BE70`, `dword_21BDF4`, `flt_21BDF0`

Callers (with call sites): `0x65C78`@`0x65D06`, `0x66550`@`0x66622`

Callees: `0x661D8`, `0x6622C`, `0x662C0`, `0x172D50`, `0x173A98`, `0x173B50`

## Unity translation

- Code comments citing this address (2): `Runtime/CityGridController.cs:1514`, `Runtime/JungleSpawnPlacement.cs:40`

## Validation

- Tests/Task027N236CityGridRepro.cs
- Tests/DC1EightDefectRepro.cs

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
