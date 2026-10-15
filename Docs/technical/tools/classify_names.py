"""Classify the provenance of function names by comparing with the Mach-O symbol table.
Environment: DC1_MACHO (the analysed binary), DC1_FUNCTIONS_IN (functions.jsonl from export_functions.py), DC1_FUNCTIONS_OUT."""
import json, os, re, struct
b = open(os.environ["DC1_MACHO"], "rb").read()
ncmds = struct.unpack("<I", b[16:20])[0]
off = 28; sym = {}
for _ in range(ncmds):
    cmd, size = struct.unpack("<2I", b[off:off + 8])
    if cmd == 2:  # LC_SYMTAB
        symoff, nsyms, stroff, strsize = struct.unpack("<4I", b[off + 8:off + 24])
        for i in range(nsyms):
            n_strx, n_type, n_sect, n_desc, n_value = struct.unpack("<IBBhI", b[symoff + 12 * i:symoff + 12 * i + 12])
            if n_type & 0xE0 or n_sect == 0 or n_value == 0: continue   # skip debug and undefined entries
            end = b.index(b"\0", stroff + n_strx)
            sym.setdefault(n_value & ~1, b[stroff + n_strx:end].decode("utf-8", "replace"))
    off += size
auto = re.compile(r"^(sub|nullsub|j|loc|unk|byte|dword|word|qword|off|flt|dbl)_")
counts = {"macho_symbol": 0, "ida_auto": 0, "ida_db_name": 0}
with open(os.environ["DC1_FUNCTIONS_IN"], encoding="utf-8") as fi, open(os.environ["DC1_FUNCTIONS_OUT"], "w", encoding="utf-8") as fo:
    for line in fi:
        r = json.loads(line); ea = int(r["ea"], 16)
        if ea in sym:
            r["name_provenance"] = "Mach-O symbol table"; r["macho_symbol"] = sym[ea]; counts["macho_symbol"] += 1
        elif auto.match(r["name"]):
            r["name_provenance"] = "IDA automatic name"; counts["ida_auto"] += 1
        else:
            r["name_provenance"] = "IDA database name (not found in the Mach-O symbol table)"; counts["ida_db_name"] += 1
        fo.write(json.dumps(r) + "\n")
print(counts)
