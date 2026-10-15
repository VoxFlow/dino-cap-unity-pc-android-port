# sub_17DD28: platform.asset_loading.native_loads_gameplay_assets_behind_the_loading_slide

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-17DD28` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x17DD28` |
| RVA (EA - 0x4000) | `0x179D28` |
| File offset | `0x179D28` |
| Size | 294 bytes, 85 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_17DD28` (IDA automatic name) |
| Bytes SHA-256 | `4baf4e668d237d903e578b6a237b40125fbed7d3c4f86667448f45273ae8ae76` |
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
0x17DD28  f0b5         PUSH            {R4-R7,LR}
0x17DD2A  03af         ADD             R7, SP, #0xC
0x17DD2C  2de9000d     PUSH.W          {R8,R10,R11}
0x17DD30  86b0         SUB             SP, SP, #0x18
0x17DD32  0546         MOV             R5, R0
0x17DD34  b868         LDR             R0, [R7,#-0x18+arg_0]
0x17DD36  9a46         MOV             R10, R3
0x17DD38  8b46         MOV             R11, R1
0x17DD3A  0592         STR             R2, [SP,#0x18+width]
0x17DD3C  0628         CMP             R0, #6; switch 7 cases
0x17DD3E  00f27380     BHI.W           def_17DD4A; jumptable 0017DD4A default case
0x17DD42  41f60814     MOVW            R4, #0x1908
0x17DD46  48f23308     MOVW            R8, #0x8033
0x17DD4A  dfe800f0     TBB.W           [PC,R0]; switch jump
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
int __fastcall sub_17DD28(int a1, const GLvoid **a2, GLsizei a3, GLsizei a4, int a5, GLuint *a6)
{
  GLenum v9; // r4
  GLenum v10; // r8

  v9 = 6408;
  v10 = 32819;
  switch ( a5 )
  {
    case 0:
      break;
    case 1:
      v9 = 6408;
      v10 = 32820;
      break;
    case 2:
      v9 = 6408;
      v10 = 5121;
      break;
    case 3:
```

## References found in the code

Strings referenced: `false` (`0x1D830C`), `/Users/<original-developer>/Desktop/iPhone App Build/power2D/` (`0x1E604B`), `LoadFromRaw` (`0x1E6481`)

Globals referenced: none among the named globals

Callers (with call sites): `0x17EB54`@`0x17EB7A`

Callees: `0x1B4E48`, `0x1B4EE8`, `0x1B4F18`, `0x1B4F68`, `0x1B4FE8`, `0x1B5048`, `0x1B5068`, `0x1B77A8`

## Unity translation

- No Unity code comment cites this address: **unmapped** (not proof the behaviour is absent from the port).

## Validation

- No test is individually linked to this function.

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
