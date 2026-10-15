# Technical index

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../start/disclaimer.md).*

Code-level evidence for the findings: which binary was analysed, where every function is, what the instructions are, and where the port's code cites it. Partial decompilation and port, not a full source recovery;
see [COVERAGE](binary/COVERAGE.md) for the counts.

| Folder | What is in it |
|---|---|
| [`binary/`](binary/ADDRESS_CONVENTIONS.md) | Identity of the analysed binary (`binaries.jsonl`), address conventions (EA, RVA, file offset, Thumb), [coverage](binary/COVERAGE.md) |
| [`functions/`](functions/ADDRESS_INDEX.md) | [Address index](functions/ADDRESS_INDEX.md) of the 195 core functions (194 cited by recovered rules plus the attack predicate `sub_FE50`), [subsystem pages](functions/SUBSYSTEMS.md), all 6,698 function records (`functions.jsonl`), one [dossier](functions/dossiers/pause-time/FUN-17C950.md) per core function (12 of them deep) |
| [`classes/`](classes/CLASSES.md) | C++ classes and their virtual tables (81 real tables; import-pointer cells excluded) with slot targets |
| [`data/`](data/GLOBALS.md) | [Named globals](data/GLOBALS.md), [notable strings](data/NOTABLE_STRINGS.md), all strings (`strings.jsonl`) |
| [`graphs/`](graphs/CALL_GRAPH.md) | Code-reference graph (39,528 edges: 38,806 encoded-direct, 722 IDA-resolved) and the core-function subgraph |
| [`maps/`](maps/SOURCE_TO_UNITY_MAP.md) | [Original to Unity](maps/SOURCE_TO_UNITY_MAP.md), [asset identity](maps/ASSET_IDENTITY_MAP.md), [figures](maps/FIGURE_INDEX.md) |
| [`registry/`](registry/README.md) | Published (redacted) copy of the recovered-rules registry |
| [`tools/`](tools/README.md) | The export scripts used to produce this data |
| [`ARTIFACT_CLASSIFICATION.md`](ARTIFACT_CLASSIFICATION.md) | Publication class of every artifact here (all original-derived: REVIEW_REQUIRED) |
| [`claims.jsonl`](claims.jsonl), [OPEN_QUESTIONS](OPEN_QUESTIONS.md) | One record per recovered rule; what is not known |

A function's subsystem folder is a heuristic from the rules that cite it, not exclusive ownership. Subsystem grouping of functions is a heuristic from the prefixes of the rules that cite each function; a few functions land in "other".
Instruction excerpts are exact; the notes beside them are interpretation. Names like `sub_76A20` are IDA's automatic names.
