# Coverage

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../start/disclaimer.md).*

What the technical layer covers, counted from the files (not estimated):

| Measure | Count |
|---|---:|
| Functions in the database (identity record each) | 6698 |
| Core functions (cited by a recovered rule) | 195 |
| Dossiers: deep (hand-written notes) | 12 |
| Dossiers: generated | 183 |
| Core functions with a Unity mapping (hand or code comment) | 127 |
| Core functions with no Unity mapping | 68 |
| Core functions with decompiler output in the export | 178 |
| Virtual tables recorded | 81 |
| Named globals resolved | 38 |
| Strings indexed | 6879 |
| Code-reference edges (all; 38,806 encoded-direct, 722 IDA-resolved) | 39528 |
| Code-reference edges among core functions (same extraction; resolution limits inherited) | 1537 |
| Recovered rules (claims) | 227 |
| Deep dossiers that list test files or classes (not per-function assertions) | 12 |

Not covered: 195 core dossiers exist (12 curated/deep, 183 generated rule/export summaries); the remaining 6503 functions have identity records only; runtime (slid) addresses; complete derivations of every rule; function-level validation for most core functions. This is a partial decompilation and port, not a full source recovery.
