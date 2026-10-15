# sub_172D1C: gameobject.api.sub_172D1C

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-172D1C` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x172D1C` |
| RVA (EA - 0x4000) | `0x16ED1C` |
| File offset | `0x16ED1C` |
| Size | 50 bytes, 15 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_172D1C` (IDA automatic name) |
| Bytes SHA-256 | `98b6963f410c911c30f72728eddc987267aa842d7578bd8ab8c19bb2cae5fa79` |
| Subsystem | doors-buildings |
| Direct callers | 304 code references (virtual-table calls not counted) |
| Recovered rules citing it (with the registry status of each) | `citygrind.building.draw_key_zero_from_base_ctor`: FALSIFIED; `gameobject.api.sub_172D1C`: PROVEN |
| Confidence of this dossier | generated summary; see the status of each citing rule below |
| Depth | generated from the database export; interpretation limited to the recovered rules |

## What it does

From the citing rule `gameobject.api.sub_172D1C` (a rule can be proven for the mechanism while parts of it stay LIKELY: read its status above): sub_172D1C(GameObject*) is GameObject::GetSprite() - asserts m_sprite != NULL and returns GameObject+52. Confirmed via an embedded assert source path literal: /Users/<original-developer>/Desktop/iPhone App Build/power2D/power2D/trunk/src/src/Triniti/Engine/Gam

## Native instructions (excerpts)

Exact bytes, in IDA 9.2's rendering (another disassembler may spell aliases and folded immediates differently). The entry, and windows around addresses that recovered rules cite. Interpretation is in the rules, not here.

**Entry (first instructions)**

```
0x172D1C  80b5         PUSH            {R7,LR}
0x172D1E  406b         LDR             R0, [R0,#0x34]
0x172D20  6f46         MOV             R7, SP
0x172D22  0028         CMP             R0, #0
0x172D24  18bf         IT NE
0x172D26  80bd         POPNE           {R7,PC}
0x172D28  42f6ae20     MOVW            R0, #:lower16:(aSprite - 0x172D48); "Sprite"
0x172D2C  40f2d512     MOVW            R2, #0x1D5; int
0x172D30  c0f20700     MOVT            R0, #:upper16:(aSprite - 0x172D48); "Sprite"
0x172D34  42f6e411c0f20701 MOV             R1, #(aUsersOriginalDeveloper_32 - 0x172D4A); "/Users/<original-developer>/Desktop/iPhone App 
0x172D3C  42f6b123c0f20703 MOV             R3, #(aMSpriteNull - 0x172D4C); "m_sprite != NULL"
0x172D44  7844         ADD             R0, PC; "Sprite"
0x172D46  7944         ADD             R1, PC; "/Users/<original-developer>/Desktop/iPhone App Bui"...
0x172D48  7b44         ADD             R3, PC; "m_sprite != NULL"
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
int __fastcall sub_172D1C(int a1)
{
  int result; // r0

  result = *(_DWORD *)(a1 + 52);
  if ( !result )
    __assert_rtn(
      "Sprite",
      "/Users/<original-developer>/Desktop/iPhone App Build/power2D/power2D/trunk/src/src/Triniti/Engine/GameObject.cpp",
      469,
      "m_sprite != NULL");
  return result;
}
```

## References found in the code

Strings referenced: `/Users/<original-developer>/Desktop/iPhone App Build/power2D/` (`0x1E572E`), `Sprite` (`0x1E57F6`), `m_sprite != NULL` (`0x1E57FD`)

Globals referenced: none among the named globals

Callers (with call sites): `0xD5B0`@`0xD608`, `0xD5B0`@`0xD638`, `0xDAA0`@`0xDBBE`, `0xDAA0`@`0xDCBE`, `0xDAA0`@`0xDDDE`, `0xDAA0`@`0xDE58`, `0xDAA0`@`0xDEE4`, `0xDAA0`@`0xDF6E`, `0xDAA0`@`0xE178`, `0xDAA0`@`0xE198`, `0xDAA0`@`0xE1BC`, `0xDAA0`@`0xE216`

Callees: `0x1B77A8`

## Unity translation

- No Unity code comment cites this address: **unmapped** (not proof the behaviour is absent from the port).

## Validation

- No test is individually linked to this function.

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
