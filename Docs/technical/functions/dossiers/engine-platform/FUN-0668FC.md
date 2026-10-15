# sub_668FC: port.policy.ranged_weapon_range_widescreen_keep_original

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-0668FC` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x668FC` |
| RVA (EA - 0x4000) | `0x628FC` |
| File offset | `0x628FC` |
| Size | 670 bytes, 237 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_668FC` (IDA automatic name) |
| Bytes SHA-256 | `524c7d038a8ae09c42724cf960d85088376c56b51b9b00b44916e236cd2dd8e6` |
| Subsystem | engine-platform |
| Direct callers | 2 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `port.policy.ranged_weapon_range_widescreen_keep_original`: PORT_POLICY_USER_DECISION |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `port.policy.ranged_weapon_range_widescreen_keep_original` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): Ranged weapon hit range is kept EXACTLY as shipped at every aspect ratio, including ultra-wide (e.g. 48:9). Shipped iPad weapon.prop ATK_DIS (world units, native 1024-wide view): A1-A4, B1-B4, C1-C4, D1-D4, F1, F3 = 1024; E1 = 220; E2 = 310; F2 = 310; G1-G4 = 

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0x668FC  f0b5         PUSH            {R4-R7,LR}
0x668FE  03af         ADD             R7, SP, #0xC
0x66900  2de9000d     PUSH.W          {R8,R10,R11}
0x66904  adf14004     SUB.W           R4, SP, #0x40 ; '@'
0x66908  24f00f04     BIC.W           R4, R4, #0xF
0x6690C  a546         MOV             SP, R4
0x6690E  04f9ed82     VST1.64         {D8-D11}, [R4@128]!
0x66912  04f9efc2     VST1.64         {D12-D15}, [R4@128]
0x66916  a4b0         SUB             SP, SP, #0x90
0x66918  0646         MOV             R6, R0
0x6691A  0c46         MOV             R4, R1
0x6691C  7068         LDR             R0, [R6,#4]
0x6691E  0696         STR             R6, [SP,#0xD0+var_B8]
0x66920  0cf1aaf9     BL              sub_172C78
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
void __fastcall sub_668FC(
        int a1,
        int a2,
        int a3,
        int a4,
        int a5,
        int a6,
        int a7,
        int a8,
        int a9,
        struct _Unwind_Exception *lpuexcpt,
        int a11,
        int a12,
        int a13,
        int a14,
        char a15,
        int a16,
        int a17,
        int a18,
        int a19,
```

## References found in the code

Strings referenced: none

Globals referenced: `dword_21BE00`, `dword_21BE0C`

Callers (with call sites): `0x66C74`@`0x66CAC`, `0x66DCC`@`0x67150`

Callees: `0xCA84`, `0x10554`, `0x77498`, `0xE885C`, `0x172C78`, `0x172D14`, `0x172D18`, `0x17C71C`, `0x1B7528`, `0x1B76A8`, `0x1B7778`, `0x1B7788`, `0x1B7798`

## Unity translation

- Code comments citing this address (6): `Runtime/Combat/MeleeContactCore.cs:7`, `Runtime/CombatSandboxV2.cs:1184`, `Runtime/CombatSandboxV2.cs:2649`, `Runtime/CombatSandboxV2.cs:2750`, `Runtime/CombatSandboxV2.cs:3342`, `Runtime/CombatSandboxV2.cs:4187`

## Validation

- No test is individually linked to this function.

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
