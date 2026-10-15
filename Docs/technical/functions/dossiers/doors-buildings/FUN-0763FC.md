# sub_763FC: citygrind.player.house_entry_exit.no_dedicated_animation

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-0763FC` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x763FC` |
| RVA (EA - 0x4000) | `0x723FC` |
| File offset | `0x723FC` |
| Size | 192 bytes, 68 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_763FC` (IDA automatic name) |
| Bytes SHA-256 | `66bbd8df5ad9ca8f962bf2813491f1c0d55df4e7ed605b1e061afed8a77fb23d` |
| Subsystem | doors-buildings |
| Direct callers | 0 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `citygrind.player.house_entry_exit.no_dedicated_animation`: SUPERSEDED by citygrind.player.house_family_full_c2b_merge (sub_68CD8 entry plays the fami |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `citygrind.player.house_entry_exit.no_dedicated_animation` (HISTORICAL: that rule is superseded or falsified, so this is not a current-behaviour statement): Native's own player-freeze routine (sub_68CD8), triggered on building entry/exit, has NO dedicated jump-in/jump-out clip, motion, or audio hook. On entry it: sets the position-freeze state (already ported); tags an inert CAObjSpriteCallBack child object ('hous

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0x763FC  f0b5         PUSH            {R4-R7,LR}
0x763FE  03af         ADD             R7, SP, #0xC
0x76400  2de9000d     PUSH.W          {R8,R10,R11}
0x76404  adf14004     SUB.W           R4, SP, #0x40 ; '@'
0x76408  24f00f04     BIC.W           R4, R4, #0xF
0x7640C  a546         MOV             SP, R4
0x7640E  04f9ed82     VST1.64         {D8-D11}, [R4@128]!
0x76412  04f9efc2     VST1.64         {D12-D15}, [R4@128]
0x76416  90b0         SUB             SP, SP, #0x40
0x76418  0c20         MOVS            R0, #0xC; unsigned int
0x7641A  41f126e9     BLX             __Znwm; operator new(ulong)
0x7641E  45f60a42     MOVW            R2, #:lower16:(___gxx_personality_sj0_ptr - 0x7642E)
0x76422  2649         LDR             R1, =(unk_1D089C - 0x76430)
0x76424  c0f21802     MOVT            R2, #:upper16:(___gxx_personality_sj0_ptr - 0x7642E)
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
_DWORD *sub_763FC()
{
  _DWORD *v1; // [sp+8h] [bp-90h]

  v1 = (_DWORD *)operator new(0xCu);
  sub_188BB4(v1);
  *v1 = &off_214F88;
  v1[2] = (char *)&std::string::_Rep::_S_empty_rep_storage + 12;
  return v1;
}
```

## References found in the code

Strings referenced: none

Globals referenced: none among the named globals

Callers (with call sites): none by direct reference

Callees: `0x188BB4`, `0x1B7648`, `0x1B7668`, `0x1B7778`, `0x1B7788`, `0x1B7798`

## Unity translation

- No Unity code comment cites this address: **unmapped** (not proof the behaviour is absent from the port).

## Validation

- No test is individually linked to this function.

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
