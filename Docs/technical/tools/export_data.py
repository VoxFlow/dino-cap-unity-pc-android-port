import sys, os, json, re, struct
os.environ.setdefault('IDADIR', r"C:\Program Files\IDA Professional 9.2")
sys.path.insert(0, os.path.join(os.environ["IDADIR"], "idalib", "python"))
import idapro
idapro.enable_console_messages(False)
OUT = os.environ["DC1_OUT"] + os.sep
os.makedirs(OUT, exist_ok=True)
idapro.open_database(os.environ["DC1_DB_COPY"], False)
import ida_funcs, ida_ida, idc, idautils, ida_name, ida_bytes, ida_xref, ida_segment, ida_hexrays, ida_lines
reg_funcs = json.load(open(os.environ["DC1_REG_FUNCS"]))
core = {int(k, 16) for k in reg_funcs}
reg_text = open(os.environ["DC1_REGISTRY"], encoding="utf-8-sig").read()

def fn_start(ea):
    f = ida_funcs.get_func(ea)
    return f.start_ea if f else None

# ---- strings
strs = {}
for s in idautils.Strings():
    try: txt = str(s)
    except Exception: continue
    strs[s.ea] = txt[:160]
with open(OUT + "strings.jsonl", "w", encoding="utf-8") as f:
    for ea, txt in sorted(strs.items()):
        refs = sorted({fn_start(x.frm) for x in idautils.XrefsTo(ea, 0) if fn_start(x.frm)})[:8]
        f.write(json.dumps({"ea": "0x%X" % ea, "text": txt, "ref_functions": ["0x%X" % r for r in refs]}) + "\n")
print("strings", len(strs))

# ---- globals cited by the registry
names = sorted(set(re.findall(r"\b((?:dword|byte|flt|word|qword|off|unk|dbl)_[0-9A-Fa-f]{4,6})\b", reg_text)))
gl = []
for nm in names:
    ea = ida_name.get_name_ea(idc.BADADDR, nm)
    if ea == idc.BADADDR: continue
    rd, wr = set(), set()
    for x in idautils.XrefsTo(ea, 0):
        fs = fn_start(x.frm)
        if not fs: continue
        (wr if x.type == ida_xref.dr_W else rd).add(fs)
    gl.append({"name": nm, "ea": "0x%X" % ea, "segment": ida_segment.get_segm_name(ida_segment.getseg(ea)), "reader_functions": ["0x%X" % a for a in sorted(rd)][:12], "writer_functions": ["0x%X" % a for a in sorted(wr)][:12], "n_readers": len(rd), "n_writers": len(wr)})
with open(OUT + "globals.jsonl", "w", encoding="utf-8") as f:
    for g in gl: f.write(json.dumps(g) + "\n")
print("globals", len(gl), "of", len(names))

# ---- full direct call graph
edges = 0
with open(OUT + "callgraph_all.jsonl", "w", encoding="utf-8") as f:
    for fea in idautils.Functions():
        for head in idautils.FuncItems(fea):
            for x in idautils.XrefsFrom(head, 0):
                if x.type in (ida_xref.fl_CN, ida_xref.fl_CF, ida_xref.fl_JN, ida_xref.fl_JF):
                    t = ida_funcs.get_func(x.to)
                    if t and t.start_ea != fea:
                        kind = "r"   # r = destination resolved by IDA (indirect jump, objc message send, thunk); d = encoded direct target equals the destination
                        try:
                            if idc.get_operand_type(head, 0) == idc.o_near and idc.get_operand_value(head, 0) == x.to: kind = "d"
                        except Exception:
                            pass
                        f.write(json.dumps([fea, head, t.start_ea, kind]) + "\n"); edges += 1
print("edges", edges)

# ---- per-core-function: referenced strings, globals, outside callers, decompile
per = {}
for fea in sorted(core):
    f = ida_funcs.get_func(fea)
    if not f: continue
    s_ref, g_ref, callees, callers = set(), set(), set(), set()
    for head in idautils.FuncItems(fea):
        for x in idautils.XrefsFrom(head, 0):
            if x.to in strs: s_ref.add(x.to)
            nm = ida_name.get_name(x.to)
            if nm and re.match(r"(dword|byte|flt|word|qword)_", nm) and x.type in (ida_xref.dr_R, ida_xref.dr_W, ida_xref.dr_O):
                g_ref.add(nm)
            if x.type in (ida_xref.fl_CN, ida_xref.fl_CF):
                t = ida_funcs.get_func(x.to)
                if t: callees.add(t.start_ea)
    for x in idautils.XrefsTo(fea, 0):
        if x.type in (ida_xref.fl_CN, ida_xref.fl_CF, ida_xref.fl_JN, ida_xref.fl_JF):
            c = fn_start(x.frm)
            if c: callers.add((c, x.frm))
    pseudo = None
    try:
        cf = ida_hexrays.decompile(fea)
        if cf: pseudo = [ida_lines.tag_remove(l.line)[:160] for l in cf.get_pseudocode()][:60]
    except Exception:
        pass
    per["0x%X" % fea] = {"strings": [{"ea": "0x%X" % a, "text": strs[a]} for a in sorted(s_ref)][:12], "globals": sorted(g_ref)[:16],
                         "callees": ["0x%X" % a for a in sorted(callees)][:40], "callers": [{"fn": "0x%X" % c, "site": "0x%X" % s} for c, s in sorted(callers)][:30],
                         "pseudocode_head": pseudo}
json.dump(per, open(OUT + "core_details.json", "w"))
print("core details", len(per), "with pseudocode", sum(1 for v in per.values() if v["pseudocode_head"]))
idapro.close_database(False)
