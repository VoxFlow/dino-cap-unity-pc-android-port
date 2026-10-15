# TASK027M.8.3 - F3 anatomical X-ray skeleton asset generator.
#
# SOURCE-FAITHFUL: the bone pixels are the original Dino Cap `k*gt*` (gutou =
# bone) X-ray skeleton parts, packed into every shipped atlas (iPad dino1/dino2,
# phone k1/k2) but referenced by NO recovered native consumer (all string xrefs
# are inside the 4 atlas builders sub_1BDC0/sub_2A8A8/sub_8925C/sub_A1518).
# We extract them verbatim from the iPad hi-res atlases and pack a Unity texture.
# Only a mild cool tint is applied to the bright bone texels so they read as an
# "X-ray" rather than loose bones; the dark body shell and every alpha value are
# preserved byte-for-byte. No resampling (point-filter safe).

from PIL import Image
import os, json

# Paths come from the environment so the script is portable: DC1_DEV_ROOT = the reverse-engineering/report root (contains Extracted/),
# DC1_UNITY_ROOT = the Unity project (defaults to four levels above this script), DC1_TMP = scratch output folder.
DEV = os.environ['DC1_DEV_ROOT']
DC1 = os.environ.get('DC1_UNITY_ROOT') or os.path.abspath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', '..', '..', '..'))
GEN_TEX = os.path.join(DC1, 'Assets', 'DinoCap', 'Generated', 'Resources', 'DinoCapGenerated', 'Textures')
GEN_INSPECT = os.path.join(DC1, 'Assets', 'DinoCap', 'Generated', 'F3Xray')
TMP = os.environ.get('DC1_TMP') or os.path.join(os.path.dirname(os.path.abspath(__file__)), '_tmp')
os.makedirs(TMP, exist_ok=True)

dino1 = Image.open(os.path.join(DEV, 'Extracted', 'Data', 'Image', 'iPad', 'dino1.png')).convert('RGBA')
dino2 = Image.open(os.path.join(DEV, 'Extracted', 'Data', 'Image', 'iPad', 'dino2.png')).convert('RGBA')
SRC = {'dino1': dino1, 'dino2': dino2}

# gt part -> (atlas, x, y, w, h)  [iPad hi-res rects, from Task019 registry]
GROUP_A = {  # k3gt* : serves species k1, k3, k4  (k1_be/k4_be use k4* refs, k3_be uses k3* refs; same rig dims)
    'tou':   ('dino2', 344, 257, 116, 95),
    'xiaba': ('dino2', 135, 439, 81, 89),
    'shen':  ('dino2', 170, 257, 174, 81),
    't1':    ('dino1', 391, 89, 40, 43),
    't2':    ('dino1', 308, 119, 44, 21),
    't3':    ('dino1', 333, 43, 34, 34),
    't4':    ('dino1', 274, 131, 34, 34),
    't5':    ('dino1', 354, 77, 37, 17),
    't6':    ('dino1', 170, 100, 29, 27),
    'zhua':  ('dino1', 352, 97, 34, 43),
}
GROUP_B = {  # k2gt* : serves species k2 (larger rig)
    'tou':   ('dino2', 170, 118, 183, 139),
    'xiaba': ('dino2', 782, 139, 125, 124),
    'shen':  ('dino2', 266, 0, 266, 118),
    't1':    ('dino1', 174, 429, 66, 83),
    't2':    ('dino1', 258, 265, 71, 41),
    't3':    ('dino1', 116, 298, 67, 52),
    't4':    ('dino1', 174, 361, 54, 68),
    't5':    ('dino1', 329, 222, 57, 34),
    't6':    ('dino1', 183, 277, 58, 42),
    'zhua':  ('dino1', 254, 165, 58, 66),
}

XRAY_TINT = (232, 242, 255)   # cool white for bone texels
BONE_LUMA = 150               # >= this mean -> treat as bone, tint it

def xray_process(im):
    im = im.copy()
    px = im.load()
    w, h = im.size
    for y in range(h):
        for x in range(w):
            r, g, b, a = px[x, y]
            if a == 0:
                continue
            luma = (r + g + b) / 3.0
            if luma >= BONE_LUMA:
                # bright bone -> gentle cool tint, keep its own brightness
                k = luma / 255.0
                px[x, y] = (
                    int(XRAY_TINT[0] * k + r * (1 - k) * 0),  # push toward tint*brightness
                    int(XRAY_TINT[1] * k),
                    int(XRAY_TINT[2] * k),
                    a,
                )
                # simpler: blend 55% toward tinted-by-brightness
                tr = int(XRAY_TINT[0] * k); tg = int(XRAY_TINT[1] * k); tb = int(XRAY_TINT[2] * k)
                px[x, y] = (int(r * .45 + tr * .55), int(g * .45 + tg * .55), int(b * .45 + tb * .55), a)
            # dark shell + mid texels: leave exactly as shipped
    return im

def bones_only(im):
    """Drop the dark body-shell texels -> bones on transparent (SHELL OFF variant)."""
    im = im.copy()
    px = im.load()
    w, h = im.size
    for y in range(h):
        for x in range(w):
            r, g, b, a = px[x, y]
            if a == 0:
                continue
            luma = (r + g + b) / 3.0
            if luma < 120:
                px[x, y] = (r, g, b, 0)              # shell -> fully transparent
            elif luma < 165:
                px[x, y] = (r, g, b, int(a * 0.35))  # soft edge feather
    return im

PAD = 2
def pack(parts_by_group):
    # collect all (refname, image)
    items = []
    for grp, table in parts_by_group.items():
        for part, (atl, x, y, w, h) in table.items():
            crop = SRC[atl].crop((x, y, x + w, y + h))
            proc = xray_process(crop)
            items.append((f'xr_{grp}_{part}', proc, w, h))          # shell + bones (as shipped)
            items.append((f'xrb_{grp}_{part}', bones_only(proc), w, h))  # bones only
    # shelf-pack into 512 wide
    AW = 512
    items.sort(key=lambda t: -t[3])
    x = y = row_h = 0
    placed = []
    for ref, img, w, h in items:
        if x + w + PAD > AW:
            x = 0
            y += row_h + PAD
            row_h = 0
        placed.append((ref, img, x, y, w, h))
        x += w + PAD
        row_h = max(row_h, h)
    AH = 1
    total_h = y + row_h + PAD
    while AH < total_h:
        AH *= 2
    atlas = Image.new('RGBA', (AW, AH), (0, 0, 0, 0))
    entries = []
    for ref, img, px_, py_, w, h in placed:
        atlas.alpha_composite(img, (px_, py_))
        entries.append(dict(ref=ref, x=px_, yTop=py_, w=w, h=h))
    return atlas, entries, (AW, AH)

parts_by_group = {'k3': GROUP_A, 'k2': GROUP_B}
atlas, entries, (AW, AH) = pack(parts_by_group)

os.makedirs(GEN_TEX, exist_ok=True)
atlas_path = os.path.join(GEN_TEX, 'f3xray_skeleton.png')
atlas.save(atlas_path)
print('packed atlas', atlas_path, (AW, AH), 'entries', len(entries))

# .meta - clone of ipad_dino1.png.meta with a fresh deterministic GUID
meta_src = open(os.path.join(GEN_TEX, 'ipad_dino1.png.meta'), encoding='utf-8').read()
import re
NEW_GUID = 'f3c0a5511be7a4a3d1c0fae913570001'  # 32 hex, deterministic, unique to F3Xray skeleton
meta_new = re.sub(r'guid: [0-9a-f]{32}', 'guid: ' + NEW_GUID, meta_src, count=1)
open(atlas_path + '.meta', 'w', encoding='utf-8', newline='\n').write(meta_new)
print('wrote meta with guid', NEW_GUID)

# manifest atlas entries (textureResource + textureWidth/Height added)
manifest_entries = []
for e in entries:
    manifest_entries.append(dict(
        ref=e['ref'],
        textureResource='DinoCapGenerated/Textures/f3xray_skeleton',
        textureWidth=AW, textureHeight=AH,
        x=float(e['x']), yTop=float(e['yTop']), w=float(e['w']), h=float(e['h']),
        confidence='SOURCE_FAITHFUL_ASSET_PORT_RECON_TIMING'))
open(os.path.join(TMP, 'f3xray_atlas_entries.json'), 'w', encoding='utf-8').write(
    json.dumps(manifest_entries, indent=2))
print('manifest entries ->', os.path.join(TMP, 'f3xray_atlas_entries.json'))

# per-species inspection assets (k1/k3/k4 = group A, k2 = group B)
SPECIES_GROUP = {'k1': GROUP_A, 'k3': GROUP_A, 'k4': GROUP_A, 'k2': GROUP_B}
anatomy = {}
for sp, table in SPECIES_GROUP.items():
    d = os.path.join(GEN_INSPECT, sp)
    os.makedirs(d, exist_ok=True)
    parts_meta = {}
    for part, (atl, x, y, w, h) in table.items():
        crop = xray_process(SRC[atl].crop((x, y, x + w, y + h)))
        crop.save(os.path.join(d, part + '.png'))
        # bone coverage stats
        pxs = [q for q in crop.getdata() if q[3] >= 40]
        n = max(1, len(pxs))
        bone = sum(1 for q in pxs if (q[0] + q[1] + q[2]) / 3 >= 150)
        parts_meta[part] = dict(w=w, h=h, opaque_px=len(pxs), bone_px=bone,
                                bone_pct=round(100 * bone / n))
    anatomy[sp] = dict(rig='A' if table is GROUP_A else 'B', parts=parts_meta)

open(os.path.join(GEN_INSPECT, 'f3xray_species_anatomy.json'), 'w', encoding='utf-8').write(
    json.dumps(anatomy, indent=2))
print('per-species inspection assets ->', GEN_INSPECT)

# contact sheet per species for the visual audit
for sp, table in SPECIES_GROUP.items():
    CS = Image.new('RGBA', (6 * 130, 2 * 170), (20, 24, 34, 255))
    for i, (part, (atl, x, y, w, h)) in enumerate(table.items()):
        crop = xray_process(SRC[atl].crop((x, y, x + w, y + h)))
        sc = min((120) / w, (150) / h)
        c2 = crop.resize((max(1, int(w * sc)), max(1, int(h * sc))), Image.NEAREST)
        cx = (i % 6) * 130 + (130 - c2.width) // 2
        cy = (i // 6) * 170 + (160 - c2.height) // 2
        CS.alpha_composite(c2, (cx, cy))
    CS.resize((CS.width * 2, CS.height * 2), Image.NEAREST).save(os.path.join(TMP, f'audit_{sp}.png'))
print('audit contact sheets -> tmp/audit_k1..k4.png')
