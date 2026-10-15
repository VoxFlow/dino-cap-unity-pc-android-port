# Call graph

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../start/disclaimer.md).*

- `callgraph_all.jsonl`: **39528 code-reference edges** between all functions, one `[caller, call-site, callee, kind]` per line. Kind `d` (38806): the instruction's encoded target is the destination. Kind `r` (722): IDA resolved the destination through an indirect jump, an Objective-C message send or a thunk, so the encoded target is something else (examples: a `BLX` to `objc_msgSend` resolved to a method implementation; an `LDR pc,[r12]` stub resolved to its target). Treat `r` edges as IDA-inferred, not as proof of a direct call.
- `callgraph_core_edges.json`: the 1537 edges among the 195 core functions (same extraction, no kind field).

Limits: calls through virtual tables and function pointers that IDA did not resolve are **not** edges (the game dispatches player, dinosaur and scene handlers that way; see [classes](../classes/CLASSES.md)), so a function with no callers here can still run. A missing edge is not proof of absence. Counts below are edges (call sites), with the number of distinct calling functions in brackets.

Most-referenced functions:

- `0x1B72E8` (_objc_msgSend): 7388 edges from 1125 distinct functions
- `0x1B7478` (__ZNSs4_Rep10_M_destroyERKSaIcE): 4896 edges from 294 distinct functions
- `0x1B7508` (__ZNSsC1EPKcRKSaIcE): 3103 edges from 271 distinct functions
- `0x17E390` (sub_17E390): 822 edges from 12 distinct functions
- `0x1B7648` (__ZdlPv): 811 edges from 458 distinct functions
- `0x1B7798` (__Unwind_SjLj_Unregister): 749 edges from 748 distinct functions
- `0x1B7778` (__Unwind_SjLj_Register): 748 edges from 748 distinct functions
- `0x1B7788` (__Unwind_SjLj_Resume): 748 edges from 748 distinct functions
- `0x1B77A8` (___assert_rtn): 422 edges from 221 distinct functions
- `0x17C968` (sub_17C968): 391 edges from 126 distinct functions
- `0x1B76A8` (___dynamic_cast): 308 edges from 165 distinct functions
- `0x172D1C` (sub_172D1C): 304 edges from 44 distinct functions ([dossier](../functions/dossiers/doors-buildings/FUN-172D1C.md))
- `0x1B4C58` (_NSLog): 293 edges from 172 distinct functions
- `0x1B7FE8` (_sprintf): 247 edges from 29 distinct functions
- `0x183D54` (sub_183D54): 230 edges from 77 distinct functions

Functions with the most outgoing edges:

- `0x2A8A8` (sub_2A8A8): 1998 edges
- `0xA1518` (sub_A1518): 1519 edges
- `0x8925C` (sub_8925C): 1061 edges
- `0x1BDC0` (sub_1BDC0): 671 edges
- `0x4319C` (sub_4319C): 432 edges
- `0xDBC40` (sub_DBC40): 388 edges
- `0x6F748` (sub_6F748): 379 edges
- `0xE4BA0` (sub_E4BA0): 379 edges
- `0xB30B2` (sub_B30B2): 363 edges
- `0x7FEF4` (InitFunc_21): 278 edges
