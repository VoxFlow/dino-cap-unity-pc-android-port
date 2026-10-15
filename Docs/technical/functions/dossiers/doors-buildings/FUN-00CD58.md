# sub_CD58: citygrind.building.composite_sprite_draw_layer10

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-00CD58` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0xCD58` |
| RVA (EA - 0x4000) | `0x8D58` |
| File offset | `0x8D58` |
| Size | 194 bytes, 69 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_CD58` (IDA automatic name) |
| Bytes SHA-256 | `ea9a3d2ddfd736d126276b4f27c8be098a0b8273bb8417beec21fa98db147b72` |
| Subsystem | doors-buildings |
| Direct callers | 0 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `citygrind.building.composite_sprite_draw_layer10`: PROVEN |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `citygrind.building.composite_sprite_draw_layer10` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): houseClassiPad ctor sub_CD58 invokes composite ctor sub_764C4; native composite sprite layer+0x5c=10 is propagated to child sprite+36 by sub_7677C. Props layer4 from sub_54FA0 is retained. Unity body/md/door all use native layer10; existing local Z preserves c

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0xCD58  f0b5         PUSH            {R4-R7,LR}
0xCD5A  03af         ADD             R7, SP, #0xC
0xCD5C  2de9000d     PUSH.W          {R8,R10,R11}
0xCD60  adf14004     SUB.W           R4, SP, #0x40 ; '@'
0xCD64  24f00f04     BIC.W           R4, R4, #0xF
0xCD68  a546         MOV             SP, R4
0xCD6A  04f9ed82     VST1.64         {D8-D11}, [R4@128]!
0xCD6E  04f9efc2     VST1.64         {D12-D15}, [R4@128]
0xCD72  90b0         SUB             SP, SP, #0x40; std::string *
0xCD74  9020         MOVS            R0, #0x90; unsigned int
0xCD76  aaf178ec     BLX             __Znwm; operator new(ulong)
0xCD7A  4ff2ae22     MOVW            R2, #:lower16:(___gxx_personality_sj0_ptr - 0xCD8A)
0xCD7E  2749         LDR             R1, =(unk_1CEA38 - 0xCD8C)
0xCD80  c0f21e02     MOVT            R2, #:upper16:(___gxx_personality_sj0_ptr - 0xCD8A)
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
int sub_CD58()
{
  int v0; // r4
  int v1; // r1
  int v2; // r2
  int v3; // r3
  std::string *v5; // [sp+0h] [bp-98h]
  std::string *v6; // [sp+4h] [bp-94h]
  int v7; // [sp+8h] [bp-90h]
  int fctx; // [sp+Ch] [bp-8Ch]
  int fctx_4; // [sp+10h] [bp-88h]
  int fctx_8; // [sp+14h] [bp-84h]

  v0 = operator new(0x90u);
  sub_764C4(v0, v1, v2, v3, v5, v6, v0, fctx, fctx_4, fctx_8);
  *(_DWORD *)v7 = &off_1FD788;
  *(_DWORD *)(v7 + 128) = 1;
  *(_DWORD *)(v7 + 136) = 0;
  *(_BYTE *)(v7 + 132) = 1;
  return v7;
```

## References found in the code

Strings referenced: none

Globals referenced: none among the named globals

Callers (with call sites): none by direct reference

Callees: `0x764C4`, `0x1B7648`, `0x1B7668`, `0x1B7778`, `0x1B7788`, `0x1B7798`

## Unity translation

- Code comments citing this address (1): `Runtime/CityGridController.cs:1083`

## Validation

- No test is individually linked to this function.

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
