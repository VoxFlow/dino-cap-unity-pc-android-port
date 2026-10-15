# sub_17AE4: citygrind.melee_ranged.arbitration_no_queue_melee_wins

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-017AE4` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x17AE4` |
| RVA (EA - 0x4000) | `0x13AE4` |
| File offset | `0x13AE4` |
| Size | 1352 bytes, 407 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_17AE4` (IDA automatic name) |
| Bytes SHA-256 | `f5e810dd35157d065f22f5cb819241c17cd0ffea4e6e70f21b8ee26c2025129e` |
| Subsystem | input-combat |
| Direct callers | 0 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `citygrind.melee_ranged.arbitration_no_queue_melee_wins`: PROVEN_NATIVE_ARM / IMPLEMENTED (City Grind) / AUTOMATED_FOCUSED_PASS / DEVICE_AB_PENDING |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `citygrind.melee_ranged.arbitration_no_queue_melee_wins` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): Touch begin (sub_56E94: melee zone C 0x57136/0x57166, fire zone D 0x571C6/0x571F6) sets byte_21BE74 (melee) / byte_21BE78 (fire) and immediately calls sub_666BC / sub_667B0; touch end clears the flag, so each flag means 'control still held'. sub_666BC (melee e

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0x17AE4  f0b5         PUSH            {R4-R7,LR}; int
0x17AE6  adf11004     SUB.W           R4, SP, #0x10
0x17AEA  03af         ADD             R7, SP, #0xC
0x17AEC  24f00f04     BIC.W           R4, R4, #0xF
0x17AF0  a546         MOV             SP, R4; int
0x17AF2  04f9ef8a     VST1.64         {D8-D9}, [R4@128]
0x17AF6  0546         MOV             R5, R0
0x17AF8  43ec303b     VMOV            D16, R3, R3
0x17AFC  0c46         MOV             R4, R1
0x17AFE  97ed020a     VLDR            S0, [R7,#-0xC+arg_0]
0x17B02  d5f8b800     LDR.W           R0, [R5,#0xB8]
0x17B06  0428         CMP             R0, #4
0x17B08  40f08982     BNE.W           loc_1801E
0x17B0C  bbff0086     VCVT.F32.S32    D8, D0
```

## Decompiler excerpt

Hex-Rays produced no output for this function in the scripted export.

## References found in the code

Strings referenced: `Audio_Button1Audio` (`0x1D4EF3`)

Globals referenced: `byte_21BE6C`, `byte_21BE70`, `byte_21BE74`, `byte_21BE78`, `dword_214F10`, `dword_214F14`, `dword_214F18`, `dword_214F1C`, `dword_214F20`, `flt_219DB4`, `flt_219DB8`, `flt_219DBC`

Callers (with call sites): none by direct reference

Callees: `0x64FA4`, `0x666BC`, `0x667B0`, `0x68080`, `0x7EB04`, `0x172D18`, `0x17C950`, `0x17C968`, `0x1B4718`, `0x1B76A8`

## Unity translation

- No Unity code comment cites this address: **unmapped** (not proof the behaviour is absent from the port).

## Validation

- Tests/Task027N236CityGridRepro.cs
- Tests/DC1EightDefectRepro.cs

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
