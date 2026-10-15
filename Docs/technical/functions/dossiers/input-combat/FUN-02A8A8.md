# sub_2A8A8: a2b.weapon_effect_attachment_subsystem

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-02A8A8` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x2A8A8` |
| RVA (EA - 0x4000) | `0x268A8` |
| File offset | `0x268A8` |
| Size | 100596 bytes, 31033 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_2A8A8` (IDA automatic name) |
| Bytes SHA-256 | `c5e6bfe6831051c54d9b26767140f8fa043b2935ec5155f62c7d32d30a74cf29` |
| Subsystem | input-combat |
| Direct callers | 1 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `a2b.weapon_effect_attachment_subsystem`: PROVEN; `citygrind.building.house456_clone_house2`: PROVEN; `platform.asset_loading.native_loads_gameplay_assets_behind_the_loading_slide`: PROVEN_NATIVE_TRIGGER_LIKELY_SCOPE |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `a2b.weapon_effect_attachment_subsystem` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): sub_7D554 lazily one-time-initializes a pool of ~6 static attachment-template slots (guarded by byte_21C3C4). sub_7D81C is a runtime per-request switch-dispatcher (17 exhaustive callers: weapon/effect functions incl. sub_A1518, sub_E4BA0, sub_8161C, plus the l

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0x2A8A8  f0b5         PUSH            {R4-R7,LR}
0x2A8AA  03af         ADD             R7, SP, #0xC
0x2A8AC  2de9000d     PUSH.W          {R8,R10,R11}
0x2A8B0  adf14004     SUB.W           R4, SP, #0x40 ; '@'
0x2A8B4  24f00f04     BIC.W           R4, R4, #0xF
0x2A8B8  a546         MOV             SP, R4
0x2A8BA  04f9ed82     VST1.64         {D8-D11}, [R4@128]!
0x2A8BE  04f9efc2     VST1.64         {D12-D15}, [R4@128]
0x2A8C2  adf5435d     SUB.W           SP, SP, #0x30C0
0x2A8C6  88b0         SUB             SP, SP, #0x20; int
0x2A8C8  8246         MOV             R10, R0
0x2A8CA  41f6ec00c0f21d00 MOV             R0, #(off_1FC1C8 - 0x2A8DC)
0x2A8D2  0c46         MOV             R4, R1
0x2A8D4  41f25271     MOVW            R1, #:lower16:(___gxx_personality_sj0_ptr - 0x2A8E6)
```

**Around 0x3DDA2 (an address cited by a recovered rule)**

```
0x3DD9A  cbf8b080     STR.W           R8, [R11,#0xB0]
0x3DD9E  0668         LDR             R6, [R0]; unk_21BDA8
0x3DDA0  3046         MOV             R0, R6; int
0x3DDA2  3af0a1fd     BL              sub_788E8
0x3DDA6  48f62325     MOVW            R5, #:lower16:(aCom1 - 0x3DDBE); "com1"
0x3DDAA  0df5405b     ADD.W           R11, SP, #0x3120+var_120
0x3DDAE  c0f21905     MOVT            R5, #:upper16:(aCom1 - 0x3DDBE); "com1"
```

**Around 0x3DDF2 (an address cited by a recovered rule)**

```
0x3DDEA  cbf8b080     STR.W           R8, [R11,#0xB0]
0x3DDEE  0668         LDR             R6, [R0]; unk_21BDC0
0x3DDF0  3046         MOV             R0, R6; int
0x3DDF2  3af079fd     BL              sub_788E8
0x3DDF6  48f6c722     MOVW            R2, #:lower16:(aFangzi5 - 0x3DE08); "fangzi5"
0x3DDFA  0df5405b     ADD.W           R11, SP, #0x3120+var_120
0x3DDFE  c0f21902     MOVT            R2, #:upper16:(aFangzi5 - 0x3DE08); "fangzi5"
```

**Around 0x3DE38 (an address cited by a recovered rule)**

```
0x3DE30  cbf8b080     STR.W           R8, [R11,#0xB0]
0x3DE34  0668         LDR             R6, [R0]; unk_21BDD8
0x3DE36  3046         MOV             R0, R6; int
0x3DE38  3af056fd     BL              sub_788E8
0x3DE3C  48f68922     MOVW            R2, #:lower16:(aFangzi6 - 0x3DE4E); "fangzi6"
0x3DE40  0df54054     ADD.W           R4, SP, #0x3120+var_120
0x3DE44  c0f21902     MOVT            R2, #:upper16:(aFangzi6 - 0x3DE4E); "fangzi6"
```

## Decompiler excerpt

Hex-Rays produced no output for this function in the scripted export.

## References found in the code

Strings referenced: `OpenDoorAudio` (`0x1D49A7`), `CloseDoorAudio` (`0x1D49B5`), `Audio_itemAudio` (`0x1D49C7`), `xue2Frame` (`0x1D49FC`), `xue5Frame` (`0x1D4A06`), `xue4Frame` (`0x1D4A10`), `xue6Frame` (`0x1D4A1A`), `xue7Frame` (`0x1D4A24`)

Globals referenced: `byte_21B400`, `byte_21C3C4`, `dword_21B3E8`, `dword_21BDF4`, `dword_21BE00`, `dword_21BE0C`, `dword_21BE10`, `dword_21C6F8`, `dword_21C708`, `flt_21BDF0`

Callers (with call sites): `0x62A98`@`0x62AA6`

Callees: `0x77E30`, `0x78658`, `0x788E8`, `0x78BC8`, `0x78BE0`, `0x78D64`, `0x79064`, `0x7D81C`, `0x7EEC8`, `0x7F008`, `0x7F1E8`, `0xE8E48`, `0xE9E54`, `0x1706D4`, `0x17E390`, `0x17E760`

## Unity translation

- Code comments citing this address (9): `Runtime/CityGridController.cs:262`, `Runtime/CityGridController.cs:3077`, `Runtime/CityGridController.cs:8229`, `Runtime/Combat/DinoCapBloodBurst.cs:37`, `Runtime/Combat/DinoCapBloodBurst.cs:51`, `Runtime/CombatSandboxV2.cs:2833`, `Runtime/CombatSandboxV2.cs:2836`, `Runtime/MainMenuController.cs:470` ...

## Validation

- Tests/Task027N236CityGridRepro.cs
- Tests/DC1EightDefectRepro.cs

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
