# sub_63BB0: citygrind.pause_menu.absolute_layout

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../../../start/disclaimer.md).*

| Field | Value |
|---|---|
| Function ID | `FUN-063BB0` |
| Binary | `BIN-DC1-IOS-1.9.8-ARMV7` |
| Static address (EA) | `0x63BB0` |
| RVA (EA - 0x4000) | `0x5FBB0` |
| File offset | `0x5FBB0` |
| Size | 550 bytes, 201 instructions |
| Instruction set | THUMB (calling convention: ARM AAPCS as used on iOS; for C++ methods `this` is in R0) |
| Name | `sub_63BB0` (IDA automatic name) |
| Bytes SHA-256 | `d3421b5ce4e96bcca4ea34e115a3bfed7137c44fa41d2f6ce91236f9d0b15211` |
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
0x63BB0  f0b5         PUSH            {R4-R7,LR}
0x63BB2  03af         ADD             R7, SP, #0xC
0x63BB4  2de9000d     PUSH.W          {R8,R10,R11}
0x63BB8  adf14004     SUB.W           R4, SP, #0x40 ; '@'
0x63BBC  24f00f04     BIC.W           R4, R4, #0xF
0x63BC0  a546         MOV             SP, R4
0x63BC2  04f9ed82     VST1.64         {D8-D11}, [R4@128]!
0x63BC6  04f9efc2     VST1.64         {D12-D15}, [R4@128]
0x63BCA  90b0         SUB             SP, SP, #0x40
0x63BCC  0190         STR             R0, [SP,#0x80+var_7C]
0x63BCE  0198         LDR             R0, [SP,#0x80+var_7C]
0x63BD0  20f14ef8     BL              sub_183C70
0x63BD4  48f25441     MOVW            R1, #:lower16:(___gxx_personality_sj0_ptr - 0x63BE4)
0x63BD8  7f48         LDR             R0, =(unk_1D01D0 - 0x63BE6)
```

## Decompiler excerpt (not authoritative)

First lines of the Hex-Rays output; the instructions win where they differ.

```c
_DWORD *__fastcall sub_63BB0(_DWORD *a1)
{
  int i; // r0
  _QWORD *v2; // r1

  sub_183C70(a1);
  sub_18315C(a1 + 16);
  *a1 = &off_1FDBD8;
  a1[16] = off_1FDC60;
  a1[17] = 0;
  a1[18] = 0;
  a1[19] = 0;
  for ( i = 20; i != 48; i += 4 )
  {
    v2 = &a1[i];
    *v2 = 0;
    v2[1] = 0;
  }
  a1[20] = 1135476736;
  a1[21] = 1109393408;
```

## References found in the code

Strings referenced: none

Globals referenced: none among the named globals

Callers (with call sites): `0x63BAC`@`0x63BAC`

Callees: `0xCA84`, `0x18315C`, `0x183174`, `0x183C70`, `0x183D38`, `0x194E24`, `0x1B7648`, `0x1B7778`, `0x1B7788`, `0x1B7798`

## Unity translation

- Code comments citing this address (2): `Runtime/CityGridController.cs:8401`, `Runtime/CityGridController.cs:8440`

## Validation

- Tests/DC1BloodBurstPauseRepro.cs
- Tests/DC1PausePointerRepro.cs

## Open / limits

Generated dossier: the summary is the first citing rule's text; read the rules for the full contract.
