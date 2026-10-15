# sub_18B5D4: gameobject.api.sub_172C7C

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-18B5D4` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x18B5D4` |
| RVA (EA - 0x4000) | `0x1875D4` |
| File offset | `0x1875D4` |
| Size | 16 bytes, 5 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_18B5D4` (IDA automatic name) |
| Bytes SHA-256 | `7f2d4de50f70a315a17455e92228f072656ed76160f47049f198b8a4c43ee9db` |
| Subsystem | animation-rendering |
| Direct callers | 2 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `gameobject.api.sub_172C7C`: PROVEN |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `gameobject.api.sub_172C7C` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): sub_172C7C(GameObject*, const float2*) is GameObject::SetScale-by-pointer: unconditionally copies a caller-supplied float pair into GameObject+40/+44 and propagates to a renderer-dirty-flag listener via sub_18B5D4 if present. NOT a position accessor.

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0x18B5D4  d1e90023     LDRD.W          R2, R3, [R1]
0x18B5D8  0121         MOVS            R1, #1
0x18B5DA  c0e90723     STRD.W          R2, R3, [R0,#0x1C]
0x18B5DE  80f84010     STRB.W          R1, [R0,#0x40]
0x18B5E2  7047         BX              LR
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
int __fastcall sub_18B5D4(int result, _QWORD *a2)
{
  *(_QWORD *)(result + 28) = *a2;
  *(_BYTE *)(result + 64) = 1;
  return result;
}
```

## References found in the code

Strings referenced: none

Globals referenced: none among the named globals

Callers (with call sites): `0x172C7C`@`0x172C8A`, `0x172C90`@`0x172CB8`

Callees: none

## Unity translation

- No Unity code comment cites this address: **unmapped** (not proof the behaviour is absent from the port).

## Validation

- No test is individually linked to this function.

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
