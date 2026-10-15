# sub_C0CE4: combat.f3_audio.retrigger_mechanism_and_cadence_gap

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-0C0CE4` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0xC0CE4` |
| RVA (EA - 0x4000) | `0xBCCE4` |
| File offset | `0xBCCE4` |
| Size | 6770 bytes, 2336 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_C0CE4` (IDA automatic name) |
| Bytes SHA-256 | `ad7a12f2c5cbb41a9a1bf711fa4cc042e7680c41912865994c95db742d974616` |
| Subsystem | input-combat |
| Direct callers | 1 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `combat.f3_audio.retrigger_mechanism_and_cadence_gap`: PROVEN_NATIVE / IMPLEMENTED / AUTOMATED_FOCUSED_PASS / DEVICE_AB_PENDING |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `combat.f3_audio.retrigger_mechanism_and_cadence_gap` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): 76A20 emits secondary animation callbacks only on flagged CAFrame+40 and changed frame. 7D81C case5 marks only secondary frame3 in the F weapon container. 66CE8 calls67CE8/Attack_F3Audio unconditionally as consumer, but the producer owns frame3-only cadence; p

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0xC0CE4  f0b5         PUSH            {R4-R7,LR}
0xC0CE6  03af         ADD             R7, SP, #0xC
0xC0CE8  2de9000d     PUSH.W          {R8,R10,R11}
0xC0CEC  adf14004     SUB.W           R4, SP, #0x40 ; '@'
0xC0CF0  24f00f04     BIC.W           R4, R4, #0xF
0xC0CF4  a546         MOV             SP, R4
0xC0CF6  04f9ed82     VST1.64         {D8-D11}, [R4@128]!
0xC0CFA  04f9efc2     VST1.64         {D12-D15}, [R4@128]
0xC0CFE  adf5087d     SUB.W           SP, SP, #0x220
0xC0D02  0390         STR             R0, [SP,#0x260+var_254]
0xC0D04  4bf21830c0f21300 MOV             R0, #(__ZNSs4_Rep20_S_empty_rep_storageE_ptr - 0xC0D14)
0xC0D0C  4bf21e31     MOVW            R1, #:lower16:(___gxx_personality_sj0_ptr - 0xC0D1A)
0xC0D10  7844         ADD             R0, PC; __ZNSs4_Rep20_S_empty_rep_storageE_ptr
0xC0D12  c0f21301     MOVT            R1, #:upper16:(___gxx_personality_sj0_ptr - 0xC0D1A)
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
void __fastcall sub_C0CE4(int a1)
{
  __int64 v1; // d8
  __int64 v2; // d9
  __int64 v3; // d10
  __int64 v4; // d11
  __int64 v5; // d12
  __int64 v6; // d13
  __int64 v7; // d14
  __int64 v8; // d15
  unsigned int *v9; // r1
  int v10; // r2
  unsigned int *v11; // r1
  int v12; // r2
  unsigned int *v13; // r1
  int v14; // r2
  unsigned int *v15; // r1
  int v16; // r2
  unsigned int *v17; // r1
  int v18; // r2
```

## References found in the code

Strings referenced: `lifeModeDuiPad` (`0x1D4B5B`), `lifeModeToolsiPad` (`0x1D4BFB`), `effectClassiPad` (`0x1D4C48`), `playerClassiPad` (`0x1D4DA4`), `bglayerClassiPad` (`0x1D4DB4`), `bulletClassiPad` (`0x1D4DE3`), `baozhaClassiPad` (`0x1D4DF3`), `particleClassiPad` (`0x1D4E03`)

Globals referenced: none among the named globals

Callers (with call sites): `0xC2758`@`0xC2758`

Callees: `0x163EFC`, `0x163F30`, `0x188D44`, `0x188FB8`, `0x188FBC`, `0x188FCC`, `0x1B7478`, `0x1B74B8`, `0x1B7508`, `0x1B7648`, `0x1B7668`, `0x1B7778`, `0x1B7788`, `0x1B7798`

## Unity translation

- No Unity code comment cites this address: **unmapped** (not proof the behaviour is absent from the port).

## Validation

- Tests/Task027N236CityGridRepro.cs
- Tests/DC1EightDefectRepro.cs

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
