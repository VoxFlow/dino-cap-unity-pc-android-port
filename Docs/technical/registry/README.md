# Source-truth registry (published copy)

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../start/disclaimer.md).*

| File | What it is |
|---|---|
| `source_contract_registry.json` | Every recovered rule ("contract") with a confidence status, the proposition and its evidence notes |
| `disarmed_heuristics.json` | Values that were rejected as guesses or calibrations |
| `asset_identity_map.json` | Hand-recorded identities between original assets and Unity assets (12 entries; the mechanical byte-identity table is in `../maps/ASSET_IDENTITY_MAP.md`) |
| `source_contract_schema.json` | The record format |

This is a copy of the working files with one change: an original developer's account name inside a string embedded in the original binary was replaced by `<original-developer>`.
Many entries cite report folders (`Dev/Reports/...`) from the private working archive; those reports are not part of this repository, so those pointers do not resolve here. Statuses use many wordings;
`../claims.jsonl` normalises them to PROVEN / LIKELY / POSSIBLE / UNRESOLVED / FALSIFIED / SUPERSEDED / NOT_PURSUED / OPEN. Names like `sub_76A20` are IDA's automatic names.

In `../claims.jsonl`, `status_text` is a preview truncated at 120 characters (13 records are cut); the registry's own `status` is authoritative. A locator `functions/functions.jsonl#id=FUN-XXXXXX` means: the record in `functions.jsonl` whose `id` field is that value (a JSONL record id, not an HTML anchor).
