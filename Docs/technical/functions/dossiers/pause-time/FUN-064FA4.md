# sub_64FA4: citygrind.pause.backdrop_alpha_and_bgm_ownership

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-064FA4` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x64FA4` |
| RVA (EA - 0x4000) | `0x60FA4` |
| File offset | `0x60FA4` |
| Size | 890 bytes, 308 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_64FA4` (IDA automatic name) |
| Bytes SHA-256 | `4a1a54e325a7abc084d6bf6d8db067b627babdb52b383fd154ef4dd97df2aed7` |
| Subsystem | pause-time |
| Direct callers | 4 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `citygrind.pause.backdrop_alpha_and_bgm_ownership`: PROVEN; `citygrind.pause_button.fires_on_touch_begin_in_native_zone`: PROVEN_NATIVE_ARM / IMPLEMENTED / AUTOMATED_FOCUSED_PASS / DEVICE_AB_PENDING; `citygrind.pause_menu.backdrop_alpha_and_bgm_ownership`: PROVEN |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `citygrind.pause.backdrop_alpha_and_bgm_ownership` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): sub_64694 (pause-menu-contents constructor) sets its whiteFrame backdrop tint via sub_183D94(obj,&v56) with v56=-1073741824 (0xC0000000 ARGB) - alpha byte 0xC0=192, giving a PROVEN alpha fraction of 192/255 (~0.753), replacing a disclosed 0.6 placeholder. Nati

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0x64FA4  f0b5         PUSH            {R4-R7,LR}
0x64FA6  03af         ADD             R7, SP, #0xC
0x64FA8  2de9000d     PUSH.W          {R8,R10,R11}
0x64FAC  adf14004     SUB.W           R4, SP, #0x40 ; '@'
0x64FB0  24f00f04     BIC.W           R4, R4, #0xF
0x64FB4  a546         MOV             SP, R4
0x64FB6  04f9ed82     VST1.64         {D8-D11}, [R4@128]!
0x64FBA  04f9efc2     VST1.64         {D12-D15}, [R4@128]
0x64FBE  b4b0         SUB             SP, SP, #0xD0
0x64FC0  c0ef5000     VMOV.I32        Q8, #0
0x64FC4  1eaa         ADD             R2, SP, #0x110+var_98
0x64FC6  0a90         STR             R0, [SP,#0x110+var_E8]
0x64FC8  47f25a01c0f21901 MOV             R1, #(___gxx_personality_sj0_ptr - 0x64FDE)
0x64FD0  4ff0ff34     MOV.W           R4, #0xFFFFFFFF
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
void __fastcall sub_64FA4(
        _DWORD *a1,
        int a2,
        int a3,
        int a4,
        int a5,
        int a6,
        int a7,
        int a8,
        int a9,
        int a10,
        int a11,
        int a12,
        int a13,
        int a14,
        int a15,
        int a16,
        int a17,
        int a18,
        int a19,
```

## References found in the code

Strings referenced: `text_offFrame` (`0x1D5415`), `text_onFrame` (`0x1D5423`)

Globals referenced: `byte_214FB8`, `byte_214FBC`, `flt_65324`, `flt_65328`, `flt_6532C`, `flt_65330`, `flt_65334`, `flt_65338`

Callers (with call sites): `0x17AE4`@`0x17F10`, `0x19340`@`0x19376`, `0x56E94`@`0x572C4`, `0x5931C`@`0x5935A`

Callees: `0xDA860`, `0xDA87C`, `0x183D54`, `0x1B7478`, `0x1B7508`, `0x1B7778`, `0x1B7788`, `0x1B7798`

## Unity translation

- Code comments citing this address (2): `Runtime/CityGridController.cs:1768`, `Runtime/CityGridController.cs:7904`

## Validation

- Tests/DC1BloodBurstPauseRepro.cs
- Tests/DC1PausePointerRepro.cs

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
