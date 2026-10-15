# sub_58B5C: citygrind.shop_screen.entry_trigger_and_layout

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-058B5C` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x58B5C` |
| RVA (EA - 0x4000) | `0x54B5C` |
| File offset | `0x54B5C` |
| Size | 728 bytes, 251 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_58B5C` (IDA automatic name) |
| Bytes SHA-256 | `d5c66521e4a30fe4f1d9d2c09cdd41e7da31717925f35b62559c065f4a1e6a9e` |
| Subsystem | saves-shop-progression |
| Direct callers | 0 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `citygrind.shop_screen.entry_trigger_and_layout`: PROVEN (fade mechanism/duration, relative layout) / HARD_BLOCKED (literal entry-trigger ca |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `citygrind.shop_screen.entry_trigger_and_layout` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): Shop entry's own scene-state=5 write is reachable via 2 independent candidates - sub_56118's own case18 (unconditional, no control-identity check) and sub_7334C (an identical single-instruction state write, found via the shop-exit-trigger family) - BOTH proven

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0x58B5C  f0b5         PUSH            {R4-R7,LR}
0x58B5E  03af         ADD             R7, SP, #0xC
0x58B60  2de9000d     PUSH.W          {R8,R10,R11}
0x58B64  adf14004     SUB.W           R4, SP, #0x40 ; '@'
0x58B68  24f00f04     BIC.W           R4, R4, #0xF
0x58B6C  a546         MOV             SP, R4
0x58B6E  04f9ed82     VST1.64         {D8-D11}, [R4@128]!
0x58B72  04f9efc2     VST1.64         {D12-D15}, [R4@128]
0x58B76  a8b0         SUB             SP, SP, #0xA0
0x58B78  8246         MOV             R10, R0
0x58B7A  23ac         ADD             R4, SP, #0xE0+var_54
0x58B7C  cdf810a0     STR.W           R10, [SP,#0xE0+var_D0]
0x58B80  43f2c840c0f21a00 MOV             R0, #(___stack_chk_guard_ptr - 0x58B90)
0x58B88  43f28a41     MOVW            R1, #:lower16:(off_1FC020 - 0x58B96)
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
int __fastcall sub_58B5C(_DWORD *a1)
{
  float32x2_t v1; // d8
  float32x2_t v2; // d9
  __int64 v3; // d10
  float32x2_t v4; // d11
  float32x2_t v5; // d12
  float32x2_t v6; // d13
  __int64 v7; // d14
  float32x2_t v8; // d15
  _DWORD *v9; // r10
  int v10; // r2
  char v11; // r5
  float32x2_t v12; // d14
  unsigned int v13; // r4
  int v14; // r0
  int v15; // r0
  unsigned int *v16; // r2
  int v17; // r1
  void (__fastcall *v18)(int, float *); // r2
```

## References found in the code

Strings referenced: `icon_num%dFrame` (`0x1D4B09`)

Globals referenced: `dword_21BDF8`, `flt_21BDF0`, `flt_58E38`, `flt_58E3C`, `flt_58E40`, `flt_58E44`, `flt_58E48`, `flt_58E4C`, `flt_58E50`

Callers (with call sites): none by direct reference

Callees: `0x17C968`, `0x183D54`, `0x185F70`, `0x1B4758`, `0x1B7478`, `0x1B7508`, `0x1B7778`, `0x1B7788`, `0x1B7798`, `0x1B7888`, `0x1B7FE8`

## Unity translation

- Code comments citing this address (2): `Runtime/CityGridController.cs:3149`, `Runtime/CityGridController.cs:8213`

## Validation

- No test is individually linked to this function.

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
