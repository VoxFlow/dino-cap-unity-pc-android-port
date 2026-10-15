# sub_172C7C: gameobject.api.sub_172C78

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-172C7C` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x172C7C` |
| RVA (EA - 0x4000) | `0x16EC7C` |
| File offset | `0x16EC7C` |
| Size | 20 bytes, 7 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_172C7C` (IDA automatic name) |
| Bytes SHA-256 | `b3084086ef3d122eca011ad2561d3939836df48d6c2c082456167194636311f8` |
| Subsystem | animation-rendering |
| Direct callers | 25 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `gameobject.api.sub_172C78`: PROVEN; `gameobject.api.sub_172C7C`: PROVEN |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `gameobject.api.sub_172C78` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): sub_172C78(GameObject*) is the genuine 1-arg GetPosition accessor (returns pointer to GameObject+32 position pair). Directly consumed by sub_76A20 to obtain RootGameObject.Position for per-part transform composition.

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0x172C7C  d1e90023     LDRD.W          R2, R3, [R1]
0x172C80  c0e90a23     STRD.W          R2, R3, [R0,#0x28]
0x172C84  806c         LDR             R0, [R0,#0x48]
0x172C86  0028         CMP             R0, #0
0x172C88  18bf         IT NE
0x172C8A  18f0a3bc     BNE.W           sub_18B5D4
0x172C8E  7047         BX              LR
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
int __fastcall sub_172C7C(int a1, _QWORD *a2)
{
  int result; // r0

  *(_QWORD *)(a1 + 40) = *a2;
  result = *(_DWORD *)(a1 + 72);
  if ( result )
    return sub_18B5D4();
  return result;
}
```

## References found in the code

Strings referenced: none

Globals referenced: none among the named globals

Callers (with call sites): `0xDAA0`@`0xDE3A`, `0xDAA0`@`0xDFEC`, `0xDAA0`@`0xE4AC`, `0xEFB4`@`0xF04E`, `0xEFB4`@`0xF09E`, `0xEFB4`@`0xF11A`, `0x11124`@`0x11284`, `0x1141C`@`0x1165A`, `0x11980`@`0x11F0E`, `0x12264`@`0x1263A`, `0x6833C`@`0x685A0`, `0x686F4`@`0x68794`

Callees: none

## Unity translation

- No Unity code comment cites this address: **unmapped** (not proof the behaviour is absent from the port).

## Validation

- No test is individually linked to this function.

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
