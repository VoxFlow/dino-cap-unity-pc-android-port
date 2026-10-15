import sys, os, json, hashlib
os.environ.setdefault('IDADIR', r"C:\Program Files\IDA Professional 9.2")
sys.path.insert(0, os.path.join(os.environ["IDADIR"], "idalib", "python"))
import idapro
idapro.enable_console_messages(False)
OUT = os.environ["DC1_OUT"] + os.sep
os.makedirs(OUT, exist_ok=True)
rc = idapro.open_database(os.environ["DC1_DB_COPY"], False)
import ida_funcs, ida_ida, idc, idautils, ida_idp, ida_segregs, ida_bytes, ida_name, ida_xref, ida_hexrays, ida_lines, ida_nalt, ida_segment
IMAGEBASE = ida_ida.inf_get_min_ea()
TREG = ida_idp.str2reg("T")
print('hexrays init', ida_hexrays.init_hexrays_plugin())
reg = json.load(open(os.environ["DC1_REG_FUNCS"]))
reg_ids = {int(k, 16): v for k, v in reg.items()}
SEL = [0x76A20, 0x17BA5C, 0x17C950, 0x56E94, 0x666BC, 0x66550, 0x68148, 0x57BD8, 0xFD58, 0xFE50, 0x10294, 0xCE2C, 0x6F124, 0x66110, 0x165298, 0x7EB04, 0x18858]

def isa(ea):
    try:
        v = ida_segregs.get_sreg(ea, TREG)
        return "THUMB" if v == 1 else "ARM"
    except Exception:
        return "UNKNOWN"

def seg_name(ea):
    s = ida_segment.getseg(ea)
    return ida_segment.get_segm_name(s) if s else None

def file_offset(ea):
    # Mach-O segment table of the thin armv7 binary: __TEXT vm 0x4000 size 0x1f8000 fileoff 0; __DATA vm 0x1fc000 size 0x24000 filesize 0x20000 fileoff 0x1f8000
    if 0x4000 <= ea < 0x4000 + 0x1f8000: return ea - 0x4000
    if 0x1fc000 <= ea < 0x1fc000 + 0x20000: return ea - 0x1fc000 + 0x1f8000
    return None

n = 0
with open(OUT + "functions.jsonl", "w", encoding="utf-8") as fo:
    for fea in idautils.Functions():
        f = ida_funcs.get_func(fea)
        if not f: continue
        size = f.end_ea - f.start_ea
        raw = ida_bytes.get_bytes(f.start_ea, size) or b""
        callers = 0
        for x in idautils.XrefsTo(f.start_ea, 0):
            if x.type in (ida_xref.fl_CN, ida_xref.fl_CF, ida_xref.fl_JN, ida_xref.fl_JF):
                callers += 1
        rec = {"id": "FUN-%06X" % fea, "binary_id": "BIN-DC1-IOS-1.9.8-ARMV7", "address_kind": "static_ea",
               "ea": "0x%X" % fea, "rva": "0x%X" % (fea - IMAGEBASE), "file_offset": ("0x%X" % file_offset(fea)) if file_offset(fea) is not None else None,
               "segment": seg_name(fea), "size": size, "isa": isa(fea), "name": ida_funcs.get_func_name(fea),
               "name_provenance": "analyst/auto (IDA)" if ida_funcs.get_func_name(fea).startswith("sub_") else "symbol from binary",
               "direct_callers": callers, "bytes_sha256": hashlib.sha256(raw).hexdigest(),
               "registry_contracts": reg_ids.get(fea, [])}
        fo.write(json.dumps(rec) + "\n"); n += 1
print("functions", n)

# call-graph edges among registry functions (direct code xrefs from instructions inside the function)
regset = set(reg_ids)
edges = []
for fea in sorted(regset):
    f = ida_funcs.get_func(fea)
    if not f: continue
    for head in idautils.FuncItems(fea):
        for x in idautils.XrefsFrom(head, 0):
            if x.type in (ida_xref.fl_CN, ida_xref.fl_CF, ida_xref.fl_JN, ida_xref.fl_JF):
                t = ida_funcs.get_func(x.to)
                if t and t.start_ea in regset and t.start_ea != fea:
                    edges.append({"caller": "0x%X" % fea, "call_site": "0x%X" % head, "callee": "0x%X" % t.start_ea})
json.dump(edges, open(OUT + "callgraph_registry_edges.json", "w"), indent=0)
print("edges", len(edges))

# dossier raw material for selected functions
dos = {}
for fea in SEL:
    f = ida_funcs.get_func(fea)
    if not f:
        dos["0x%X" % fea] = {"error": "not a function start in the database"}; continue
    lines = []
    cnt = 0
    for head in idautils.FuncItems(f.start_ea):
        sz = ida_bytes.get_item_size(head)
        raw = (ida_bytes.get_bytes(head, sz) or b"").hex()
        lines.append("0x%X  %-12s %s" % (head, raw, idc.generate_disasm_line(head, 0)))
        cnt += 1
    pseudo = None
    try:
        cf = ida_hexrays.decompile(f.start_ea)
        if cf is None: raise RuntimeError('decompile returned None')
        pseudo = [ida_lines.tag_remove(l.line) for l in cf.get_pseudocode()]
    except Exception as e:
        pseudo = ["<decompile failed: %s>" % e]
    dos["0x%X" % fea] = {"name": ida_funcs.get_func_name(f.start_ea), "start": "0x%X" % f.start_ea, "end": "0x%X" % f.end_ea, "instructions": cnt, "isa": isa(f.start_ea), "disasm": lines, "pseudocode": pseudo}
json.dump(dos, open(OUT + "dossier_raw.json", "w"))
print("dossiers", len(dos), {k: v.get("instructions") for k, v in dos.items()})
idapro.close_database(False)
