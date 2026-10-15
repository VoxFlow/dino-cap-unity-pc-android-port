# sub_1B95C: building.loot_popup.removal_mechanism

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-01B95C` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x1B95C` |
| RVA (EA - 0x4000) | `0x1795C` |
| File offset | `0x1795C` |
| Size | 462 bytes, 147 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_1B95C` (IDA automatic name) |
| Bytes SHA-256 | `eeb6b858d0f61ba8da7187d095443c97877670ba825dd53a99f680401c30824c` |
| Subsystem | doors-buildings |
| Direct callers | 2 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `building.loot_popup.removal_mechanism`: PARTIALLY_SUPERSEDED_BY_DIRECT_NATIVE_EVIDENCE |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `building.loot_popup.removal_mechanism` (HISTORICAL: that rule is superseded or falsified, so this is not a current-behaviour statement): The native reward-icon+digit popup (itemClassiPad) has NO timed removal/fade of any kind - its own per-frame Tick (sub_EFB4) only grows a clamped [0,1] scale scalar and never clears it; the 'close' timer callback that creates/configures it (sub_D004 via sub_F1

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0x1B95C  f0b5         PUSH            {R4-R7,LR}
0x1B95E  03af         ADD             R7, SP, #0xC
0x1B960  4df8048d     PUSH.W          {R8}
0x1B964  0c46         MOV             R4, R1
0x1B966  1e46         MOV             R6, R3
0x1B968  2068         LDR             R0, [R4]
0x1B96A  9046         MOV             R8, R2
0x1B96C  0528         CMP             R0, #5
0x1B96E  00f3d980     BGT.W           loc_1BB24
0x1B972  9cf1d2ea     BLX             _rand
0x1B976  40ec320b     VMOV            D18, R0, R0
0x1B97A  9fed6c0a     VLDR            S0, =100.0
0x1B97E  fbff2226     VCVT.F32.S32    D18, D18
0x1B982  2068         LDR             R0, [R4]
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
int __fastcall sub_1B95C(int a1, int *a2, _QWORD *a3, int a4, int a5)
{
  int v5; // r5
  float32x2_t v6; // d0
  int result; // r0
  int32x2_t v11; // d18
  int v12; // r0
  const void *v13; // r0
  int *v14; // r2
  int v15; // r1
  int v16; // r2

  result = *a2;
  if ( *a2 <= 5 )
  {
    v11.i32[0] = rand();
    v11.i32[1] = v11.i32[0];
    v6.i32[0] = 1120403456;
    v12 = *a2;
    v6.i32[0] = vcvt_f32_s32(vcvt_s32_f32(vmul_f32(vmul_f32(vcvt_f32_s32(v11), (float32x2_t)0x3000000030000000LL), v6))).u32[0];
```

## References found in the code

Strings referenced: none

Globals referenced: `dword_21BE00`, `dword_21BE24`, `flt_1BB2C`, `flt_1BB30`, `flt_1BB34`, `flt_1BB38`

Callers (with call sites): `0x1B13C`@`0x1B2F8`, `0x1B13C`@`0x1B6A0`

Callees: `0xF154`, `0x172D18`, `0x1B76A8`, `0x1B7F18`

## Unity translation

- No Unity code comment cites this address: **unmapped** (not proof the behaviour is absent from the port).

## Validation

- No test is individually linked to this function.

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
