# sub_63BAC: citygrind.pause_menu.absolute_layout

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-063BAC` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x63BAC` |
| RVA (EA - 0x4000) | `0x5FBAC` |
| File offset | `0x5FBAC` |
| Size | 4 bytes, 1 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_63BAC` (IDA automatic name) |
| Bytes SHA-256 | `210b66c4b1aac7d2762f66edf292587c5a58280d04096914ac4b47c54747db1a` |
| Subsystem | pause-time |
| Direct callers | 1 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `citygrind.pause_menu.absolute_layout`: PROVEN |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `citygrind.pause_menu.absolute_layout` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): DinoCap::pauseCtrliPad's own base constructor (sub_63BB0) hardcodes a literal 7x(x,y,w,h) float array (object offset bytes 80-191) giving the EXACT logical-screen-space (1024x768) absolute rect for each of its 7 UIMoveCtrl children, constructed in this fixed o

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0x63BAC  00f000b8     B.W             sub_63BB0
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
// attributes: thunk
int __fastcall sub_63BAC(int a1)
{
  return sub_63BB0(a1);
}
```

## References found in the code

Strings referenced: none

Globals referenced: none among the named globals

Callers (with call sites): `0x19760`@`0x1981C`

Callees: none

## Unity translation

- No Unity code comment cites this address: **unmapped** (not proof the behaviour is absent from the port).

## Validation

- Tests/DC1BloodBurstPauseRepro.cs
- Tests/DC1PausePointerRepro.cs

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
