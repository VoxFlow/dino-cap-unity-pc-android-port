import sys, os, json
os.environ.setdefault('IDADIR', r"C:\Program Files\IDA Professional 9.2")
sys.path.insert(0, os.path.join(os.environ["IDADIR"], "idalib", "python"))
import idapro
idapro.enable_console_messages(False)
OUT = os.environ["DC1_OUT"] + os.sep
idapro.open_database(os.environ["DC1_DB_COPY"], False)
import ida_funcs, ida_ida, idc, idautils, ida_name, ida_bytes
classes = []
for ea, nm in idautils.Names():
    if nm.startswith("_ZTV") and not nm.endswith("_ptr"):
        dem = ida_name.demangle_name("_" + nm, 0) or ida_name.demangle_name(nm, 0) or nm
        slots = []
        p = ea + 8
        while len(slots) < 120:
            v = ida_bytes.get_dword(p); t = v & ~1
            f = ida_funcs.get_func(t)
            if not f or f.start_ea != t: break
            n2 = ida_name.get_name(p)
            if n2 and n2.startswith(("_ZT", "__ZT")): break
            slots.append("0x%X" % t); p += 4
        classes.append({"vtable_ea": "0x%X" % ea, "symbol": nm, "demangled": dem, "slots": slots})
with open(OUT + "classes.jsonl", "w", encoding="utf-8") as f:
    for c in classes: f.write(json.dumps(c) + "\n")
print(len(classes), sum(len(c["slots"]) for c in classes))
idapro.close_database(False)
