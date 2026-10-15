import sys, os, json
os.environ.setdefault('IDADIR', r"C:\Program Files\IDA Professional 9.2")
sys.path.insert(0, os.path.join(os.environ["IDADIR"], "idalib", "python"))
import idapro
idapro.enable_console_messages(False)
OUT = os.environ["DC1_OUT"] + os.sep
idapro.open_database(os.environ["DC1_DB_COPY"], False)
import ida_funcs, idc, idautils, ida_bytes
core = [int(k, 16) for k in json.load(open(os.environ["DC1_REG_FUNCS"]))]
res = {}
for fea in sorted(core):
    f = ida_funcs.get_func(fea)
    if not f: continue
    L = []
    for h in idautils.FuncItems(fea):
        sz = ida_bytes.get_item_size(h)
        L.append("0x%X  %-12s %s" % (h, (ida_bytes.get_bytes(h, sz) or b"").hex(), idc.generate_disasm_line(h, 0)[:100]))
    res["0x%X" % fea] = {"start": f.start_ea, "end": f.end_ea, "disasm": L}
json.dump(res, open(OUT + "core_disasm.json", "w"))
print(len(res), sum(len(v["disasm"]) for v in res.values()))
idapro.close_database(False)
