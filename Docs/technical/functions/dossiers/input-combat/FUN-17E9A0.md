# sub_17E9A0: a2b.weapon_effect_attachment_subsystem

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-17E9A0` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x17E9A0` |
| RVA (EA - 0x4000) | `0x17A9A0` |
| File offset | `0x17A9A0` |
| Size | 32 bytes, 12 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_17E9A0` (IDA automatic name) |
| Bytes SHA-256 | `4757f5d69143168c2c6937fe49de557c19c3eddb37c958c9c9153443f0a17f9e` |
| Subsystem | input-combat |
| Direct callers | 3 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `a2b.weapon_effect_attachment_subsystem`: PROVEN |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `a2b.weapon_effect_attachment_subsystem` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): sub_7D554 lazily one-time-initializes a pool of ~6 static attachment-template slots (guarded by byte_21C3C4). sub_7D81C is a runtime per-request switch-dispatcher (17 exhaustive callers: weapon/effect functions incl. sub_A1518, sub_E4BA0, sub_8161C, plus the l

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0x17E9A0  90b5         PUSH            {R4,R7,LR}
0x17E9A2  0446         MOV             R4, R0
0x17E9A4  04f1fc00     ADD.W           R0, R4, #0xFC; int
0x17E9A8  01af         ADD             R7, SP, #4
0x17E9AA  38f096e9     BLX             j___ZNSt8_Rb_treeISsSt4pairIKSsiESt10_Select1stIS2_ESt4lessISsESaIS2_EE4findERS
0x17E9AE  04f58072     ADD.W           R2, R4, #0x100
0x17E9B2  4ff0ff31     MOV.W           R1, #0xFFFFFFFF
0x17E9B6  9042         CMP             R0, R2
0x17E9B8  18bf         IT NE
0x17E9BA  4169         LDRNE           R1, [R0,#0x14]
0x17E9BC  0846         MOV             R0, R1
0x17E9BE  90bd         POP             {R4,R7,PC}
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
int __fastcall sub_17E9A0(int a1, std::string *a2)
{
  int v3; // r0
  int v4; // r1

  v3 = std::_Rb_tree<std::string,std::pair<std::string const,int>,std::_Select1st<std::pair<std::string const,int>>,std::less<std::string>,s
         a1 + 252,
         a2);
  v4 = -1;
  if ( v3 != a1 + 256 )
    return *(_DWORD *)(v3 + 20);
  return v4;
}
```

## References found in the code

Strings referenced: none

Globals referenced: none among the named globals

Callers (with call sites): `0x76A20`@`0x76FDE`, `0x78D64`@`0x78E56`, `0x174728`@`0x17474A`

Callees: `0x1B6CD8`

## Unity translation

- No Unity code comment cites this address: **unmapped** (not proof the behaviour is absent from the port).

## Validation

- Tests/Task027N236CityGridRepro.cs
- Tests/DC1EightDefectRepro.cs

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
