"""NAAD bpy smoke test + naming check. Runs two ways:
  python validate_bpy.py                      (bpy module from pip)
  blender --background --python validate_bpy.py   (embedded interpreter)
Exit 0 = OK, 1 = bpy missing/broken, 2 = NAAD rule regression.
"""
import os
import sys
import tempfile

HERE = os.path.dirname(os.path.abspath(__file__))
ADDON_DIR = os.path.abspath(os.path.join(HERE, "..", "unity-asset-pipeline", "naad_addon"))
if ADDON_DIR not in sys.path:
    sys.path.insert(0, ADDON_DIR)

try:
    import bpy
except ImportError as err:
    print("[NAAD] FAIL: cannot import bpy: %s" % err)
    print("[NAAD] fix: pip install bpy  OR  blender --background --python %s" % __file__)
    sys.exit(1)

print("[NAAD] bpy OK: Blender %s, Python %s" % (
    getattr(bpy.app, "version_string", "?"), ".".join(map(str, sys.version_info[:3]))))

# 1. NAAD naming grammar (bpy-free rules.py, same as add-on).
try:
    import rules
except ImportError as err:
    print("[NAAD] FAIL: cannot import rules.py: %s" % err)
    sys.exit(2)

cases = [
    ("PRP", "Booth", "ND", None, 1, "PRP_Booth_ND_v001"),
    ("ENV" if "ENV" in rules.CATEGORIES else "NCL", "Stage", "IS", "M", 2, None),
]
failed = False
for cat, part, dist, size, ver, exact in cases:
    name, problems = rules.suggest_asset_name(cat, part, dist, ver, size)
    if problems or not name or not rules.split_asset_name(name):
        print("[NAAD] FAIL rule: %s/%s/%s -> %r %r" % (cat, part, dist, name, problems))
        failed = True
    elif exact and name != exact:
        print("[NAAD] FAIL rule: expected %s got %s" % (exact, name))
        failed = True
    else:
        print("[NAAD] rule OK: %s" % name)
if failed:
    sys.exit(2)

# 2. Headless scene op: create cube with valid NAAD name, export FBX.
name, _ = rules.suggest_asset_name("PRP", "SmokeCube", "ND", 1, None)
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.mesh.primitive_cube_add()
cube = bpy.context.active_object
cube.name = name
tmp = tempfile.mkdtemp(prefix="naad_bpy_")
fbx = os.path.join(tmp, name + ".fbx")
try:
    bpy.ops.export_scene.fbx(filepath=fbx, use_selection=True)
    ok = os.path.isfile(fbx) and os.path.getsize(fbx) > 0
    print("[NAAD] FBX export %s: %s (%d bytes)" % (
        "OK", fbx, os.path.getsize(fbx) if ok else 0))
    if not ok:
        sys.exit(1)
except Exception as err:
    print("[NAAD] FAIL FBX export: %s" % err)
    sys.exit(1)

print("[NAAD] PASS: bpy import + rules + headless FBX export all OK.")
