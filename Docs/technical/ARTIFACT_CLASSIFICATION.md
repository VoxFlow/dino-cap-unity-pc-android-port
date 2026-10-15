# Artifact classification (publication class)

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../start/disclaimer.md).*

Technical analysis is not permission to redistribute original material. Everything in this folder is derived from the original binary and data. This page classifies it; **publication approval is a separate owner decision**
(the repository is currently private).

| Artifact | Class | Why |
|---|---|---|
| `binary/binaries.jsonl`, `ADDRESS_CONVENTIONS.md`, `COVERAGE.md` | REVIEW_REQUIRED | Hashes, layout and counts of the original binary |
| `functions/functions.jsonl`, `ADDRESS_INDEX.md`, `SUBSYSTEMS.md`, `by-subsystem/` | REVIEW_REQUIRED | Function addresses, sizes, byte hashes and names from the original |
| `functions/dossiers/` | REVIEW_REQUIRED | Instruction excerpts (bytes and disassembly) and pseudocode heads from the original |
| `classes/` | REVIEW_REQUIRED | Class names and virtual-table layouts from the original |
| `data/` (globals, strings) | REVIEW_REQUIRED | Strings and data names extracted from the original |
| `graphs/` | REVIEW_REQUIRED | Call relations from the original |
| `maps/ASSET_IDENTITY_MAP.md` | REVIEW_REQUIRED | Hashes and names of original assets |
| `maps/FIGURE_INDEX.md`, `../evidence/figures/` | REVIEW_REQUIRED | Original game art and captures |
| `maps/SOURCE_TO_UNITY_MAP.md`, `claims.jsonl`, `OPEN_QUESTIONS.md` | PUBLIC_CANDIDATE | Our own statements and mappings |
| `registry/` | REVIEW_REQUIRED (redacted derivative) | Descriptions of original behaviour; one developer account name redacted |
| `tools/` | PUBLIC_CANDIDATE | Our scripts (paths from environment variables) |
