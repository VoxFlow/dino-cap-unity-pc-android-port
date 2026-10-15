# sub_1BDC0: platform.asset_loading.native_loads_gameplay_assets_behind_the_loading_slide

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-01BDC0` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x1BDC0` |
| RVA (EA - 0x4000) | `0x17DC0` |
| File offset | `0x17DC0` |
| Size | 36390 bytes, 11146 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_1BDC0` (IDA automatic name) |
| Bytes SHA-256 | `d8bacab9d958daf8df1ba62486c4b762e0aa787897943fa28bec3b8b8168d6da` |
| Subsystem | engine-platform |
| Direct callers | 1 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `platform.asset_loading.native_loads_gameplay_assets_behind_the_loading_slide`: PROVEN_NATIVE_TRIGGER_LIKELY_SCOPE |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `platform.asset_loading.native_loads_gameplay_assets_behind_the_loading_slide` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): The original does NOT preload gameplay assets at launch. The menu scene's virtual sub_62A98 (vtable data xref 0x1FDB90) runs sub_2A8A8(this+16, [this+72]) only when its state field [this+12]==4, which is set when the 'Loading...' slide-in control (id 13, text_

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0x1BDC0  f0b5         PUSH            {R4-R7,LR}
0x1BDC2  03af         ADD             R7, SP, #0xC
0x1BDC4  2de9000d     PUSH.W          {R8,R10,R11}
0x1BDC8  adf14004     SUB.W           R4, SP, #0x40 ; '@'
0x1BDCC  24f00f04     BIC.W           R4, R4, #0xF
0x1BDD0  a546         MOV             SP, R4
0x1BDD2  04f9ed82     VST1.64         {D8-D11}, [R4@128]!
0x1BDD6  04f9efc2     VST1.64         {D12-D15}, [R4@128]
0x1BDDA  adf5a65d     SUB.W           SP, SP, #0x14C0
0x1BDDE  84b0         SUB             SP, SP, #0x10
0x1BDE0  40f24640     MOVW            R0, #:lower16:(off_1FC234 - 0x1BDEE)
0x1BDE4  0021         MOVS            R1, #0
0x1BDE6  c0f21e00     MOVT            R0, #:upper16:(off_1FC234 - 0x1BDEE)
0x1BDEA  7844         ADD             R0, PC; off_1FC234
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
void sub_1BDC0()
{
  __int64 v0; // d8
  __int64 v1; // d9
  __int64 v2; // d10
  __int64 v3; // d11
  __int64 v4; // d12
  __int64 v5; // d13
  __int64 v6; // d14
  __int64 v7; // d15
  unsigned int *v8; // r1
  int v9; // r2
  unsigned int *v10; // r1
  int v11; // r2
  unsigned int *v12; // r1
  int v13; // r2
  unsigned int *v14; // r1
  int v15; // r2
  unsigned int *v16; // r1
  int v17; // r2
```

## References found in the code

Strings referenced: `whiteFrame` (`0x1D4939`), `k4s1Frame` (`0x1D4A3E`), `k3s1Frame` (`0x1D4A48`), `k4s2Frame` (`0x1D4A5C`), `k3s2Frame` (`0x1D4A66`), `icon_itemxFrame` (`0x1D4A87`), `icon_item0Frame` (`0x1D4A97`), `icon_item1Frame` (`0x1D4AA7`)

Globals referenced: `dword_21C708`

Callers (with call sites): `0xD5BC0`@`0xD5DA6`

Callees: `0x7F008`, `0x7F1E8`, `0x7F3CC`, `0x7F74C`, `0x17E390`, `0x17EC00`, `0x1B7478`, `0x1B7508`, `0x1B7778`, `0x1B7798`

## Unity translation

- Code comments citing this address (2): `Runtime/CityGridController.cs:7858`, `Runtime/Power2DSourceTruthRenderer.cs:181`

## Validation

- No test is individually linked to this function.

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
