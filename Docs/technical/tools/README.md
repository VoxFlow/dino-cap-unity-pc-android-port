# Export tools

> *AI-reviewed content: may change at any time and should not be fully trusted. See the [disclaimer](../../start/disclaimer.md).*

These scripts produced the data in this folder from the IDA database. They are the reproduction record, not game code. They need IDA Pro 9.2 with `idalib` and a **disposable copy** of the database. They call
`open_database(path, False)` (auto-analysis disabled) and `close_database(False)` (no save). That is a safeguard, not an enforced read-only mode: do not point `DC1_DB_COPY` at the only copy, and keep the copy and its SHA-256 as the snapshot the data is tied to.
`IDADIR` defaults to the standard IDA 9.2 install folder of the machine that ran them; set it for yours.

Environment variables: `IDADIR` (IDA install folder), `DC1_DB_COPY` (path of the database copy), `DC1_OUT` (output folder), `DC1_REGISTRY` (path of `registry/source_contract_registry.json`), `DC1_REG_FUNCS` (output of `make_reg_funcs.py`); `classify_names.py` also needs `DC1_MACHO` (the analysed binary), `DC1_FUNCTIONS_IN` and `DC1_FUNCTIONS_OUT`.

| Script | Produces |
|---|---|
| `make_reg_funcs.py` | list of core function addresses cited by the rules |
| `export_functions.py` | the raw function records (name provenance is assigned later by `classify_names.py`; the exporter's own label is not reliable), the core call edges and the first dossier excerpts |
| `classify_names.py` | `functions/functions.jsonl`: name provenance checked against the Mach-O symbol table |
| `export_data.py` | strings, globals, the full call graph (edge kinds `d`/`r`), per-function strings/globals/callers/pseudocode |
| `export_core_disassembly.py` | disassembly of every core function (input to the dossiers) |
| `export_vtables.py` | `classes/classes.jsonl` |

The markdown pages are generated from these exports and the registry by further scripts that are not included; the JSON and JSONL files are the primary data. The script copies keep the original author's helper structure and may need path adjustments.
