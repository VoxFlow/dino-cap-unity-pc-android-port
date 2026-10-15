"""Collect the sub_XXXX addresses cited by the registry. Usage: set DC1_REGISTRY and DC1_REG_FUNCS, then run."""
import json, os, re, collections
reg = json.load(open(os.environ["DC1_REGISTRY"], encoding="utf-8-sig"))["contracts"]
where = collections.defaultdict(set)
for c in reg:
    for m in re.finditer(r"sub_([0-9A-Fa-f]{4,6})\b", json.dumps(c)):
        where[m.group(1).upper()].add(c["id"])
json.dump({a: sorted(v) for a, v in where.items()}, open(os.environ["DC1_REG_FUNCS"], "w"))
print(len(where), "distinct functions")
