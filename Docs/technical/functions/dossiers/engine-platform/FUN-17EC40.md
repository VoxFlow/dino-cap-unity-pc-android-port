# sub_17EC40: platform.asset_loading.native_loads_gameplay_assets_behind_the_loading_slide

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-17EC40` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x17EC40` |
| RVA (EA - 0x4000) | `0x17AC40` |
| File offset | `0x17AC40` |
| Size | 580 bytes, 217 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_17EC40` (IDA automatic name) |
| Bytes SHA-256 | `7fba01ca90f273571d6436d7313f245c6db55f869f7aa9e6eefff951bc312e31` |
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
0x17EC40  f0b5         PUSH            {R4-R7,LR}
0x17EC42  03af         ADD             R7, SP, #0xC
0x17EC44  2de9000d     PUSH.W          {R8,R10,R11}
0x17EC48  adf14004     SUB.W           R4, SP, #0x40 ; '@'
0x17EC4C  24f00f04     BIC.W           R4, R4, #0xF
0x17EC50  a546         MOV             SP, R4
0x17EC52  04f9ed82     VST1.64         {D8-D11}, [R4@128]!
0x17EC56  04f9efc2     VST1.64         {D12-D15}, [R4@128]
0x17EC5A  a0b0         SUB             SP, SP, #0x80
0x17EC5C  1d46         MOV             R5, R3
0x17EC5E  0992         STR             R2, [SP,#0xC0+textures]; int
0x17EC60  0a90         STR             R0, [SP,#0xC0+var_98]; int
0x17EC62  0c46         MOV             R4, R1
0x17EC64  0b95         STR             R5, [SP,#0xC0+var_94]; int
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
int __fastcall sub_17EC40(
        int a1,
        __int64 *a2,
        int a3,
        _DWORD *a4,
        GLsizei *a5,
        GLsizei *a6,
        GLsizei *a7,
        int a8,
        int a9,
        int a10,
        int a11,
        void *a12,
        int a13,
        int a14,
        int a15,
        void *a16,
        int a17,
        int a18,
        int a19,
```

## References found in the code

Strings referenced: none

Globals referenced: none among the named globals

Callers (with call sites): `0x17D540`@`0x17D75C`

Callees: `0x1748F0`, `0x17F4BC`, `0x1B4E48`, `0x1B4EE8`, `0x1B4F18`, `0x1B4F68`, `0x1B4FE8`, `0x1B5048`, `0x1B5068`, `0x1B7648`, `0x1B7778`, `0x1B7788`, `0x1B7798`

## Unity translation

- No Unity code comment cites this address: **unmapped** (not proof the behaviour is absent from the port).

## Validation

- No test is individually linked to this function.

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
