# sub_D5BC0: citygrind.decoration_prop_worldy.device_branch_and_identity_FINAL

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-0D5BC0` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0xD5BC0` |
| RVA (EA - 0x4000) | `0xD1BC0` |
| File offset | `0xD1BC0` |
| Size | 1574 bytes, 533 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_D5BC0` (IDA automatic name) |
| Bytes SHA-256 | `70bc7d799870443ae41f2688ec4f93af615d8311cd9d470eeb47145c68e09ed6` |
| Subsystem | engine-platform |
| Direct callers | 0 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `citygrind.decoration_prop_worldy.device_branch_and_identity_FINAL`: PROVEN (device branch, formula, identity) / HARD_BLOCKED (render-pivot mechanism, exhausti |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `citygrind.decoration_prop_worldy.device_branch_and_identity_FINAL` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): CONFIRMS the iPad branch (byte_214FC8==0, flt_214FD4=-104.0) is PROVEN authoritative for the real shipped hardware (sub_175B88 is a genuine Info.plist/UIDevice.model iPad check, TRUE on real iPad; independently corroborated by the Extracted corpus itself being

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0xD5BC0  f0b5         PUSH            {R4-R7,LR}
0xD5BC2  03af         ADD             R7, SP, #0xC
0xD5BC4  2de9000d     PUSH.W          {R8,R10,R11}
0xD5BC8  adf14004     SUB.W           R4, SP, #0x40 ; '@'
0xD5BCC  24f00f04     BIC.W           R4, R4, #0xF
0xD5BD0  a546         MOV             SP, R4
0xD5BD2  04f9ed82     VST1.64         {D8-D11}, [R4@128]!
0xD5BD6  04f9efc2     VST1.64         {D12-D15}, [R4@128]
0xD5BDA  a8b0         SUB             SP, SP, #0xA0
0xD5BDC  0190         STR             R0, [SP,#0xE0+var_DC]
0xD5BDE  41f65240c0f21000 MOV             R0, #(aTestappOnload - 0xD5BEA); "testApp::OnLoad"
0xD5BE6  7844         ADD             R0, PC; "testApp::OnLoad"
0xD5BE8  94f056fa     BL              sub_16A098
0xD5BEC  46f25860c0f21200 MOV             R0, #(off_1FC254 - 0xD5BFC)
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
void __fastcall sub_D5BC0(int a1)
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

Strings referenced: `weapon.prop` (`0x1D57F4`), `PreTitle` (`0x1D7795`), `PreTitleiPad` (`0x1D77B1`), `testApp::OnLoad` (`0x1D783C`), `Image/Conf/lifedinosaur.prop` (`0x1D784C`), `Image/Conf/dinosaur.prop` (`0x1D7869`), `Image/Conf/iPad/dinosaur.prop` (`0x1D7882`), `Image/Conf/iPad/lifedinosaur.prop` (`0x1D78A0`)

Globals referenced: `byte_214FC8`, `byte_21BE68`, `dword_21BE00`, `dword_21C710`

Callers (with call sites): none by direct reference

Callees: `0x1BDC0`, `0x7EDA8`, `0x8925C`, `0xBEE60`, `0xBF1F4`, `0xE8E48`, `0xEB9E4`, `0x164BA8`, `0x16A098`, `0x1706D4`, `0x175B88`, `0x18FEAC`, `0x194D44`, `0x196C8C`, `0x1B7478`, `0x1B7508`

## Unity translation

- No Unity code comment cites this address: **unmapped** (not proof the behaviour is absent from the port).

## Validation

- No test is individually linked to this function.

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
