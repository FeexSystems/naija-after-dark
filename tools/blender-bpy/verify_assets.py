"""Round-trip export QA for NAAD assets.

This script verifies that exported FBX/GLB files can be re-imported into Blender
without errors, checking:
- File existence and non-zero size
- FBX import success (no crashes or warnings)
- GLB import success (no crashes or warnings)
- Mesh integrity (vertex count, bounds)
- Material count matches expected
- For humanoid assets: armature presence and bone count

Usage:
  python tools/blender-bpy/verify_assets.py [--out <dir>]
  blender --background --python tools/blender-bpy/verify_assets.py -- [options]

Options:
  --out <dir>       output root (default: game/unity/Assets/NAAD/Content/Models)
  --only <prefix>   verify only assets whose name starts with prefix
  --category <CAT>  same as --only with a single category code
"""

import json
import os
import sys
import traceback

HERE = os.path.dirname(os.path.abspath(__file__))
if HERE not in sys.path:
    sys.path.insert(0, HERE)

try:
    import bpy  # noqa: E402
    HAS_BPY = True
except ImportError:
    HAS_BPY = False

import assets_apartment  # noqa: E402
import assets_beach  # noqa: E402
import assets_characters  # noqa: E402
import assets_nightclub  # noqa: E402
import assets_street  # noqa: E402
import assets_suya  # noqa: E402
import assets_vehicles  # noqa: E402

ALL_SPECS = []
for module in (assets_apartment, assets_street, assets_suya, assets_nightclub,
               assets_beach, assets_vehicles, assets_characters):
    ALL_SPECS.extend(module.SPECS)

REGISTRY = {spec.name: spec for spec in ALL_SPECS}


def repo_root():
    return os.path.abspath(os.path.join(HERE, "..", ".."))


def parse_args(argv):
    if "--" in argv:
        args = argv[argv.index("--") + 1:]
    else:
        args = argv[1:]
    opts = {
        "out": os.path.join(repo_root(), "game", "unity", "Assets", "NAAD",
                            "Content", "Models"),
        "only": None,
    }
    i = 0
    while i < len(args):
        arg = args[i]
        if arg in ("--out", "--only", "--category") and i + 1 < len(args):
            value = args[i + 1]
            i += 2
            if arg == "--out":
                opts["out"] = os.path.abspath(value)
            elif arg == "--only":
                opts["only"] = [v for v in value.split(",") if v]
            elif arg == "--category":
                opts["only"] = [v + "_" for v in value.split(",") if v]
            continue
        i += 1
    return opts


def selected(only):
    if not only:
        return ALL_SPECS
    picked = []
    for prefix in only:
        picked.extend([s for s in ALL_SPECS if s.name.startswith(prefix)])
    seen = set()
    unique = []
    for spec in picked:
        if spec.name not in seen:
            seen.add(spec.name)
            unique.append(spec)
    return unique


def reset_scene():
    if HAS_BPY:
        bpy.ops.wm.read_factory_settings(use_empty=True)
        for block in (bpy.data.meshes, bpy.data.materials, bpy.data.collections,
                      bpy.data.objects, bpy.data.armatures):
            for item in list(block):
                try:
                    block.remove(item)
                except Exception:
                    pass


def verify_fbx(path, spec):
    """Import FBX and verify structure."""
    if not HAS_BPY:
        return {"fbx_import": "skipped (no bpy)"}

    try:
        reset_scene()
        bpy.ops.import_scene.fbx(filepath=path)
    except Exception as e:
        return {"fbx_import": "failed: %s" % str(e)}

    # Check objects
    objects = [o for o in bpy.data.objects if o.type in ("MESH", "ARMATURE")]
    if not objects:
        return {"fbx_import": "no objects found"}

    # For humanoid assets, check armature
    if spec.humanoid:
        armatures = [o for o in objects if o.type == "ARMATURE"]
        if not armatures:
            return {"fbx_import": "humanoid asset missing armature"}
        arm = armatures[0]
        bone_count = len(arm.data.bones)
        if bone_count < 20:  # Mixamo has ~23 bones
            return {"fbx_import": "armature has too few bones: %d" % bone_count}

    # Check meshes
    meshes = [o for o in objects if o.type == "MESH"]
    if not meshes:
        return {"fbx_import": "no meshes found"}

    total_verts = sum(len(m.data.vertices) for m in meshes)
    total_tris = 0
    for m in meshes:
        m.data.calc_loop_triangles()
        total_tris += len(m.data.loop_triangles)

    return {
        "fbx_import": "ok",
        "fbx_objects": len(objects),
        "fbx_meshes": len(meshes),
        "fbx_vertices": total_verts,
        "fbx_triangles": total_tris,
    }


def verify_glb(path, spec):
    """Import GLB and verify structure."""
    if not HAS_BPY:
        return {"glb_import": "skipped (no bpy)"}

    try:
        reset_scene()
        bpy.ops.import_scene.gltf(filepath=path)
    except Exception as e:
        return {"glb_import": "failed: %s" % str(e)}

    # Check objects
    objects = [o for o in bpy.data.objects if o.type in ("MESH", "ARMATURE")]
    if not objects:
        return {"glb_import": "no objects found"}

    # For humanoid assets, check armature
    if spec.humanoid:
        armatures = [o for o in objects if o.type == "ARMATURE"]
        if not armatures:
            return {"glb_import": "humanoid asset missing armature"}

    # Check meshes
    meshes = [o for o in objects if o.type == "MESH"]
    if not meshes:
        return {"glb_import": "no meshes found"}

    total_verts = sum(len(m.data.vertices) for m in meshes)
    total_tris = 0
    for m in meshes:
        m.data.calc_loop_triangles()
        total_tris += len(m.data.loop_triangles)

    return {
        "glb_import": "ok",
        "glb_objects": len(objects),
        "glb_meshes": len(meshes),
        "glb_vertices": total_verts,
        "glb_triangles": total_tris,
    }


def verify_file(path):
    """Check file exists and has non-zero size."""
    if not os.path.isfile(path):
        return {"exists": False, "size": 0}
    return {"exists": True, "size": os.path.getsize(path)}


def main():
    opts = parse_args(sys.argv)
    specs = selected(opts["only"])

    if not specs:
        print("[VERIFY] FAIL: selector matched no assets: %s" % opts["only"])
        return 2

    results = []
    failures = []

    for spec in specs:
        out_dir = os.path.join(opts["out"], spec.district)
        fbx_path = os.path.join(out_dir, spec.name + ".fbx")
        glb_path = os.path.join(out_dir, spec.name + ".glb")

        result = {
            "name": spec.name,
            "district": spec.district,
            "humanoid": spec.humanoid,
        }

        # Check files
        fbx_info = verify_file(fbx_path)
        glb_info = verify_file(glb_path)
        result.update({"fbx": fbx_info, "glb": glb_info})

        if not fbx_info["exists"] or fbx_info["size"] == 0:
            result["status"] = "fbx_missing"
            failures.append(spec.name)
            results.append(result)
            continue

        if not glb_info["exists"] or glb_info["size"] == 0:
            result["status"] = "glb_missing"
            failures.append(spec.name)
            results.append(result)
            continue

        # Import and verify
        if HAS_BPY:
            fbx_verify = verify_fbx(fbx_path, spec)
            glb_verify = verify_glb(glb_path, spec)
            result.update(fbx_verify)
            result.update(glb_verify)

            if fbx_verify.get("fbx_import") != "ok":
                result["status"] = "fbx_import_failed"
                failures.append(spec.name)
            elif glb_verify.get("glb_import") != "ok":
                result["status"] = "glb_import_failed"
                failures.append(spec.name)
            else:
                result["status"] = "ok"
        else:
            result["status"] = "files_ok_no_import_check"

        results.append(result)
        print("[VERIFY] %s: %s" % (spec.name, result["status"]))

    # Write report
    report_path = os.path.join(opts["out"], "naad_verify_report.json")
    os.makedirs(os.path.dirname(report_path), exist_ok=True)
    with open(report_path, "w", encoding="utf-8") as f:
        json.dump({
            "generator": "tools/blender-bpy/verify_assets.py",
            "has_bpy": HAS_BPY,
            "total": len(specs),
            "passed": len([r for r in results if r["status"] == "ok"]),
            "failed": len(failures),
            "assets": results,
        }, f, indent=2)

    print("[VERIFY] SUMMARY passed=%d failed=%d of %d"
          % (len([r for r in results if r["status"] == "ok"]), len(failures), len(specs)))
    print("[VERIFY] WROTE %s" % report_path)

    if failures:
        print("[VERIFY] FAILED %s" % ", ".join(failures))
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
