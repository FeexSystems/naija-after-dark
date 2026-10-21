"""NAAD procedural asset kit for Blender (bpy).

Shared primitives used by every builder in `build_assets.py`. No bpy-free logic
lives here on purpose: this module is only imported inside Blender or with the
`bpy` pip module installed.

Conventions every builder must follow (they are asserted by
`tools/blender-bpy/verify_assets.py`):

* Units are metres, +Z up, asset *front* faces -Y, floor at Z = 0.
* The collection name is the NAAD asset name, e.g. `PRP_Booth_ND_v001`.
* Mesh objects are `<AssetName>_<Part>`, empties `<AssetName>_Anchor_<Name>`.
* Everything is low-poly (default budget 2.5k tris) with flat/faceted shading.
* Materials are flat Principled BSDFs from `naad_materials.palette()`; no textures.
* Humanoid assets (`AssetSpec.humanoid`) keep their armature and are *not* joined;
  their meshes are parented to `spec.armature` and exported with it.
"""

import math

import bmesh
import bpy
from mathutils import Euler, Vector

SCALE_TOLERANCE = 0.12  # intended size vs measured bounds, relative
PALETTE = {}


# ---------------------------------------------------------------------------
# Scene / collection
# ---------------------------------------------------------------------------

def reset_scene():
    global PALETTE
    bpy.ops.wm.read_factory_settings(use_empty=True)
    for block in (bpy.data.meshes, bpy.data.materials, bpy.data.collections):
        for item in list(block):
            try:
                block.remove(item)
            except Exception:
                pass


def new_collection(name):
    coll = bpy.data.collections.new(name)
    bpy.context.scene.collection.children.link(coll)
    return coll


# ---------------------------------------------------------------------------
# Materials
# ---------------------------------------------------------------------------

_MATERIALS = {}


def mat(name, color, metallic=0.0, roughness=0.7, alpha=1.0, emission=None,
        emission_strength=0.0):
    """Create (or reuse) a flat Principled material."""
    key = (name, color, metallic, roughness, alpha, emission, emission_strength)
    cached = _MATERIALS.get(key)
    if cached is not None:
        try:
            if cached.name:  # .name on a removed ID datablock raises ReferenceError
                return cached
        except ReferenceError:
            _MATERIALS.pop(key, None)

    material = bpy.data.materials.new(name=name)
    material.use_nodes = True
    bsdf = material.node_tree.nodes.get("Principled BSDF")
    if bsdf:
        bsdf.inputs["Base Color"].default_value = (*color, 1.0)
        if "Metallic" in bsdf.inputs:
            bsdf.inputs["Metallic"].default_value = metallic
        if "Roughness" in bsdf.inputs:
            bsdf.inputs["Roughness"].default_value = roughness
        if alpha < 1.0 and "Alpha" in bsdf.inputs:
            bsdf.inputs["Alpha"].default_value = alpha
            if hasattr(material, "blend_method"):
                try:
                    material.blend_method = "BLEND"
                except Exception:
                    pass
            elif hasattr(material, "surface_render_method"):
                material.surface_render_method = "BLENDED"
        if emission is not None:
            for prop in ("Emission Color", "Emission"):
                if prop in bsdf.inputs:
                    bsdf.inputs[prop].default_value = (*emission, 1.0)
                    break
            if "Emission Strength" in bsdf.inputs:
                bsdf.inputs["Emission Strength"].default_value = emission_strength
    _MATERIALS[key] = material
    return material


# ---------------------------------------------------------------------------
# Mesh helpers
# ---------------------------------------------------------------------------

class Kit:
    """Accumulates parts of one asset into a single collection."""

    def __init__(self, name, collection=None, palette=None, spec=None):
        self.name = name
        self.collection = collection or new_collection(name)
        self.parts = []
        self.spec = spec
        if palette is not None:
            self.p = palette
        elif PALETTE:
            self.p = PALETTE
        else:
            import naad_materials

            self.p = naad_materials.palette()

    # -- registration -----------------------------------------------------
    def _link(self, obj, material, bevel):
        for other in list(obj.users_collection):
            other.objects.unlink(obj)
        self.collection.objects.link(obj)
        if material is not None:
            obj.data.materials.append(material)
        if bevel and bevel > 0:
            mod = obj.modifiers.new("NAAD_Bevel", "BEVEL")
            mod.width = bevel
            mod.segments = 1
            mod.limit_method = "ANGLE"
        self.parts.append(obj)
        return obj

    def _part_name(self, part):
        return "%s_%s" % (self.name, part)

    # -- primitives -------------------------------------------------------
    def box(self, part, loc, size, material=None, rot=(0, 0, 0), bevel=0.0):
        """Axis aligned (then rotated) box. `size` is full extents."""
        bpy.ops.mesh.primitive_cube_add(size=1.0, location=loc)
        obj = bpy.context.active_object
        obj.name = self._part_name(part)
        obj.scale = size
        obj.rotation_euler = Euler(rot)
        bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
        return self._link(obj, material, bevel)

    def cyl(self, part, loc, radius, depth, material=None, rot=(0, 0, 0), verts=12,
            bevel=0.0):
        bpy.ops.mesh.primitive_cylinder_add(
            vertices=verts, radius=radius, depth=depth, location=loc)
        obj = bpy.context.active_object
        obj.name = self._part_name(part)
        obj.rotation_euler = Euler(rot)
        bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
        return self._link(obj, material, bevel)

    def cone(self, part, loc, r1, r2, depth, material=None, rot=(0, 0, 0), verts=12):
        bpy.ops.mesh.primitive_cone_add(
            vertices=verts, radius1=r1, radius2=r2, depth=depth, location=loc)
        obj = bpy.context.active_object
        obj.name = self._part_name(part)
        obj.rotation_euler = Euler(rot)
        bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
        return self._link(obj, material, 0.0)

    def sphere(self, part, loc, radius, material=None, segments=12, rings=6,
               scale=(1, 1, 1)):
        bpy.ops.mesh.primitive_uv_sphere_add(
            segments=segments, ring_count=rings, radius=radius, location=loc)
        obj = bpy.context.active_object
        obj.name = self._part_name(part)
        obj.scale = scale
        bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
        return self._link(obj, material, 0.0)

    def torus(self, part, loc, major, minor, material=None, rot=(0, 0, 0),
              major_seg=16, minor_seg=6):
        bpy.ops.mesh.primitive_torus_add(
            major_radius=major, minor_radius=minor, location=loc,
            major_segments=major_seg, minor_segments=minor_seg)
        obj = bpy.context.active_object
        obj.name = self._part_name(part)
        obj.rotation_euler = Euler(rot)
        bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
        return self._link(obj, material, 0.0)

    # -- composite helpers ------------------------------------------------
    def legs(self, part, corners, height, thickness, material, bevel=0.0):
        """Four vertical legs at (x, y) corner pairs."""
        for i, (x, y) in enumerate(corners):
            self.box("%s_%d" % (part, i + 1), (x, y, height / 2.0),
                     (thickness, thickness, height), material, bevel=bevel)

    def slats(self, part, count, axis, start, step, loc, size, material, rot=(0, 0, 0)):
        """Repeat `count` slabs along one axis - pallets, shelves, planks."""
        for i in range(count):
            offset = [0.0, 0.0, 0.0]
            offset["xyz".index(axis)] = start + step * i
            self.box("%s_%d" % (part, i + 1),
                     (loc[0] + offset[0], loc[1] + offset[1], loc[2] + offset[2]),
                     size, material, rot=rot)

    def railing(self, part, length, height, material, posts=4, y=0.0, x0=None,
                bar_radius=0.03):
        """Horizontal rail with vertical posts, running along X."""
        x0 = -length / 2.0 if x0 is None else x0
        for i in range(posts):
            x = x0 + (length / float(max(posts - 1, 1))) * i
            self.cyl("%s_Post_%d" % (part, i + 1), (x, y, height / 2.0),
                     bar_radius, height, material, verts=8)
        self.cyl("%s_Rail" % part, (x0 + length / 2.0, y, height),
                 bar_radius * 1.1, length, material, rot=(0, math.pi / 2, 0), verts=8)

    def anchor(self, name, loc):
        empty = bpy.data.objects.new("%s_Anchor_%s" % (self.name, name), None)
        empty.location = Vector(loc)
        empty.empty_display_size = 0.15
        self.collection.objects.link(empty)
        return empty

    # -- finish -----------------------------------------------------------
    def join(self):
        """Join every part into one mesh object named after the asset."""
        if not self.parts:
            return None
        bpy.ops.object.select_all(action="DESELECT")
        meshes = [p for p in self.parts if p.type == "MESH"]
        if not meshes:
            return None
        for obj in meshes:
            obj.select_set(True)
            if obj.mode != "OBJECT":
                bpy.ops.object.mode_set(mode="OBJECT")
        bpy.context.view_layer.objects.active = meshes[0]
        bpy.ops.object.join()
        joined = bpy.context.active_object
        joined.name = self.name
        joined.data.name = self.name + "_Mesh"

        # `bpy.ops.mesh.primitive_*_add` leaves its "last operator size" on the new
        # object, which `join` carries over as a non-1 scale on the active object.
        # Baked into the mesh here so the GLB/FBX never ship a scaled root.
        joined.scale = (1.0, 1.0, 1.0)

        # Apply bevels + weld by distance, then flat-shade for the low-poly look.
        for mod in list(joined.modifiers):
            try:
                bpy.ops.object.modifier_apply(modifier=mod.name)
            except Exception:
                joined.modifiers.remove(mod)
        me = joined.data
        bm = bmesh.new()
        bm.from_mesh(me)
        bmesh.ops.remove_doubles(bm, verts=bm.verts, dist=0.0005)
        bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
        bm.to_mesh(me)
        bm.free()
        for poly in me.polygons:
            poly.use_smooth = False
        self.parts = [joined]
        return joined


# ---------------------------------------------------------------------------
# Measurement / export
# ---------------------------------------------------------------------------

def collection_bounds(collection):
    lo = Vector((1e9, 1e9, 1e9))
    hi = Vector((-1e9, -1e9, -1e9))
    found = False
    for obj in collection.objects:
        if obj.type != "MESH":
            continue
        found = True
        for corner in obj.bound_box:
            world = obj.matrix_world @ Vector(corner)
            for i in range(3):
                lo[i] = min(lo[i], world[i])
                hi[i] = max(hi[i], world[i])
    if not found:
        return Vector((0, 0, 0)), Vector((0, 0, 0))
    return lo, hi


def tri_count(collection):
    total = 0
    for obj in collection.objects:
        if obj.type != "MESH":
            continue
        me = obj.data
        me.calc_loop_triangles()
        total += len(me.loop_triangles)
    return total


def options(args):
    """Parse the shared build switches (everything after `--` when run in Blender).

    Returns (out_root, preview). `out_root` is None when the caller has no
    explicit output directory, i.e. `build_assets.py` decides.
    """
    if "--" in args:
        args = args[args.index("--") + 1:]
    else:
        args = args[1:]
    out_root = None
    preview = True
    i = 0
    while i < len(args):
        arg = args[i]
        if arg == "--out" and i + 1 < len(args):
            out_root = args[i + 1]
            i += 2
            continue
        if arg in ("--only", "--category", "--jobs") and i + 1 < len(args):
            i += 2  # consumed by build_assets.parse_args, not by the kit
            continue
        if arg == "--no-preview":
            preview = False
        i += 1
    return out_root, preview


def fbx_props(asset, kind="PRP", venue="STR"):
    """Tag the asset root with NAAD metadata before export.

    Blender's FBX exporter does not write custom properties, so this is metadata
    for anyone re-opening the .blend/scene, not something Unity reads. Unity gets
    the same information from the file name (the NAAD naming grammar).
    """
    try:
        name = asset.name if hasattr(asset, "name") else str(asset)
        asset["naad_asset"] = name
        asset["naad_category"] = kind
        asset["naad_venue"] = venue
        asset["naad_units"] = "metres"
    except Exception:
        pass
    return asset


def export(args, out_dir, name, object_types=("MESH", "EMPTY")):
    """Export the current scene as `<name>.fbx` + `<name>.glb` into `out_dir`.

    Settings mirror `tools/unity-asset-pipeline/naad_addon/scene.py` (the in-Blender
    add-on) so the headless and dialog exports land in Unity identically.
    """
    import os

    explicit_out, _ = options(args)
    out_dir = explicit_out or out_dir
    os.makedirs(out_dir, exist_ok=True)

    glb = os.path.join(out_dir, name + ".glb")
    fbx = os.path.join(out_dir, name + ".fbx")

    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.export_scene.gltf(
        filepath=glb,
        export_format="GLB",
        use_selection=False,
        export_materials="EXPORT",
        export_apply=True,
        export_yup=True,
    )
    bpy.ops.export_scene.fbx(
        filepath=fbx,
        use_selection=False,
        object_types=set(object_types),
        use_mesh_modifiers=True,
        mesh_smooth_type="FACE",
        bake_space_transform=True,
        # Blender is metres/-Z forward/Y up, Unity is centimetres/-Z forward/Y up.
        # Scene units in the FBX say "cm", so Unity imports 1 Blender unit = 1 m.
        apply_scale_options="FBX_SCALE_ALL",
        bake_anim=bool(True),
        path_mode="AUTO",
    )
    return glb, fbx


def build(spec, args, out_dir):
    """Run a builder spec and export. Returns a result dict (printed as JSON)."""
    import json
    import os
    import time

    start = time.time()
    import naad_materials

    global PALETTE
    explicit_out, preview_wanted = options(args)
    out_root = explicit_out or out_dir

    reset_scene()
    PALETTE = naad_materials.palette()
    collection = new_collection(spec.name)
    kit = Kit(spec.name, collection, spec=spec)
    spec.build(kit)

    lo, hi = collection_bounds(collection)
    measured = hi - lo
    tris = tri_count(collection)
    specs = spec.specs

    if not spec.humanoid:
        kit.join()
    if spec.armature is not None:
        for obj in list(collection.objects):
            if obj.type == "MESH":
                obj.parent = spec.armature
                obj.matrix_parent_inverse = spec.armature.matrix_world.inverted()
    # Tag whatever represents the asset for anyone re-opening the file.
    tag = spec.armature or (collection.objects[0] if len(collection.objects) else None)
    if tag is not None:
        fbx_props(tag, spec.name.split("_")[0], spec.name.split("_")[2])

    png = None
    if preview_wanted:
        try:
            # Rendered after export-time state is captured; render_preview adds a
            # ground plane + lights, so nothing is exported that the preview needs.
            png = render_preview(collection, spec.name, out_root)
        except Exception as err:  # a failed preview must never lose the model
            print("[NAAD] WARN preview failed for %s: %s" % (spec.name, err))

    object_types = ("MESH", "EMPTY", "ARMATURE") if spec.humanoid else ("MESH", "EMPTY")
    glb, fbx = export(args, out_dir, spec.name, object_types)
    glb_size = os.path.getsize(glb) if os.path.isfile(glb) else 0
    fbx_size = os.path.getsize(fbx) if os.path.isfile(fbx) else 0
    if not glb_size or not fbx_size:
        raise RuntimeError("export produced empty files for %s" % spec.name)

    report = {
        "name": spec.name,
        "category": spec.name.split("_")[0],
        "district": spec.district,
        "glb": os.path.basename(glb),
        "fbx": os.path.basename(fbx),
        "glbBytes": glb_size,
        "fbxBytes": fbx_size,
        "tris": tris,
        "intendedXZY": [round(v, 3) for v in specs],
        "measuredXZY": [round(v, 3) for v in measured],
        "boundsContainIntended": all(m >= i * (1.0 - SCALE_TOLERANCE)
                                    for m, i in zip(measured, specs)),
        "objects": len(collection.objects),
        "preview": png,
        "humanoid": bool(spec.humanoid),
        "seconds": round(time.time() - start, 2),
    }
    print("[NAAD] BUILD %s" % json.dumps(report))
    return report


# ---------------------------------------------------------------------------
# Preview render (visual QA, never part of the Unity import)
# ---------------------------------------------------------------------------

def _scene_lights(rig):
    world = bpy.data.worlds.new("NAAD_Preview_World")
    world.use_nodes = True
    background = world.node_tree.nodes.get("Background")
    if background:
        background.inputs["Color"].default_value = (0.035, 0.040, 0.060, 1.0)
        background.inputs["Strength"].default_value = 1.0
    bpy.context.scene.world = world

    def area(name, loc, energy, size, color=(1.0, 0.95, 0.88)):
        light = bpy.data.lights.new(name, "AREA")
        light.energy = energy
        light.size = size
        light.color = color
        obj = bpy.data.objects.new(name, light)
        obj.location = loc
        direction = Vector((loc[0], loc[1], loc[2])) * -1.0
        obj.rotation_euler = direction.to_track_quat("-Z", "Y").to_euler()
        rig.objects.link(obj)

    area("Key", (6, -8, 9), 2600, 7.0)
    area("Fill", (-9, -4, 4), 900, 8.0, (0.55, 0.70, 1.0))
    area("Rim", (2, 9, 6), 1500, 6.0, (1.0, 0.55, 0.35))


def render_preview(collection, name, out_root):
    """Render a 3/4 view PNG to <out_root>/_previews/<Name>.png.

    Runs *before* the export and links everything it adds (ground, lights, camera)
    to a throw-away `_PreviewRig` collection, so `export(..., use_selection=False)`
    still only writes the asset.
    """
    import math
    import os

    lo, hi = collection_bounds(collection)
    center = (lo + hi) * 0.5
    size = hi - lo
    radius = max(size.length * 0.5, 0.5)

    # Ground plane so shadows read and the object does not float.
    rig = new_collection("_PreviewRig")
    bpy.ops.mesh.primitive_plane_add(size=radius * 12, location=(0, 0, lo.z - 0.002))
    ground = bpy.context.active_object
    ground.name = "_PreviewGround"
    ground.data.materials.append(mat("MAT_Preview_Ground", (0.035, 0.035, 0.04)))
    for other in list(ground.users_collection):
        other.objects.unlink(ground)
    rig.objects.link(ground)

    _scene_lights(rig)

    cam_data = bpy.data.cameras.new("PreviewCam")
    cam_data.lens = 50
    cam = bpy.data.objects.new("PreviewCam", cam_data)
    rig.objects.link(cam)
    direction = Vector((0.85, -1.0, 0.62)).normalized()
    # Tall assets (characters, lamp posts) are cropped by a 50 mm lens on a 4:3
    # frame; back the camera off until the largest extent fits with margin.
    fit = max(size.x, size.y, size.z) * 1.3
    half_fov = math.atan(0.5 * 24.0 / cam_data.lens)  # sensor 36 mm, 4:3 -> 24 mm tall
    distance = max(radius * 3.1, (fit * 0.5) / math.tan(half_fov))
    cam.location = center + direction * distance
    cam.rotation_euler = (center - cam.location).to_track_quat("-Z", "Y").to_euler()
    bpy.context.scene.camera = cam

    scene = bpy.context.scene
    # EEVEE is GPU-only on some CI hosts; Cycles CPU always renders headless.
    try:
        scene.render.engine = "BLENDER_EEVEE_NEXT"
    except TypeError:
        try:
            scene.render.engine = "BLENDER_EEVEE"
        except TypeError:
            scene.render.engine = "CYCLES"
    scene.render.resolution_x = 640
    scene.render.resolution_y = 480
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"
    scene.render.film_transparent = False
    if hasattr(scene, "eevee"):
        try:
            scene.eevee.taa_render_samples = 16
        except Exception:
            pass

    out_dir = os.path.join(out_root, "_previews")
    os.makedirs(out_dir, exist_ok=True)
    path = os.path.join(out_dir, name + ".png")
    scene.render.filepath = path
    try:
        bpy.ops.render.render(write_still=True)
    finally:
        # Detach the preview rig again: it must never be exported.
        scene.camera = None
        for obj in list(rig.objects):
            data = obj.data
            bpy.data.objects.remove(obj, do_unlink=True)
            if data is not None and data.users == 0:
                block = getattr(bpy.data, "lights" if hasattr(data, "energy") else "meshes", None)
                if block is not None and block.get(getattr(data, "name", "")) is data:
                    block.remove(data)
        bpy.data.collections.remove(rig)
    return "_previews/" + name + ".png" if os.path.isfile(path) else None
