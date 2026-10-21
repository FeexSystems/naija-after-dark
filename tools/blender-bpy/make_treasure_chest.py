"""Model a low-poly treasure chest and export to .glb.
Runs both ways:
  python make_treasure_chest.py [output.glb]        (bpy pip module)
  blender --background --python make_treasure_chest.py -- [output.glb]

Default output: <repo>/game/unity/Assets/Models/TreasureChest.glb
(Unity layout) + fallback <repo>/Assets/Models/TreasureChest.glb.
"""
import os
import sys

try:
    import bpy
except ImportError as err:
    print("[NAAD] FAIL: bpy not available: %s" % err)
    print("[NAAD] fix: pip install bpy (py3.13) OR use blender --background --python")
    sys.exit(1)


def repo_root():
    here = os.path.dirname(os.path.abspath(__file__))
    # tools/blender-bpy -> repo root
    return os.path.abspath(os.path.join(here, "..", ".."))


def resolve_output(argv):
    # blender passes args after '--'; plain python passes them directly
    args = argv[1:]
    if "--" in argv:
        args = argv[argv.index("--") + 1:]
    if args:
        p = args[0]
        if not os.path.isabs(p):
            p = os.path.join(os.getcwd(), p)
        return p
    root = repo_root()
    unity_path = os.path.join(root, "game", "unity", "Assets", "Models", "TreasureChest.glb")
    # Prefer Unity path when the Unity project exists, else repo-level Assets/
    if os.path.isdir(os.path.join(root, "game", "unity", "Assets")):
        return unity_path
    return os.path.join(root, "Assets", "Models", "TreasureChest.glb")


def mat_simple(name, base_color, metallic=0.0, roughness=0.8):
    mat = bpy.data.materials.new(name=name)
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get("Principled BSDF")
    if bsdf:
        bsdf.inputs["Base Color"].default_value = (*base_color, 1.0)
        if "Metallic" in bsdf.inputs:
            bsdf.inputs["Metallic"].default_value = metallic
        if "Roughness" in bsdf.inputs:
            bsdf.inputs["Roughness"].default_value = roughness
    return mat


def box(name, loc, scale, material=None):
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=loc)
    obj = bpy.context.active_object
    obj.name = name
    obj.scale = scale
    bpy.ops.object.transform_apply(scale=True)
    if material:
        obj.data.materials.append(material)
    return obj


def main():
    out = resolve_output(sys.argv)
    os.makedirs(os.path.dirname(out), exist_ok=True)

    # --- clean scene ---
    bpy.ops.wm.read_factory_settings(use_empty=True)

    # --- materials: wood body, dark metal bands, gold lock ---
    wood = mat_simple("NAAD_Wood", (0.45, 0.26, 0.12), metallic=0.0, roughness=0.85)
    metal = mat_simple("NAAD_DarkMetal", (0.12, 0.12, 0.14), metallic=0.85, roughness=0.45)
    gold = mat_simple("NAAD_Gold", (1.0, 0.72, 0.18), metallic=1.0, roughness=0.3)

    # --- body: 2.0 wide (X) x 1.2 deep (Y) x 1.0 tall, base at z 0..1 ---
    body = box("ChestBody", (0, 0, 0.5), (2.0, 1.2, 1.0), wood)

    # --- lid: flat low-poly lid with slight overhang ---
    lid = box("ChestLid", (0, 0, 1.12), (2.1, 1.3, 0.25), wood)

    # --- lid ridge (stylised curve hint, still 1 cube) ---
    ridge = box("ChestLidRidge", (0, 0, 1.30), (1.7, 1.0, 0.12), wood)

    # --- dark metal bands: 2 vertical straps wrapping body+lid ---
    for i, x in enumerate((-0.65, 0.65)):
        band_body = box("BandBody_%d" % i, (x, 0, 0.5), (0.14, 1.26, 1.06), metal)
        band_lid = box("BandLid_%d" % i, (x, 0, 1.12), (0.14, 1.36, 0.31), metal)
        # keep refs to avoid unused warnings
        _ = (band_body, band_lid)

    # --- horizontal trim along base ---
    trim = box("BaseTrim", (0, 0, 0.10), (2.06, 1.26, 0.12), metal)

    # --- gold lock: plate on front + hasp on lid + keyhole ---
    lock_plate = box("GoldLockPlate", (0, -0.62, 0.72), (0.30, 0.08, 0.40), gold)
    lock_hasp = box("GoldLockHasp", (0, -0.64, 1.10), (0.22, 0.08, 0.28), gold)
    keyhole = box("Keyhole", (0, -0.665, 0.72), (0.07, 0.03, 0.12), metal)

    # --- feet: 4 small metal cubes ---
    for ix, x in enumerate((-0.85, 0.85)):
        for iz, y in enumerate((-0.45, 0.45)):
            box("Foot_%d_%d" % (ix, iz), (x, y, 0.05), (0.22, 0.22, 0.10), metal)

    _ = (body, lid, ridge, trim, lock_plate, lock_hasp, keyhole)

    # --- ensure glTF exporter available (usually built-in) ---
    try:
        bpy.ops.preferences.addon_enable(module="io_scene_gltf2")
    except Exception:
        pass

    # --- select all for export, apply transforms ---
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.export_scene.gltf(
        filepath=out,
        export_format="GLB",
        use_selection=False,
        export_materials="EXPORT",
        export_apply=True,
    )
    size = os.path.getsize(out) if os.path.isfile(out) else 0
    if not size:
        print("[NAAD] FAIL: export produced no file at %s" % out)
        sys.exit(1)
    print("[NAAD] OK: exported %s (%d bytes)" % (out, size))
    print("[NAAD] objects: %s" % sorted(o.name for o in bpy.data.objects))


if __name__ == "__main__":
    main()
