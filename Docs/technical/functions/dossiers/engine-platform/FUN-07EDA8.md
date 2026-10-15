# sub_7EDA8: citygrind.decoration_prop_worldy.device_branch_and_identity_FINAL

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-07EDA8` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x7EDA8` |
| RVA (EA - 0x4000) | `0x7ADA8` |
| File offset | `0x7ADA8` |
| Size | 256 bytes, 78 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_7EDA8` (IDA automatic name) |
| Bytes SHA-256 | `f34235be57532ab312c9e6fdb0832a9152c8bfd263aeeeaa2c2fd176083ad34a` |
| Subsystem | engine-platform |
| Direct callers | 1 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `citygrind.decoration_prop_worldy.device_branch_and_identity_FINAL`: PROVEN (device branch, formula, identity) / HARD_BLOCKED (render-pivot mechanism, exhausti; `citygrind.decoration_prop_worldy.native_contract`: PROVEN; `citygrind.viewport_aspect_policy.native_contract_and_unity_proof`: PROVEN |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `citygrind.decoration_prop_worldy.device_branch_and_identity_FINAL` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): CONFIRMS the iPad branch (byte_214FC8==0, flt_214FD4=-104.0) is PROVEN authoritative for the real shipped hardware (sub_175B88 is a genuine Info.plist/UIDevice.model iPad check, TRUE on real iPad; independently corroborated by the Extracted corpus itself being

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0x7EDA8  f0b5         PUSH            {R4-R7,LR}
0x7EDAA  03af         ADD             R7, SP, #0xC
0x7EDAC  4df8048d     PUSH.W          {R8}
0x7EDB0  4df28c01     MOVW            R1, #:lower16:(byte_21BE78 - 0x7EDEC)
0x7EDB4  0020         MOVS            R0, #0
0x7EDB6  c0f21901     MOVT            R1, #:upper16:(byte_21BE78 - 0x7EDEC)
0x7EDBA  46f2e612c0f21902 MOV             R2, #(byte_214FC8 - 0x7EDE2)
0x7EDC2  4df27e03c0f21903 MOV             R3, #(byte_21BE70 - 0x7EDF2)
0x7EDCA  46f26e19c0f21909 MOV             R9, #(flt_214FAC - 0x7EE3E)
0x7EDD2  4df27606c0f21906 MOV             R6, #(byte_21BE6C - 0x7EDF6)
0x7EDDA  46f2801c     MOVW            R12, #:lower16:(flt_214FCC - 0x7EE4C)
0x7EDDE  7a44         ADD             R2, PC; byte_214FC8
0x7EDE0  c0f2190c     MOVT            R12, #:upper16:(flt_214FCC - 0x7EE4C)
0x7EDE4  4df27c05     MOVW            R5, #:lower16:(byte_21BE74 - 0x7EDF8)
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
int sub_7EDA8()
{
  int result; // r0
  char *v1; // r3
  float *v2; // r1
  float *v3; // r4
  float *v4; // r2
  float v5; // s8
  float v6; // s0
  float v7; // s4

  result = 0;
  byte_21BE70 = 0;
  byte_21BE78 = 0;
  v1 = "ff&?";
  v2 = &flt_7EEB0;
  byte_21BE6C = 0;
  v3 = &flt_7EEB8;
  byte_21BE74 = 0;
  if ( !byte_214FC8 )
```

## References found in the code

Strings referenced: none

Globals referenced: `byte_214FB8`, `byte_214FBC`, `byte_214FC8`, `byte_21BE6C`, `byte_21BE70`, `byte_21BE74`, `byte_21BE78`, `byte_21C3C4`, `dword_214FB0`, `dword_214FB4`, `dword_214FC0`, `flt_214FAC`

Callers (with call sites): `0xD5BC0`@`0xD5C12`

Callees: none

## Unity translation

- No Unity code comment cites this address: **unmapped** (not proof the behaviour is absent from the port).

## Validation

- No test is individually linked to this function.

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
