"""One humanoid rig for every CHR_ asset, so Unity retargets them all once.

Bone names are Mixamo's without the `:` separator (`mixamorigHips`,
`mixamorigLeftArm`, ...). That is not cosmetic: Unity's built-in humanoid mapper
recognises Mixamo names, so `GateCAssetBuilder` can set `animationType = Human`
on the copied FBX and still get a *valid* avatar. Renaming a bone here re-opens
Phase C1 of Gate C.

Layout follows the kit convention: metres, +Z up, feet on Z = 0, front faces -Y.
"""

import bpy
from mathutils import Vector

HUMANOID_HEIGHT = 1.72  # floor to top of head, metres
HIP_Z = 1.00

# suffix -> (head, tail, parent) in metres at HUMANOID_HEIGHT. Joints are shared
# with Body() below, so the skin and the bones cannot drift apart.
JOINTS = {
    "hips": (0, 0, 1.00),
    "chest": (0, 0, 1.34),
    "neck": (0, 0, 1.44),
    "head": (0, 0, 1.52),
    "head_top": (0, 0, 1.70),
    "shoulder_l": (0.150, 0, 1.42),
    "elbow_l": (0.175, 0, 1.16),
    "wrist_l": (0.180, 0, 0.92),
    "hand_end_l": (0.180, 0, 0.82),
    "shoulder_r": (-0.150, 0, 1.42),
    "elbow_r": (-0.175, 0, 1.16),
    "wrist_r": (-0.180, 0, 0.92),
    "hand_end_r": (-0.180, 0, 0.82),
    "hip_l": (0.085, 0, 0.98),
    "knee_l": (0.085, 0, 0.54),
    "ankle_l": (0.085, 0, 0.10),
    "toe_l": (0.085, -0.14, 0.03),
    "hip_r": (-0.085, 0, 0.98),
    "knee_r": (-0.085, 0, 0.54),
    "ankle_r": (-0.085, 0, 0.10),
    "toe_r": (-0.085, -0.14, 0.03),
}

# suffix -> (head joint, tail joint, parent). "Root" is synthesised below.
_BONES = (
    ("Hips", "hips", "chest", "Root"),
    ("Spine", "hips", "chest", "Hips"),
    ("Spine1", "hips", "chest", "Spine"),
    ("Spine2", "chest", "neck", "Spine1"),
    ("Neck", "neck", "head", "Spine2"),
    ("Head", "head", "head_top", "Neck"),
    ("LeftShoulder", "neck", "shoulder_l", "Spine2"),
    ("LeftArm", "shoulder_l", "elbow_l", "LeftShoulder"),
    ("LeftForeArm", "elbow_l", "wrist_l", "LeftArm"),
    ("LeftHand", "wrist_l", "hand_end_l", "LeftForeArm"),
    ("RightShoulder", "neck", "shoulder_r", "Spine2"),
    ("RightArm", "shoulder_r", "elbow_r", "RightShoulder"),
    ("RightForeArm", "elbow_r", "wrist_r", "RightArm"),
    ("RightHand", "wrist_r", "hand_end_r", "RightForeArm"),
    ("LeftUpLeg", "hip_l", "knee_l", "Hips"),
    ("LeftLeg", "knee_l", "ankle_l", "LeftUpLeg"),
    ("LeftFoot", "ankle_l", "toe_l", "LeftLeg"),
    ("LeftToeBase", "toe_l", "toe_l", "LeftFoot"),
    ("RightUpLeg", "hip_r", "knee_r", "Hips"),
    ("RightLeg", "knee_r", "ankle_r", "RightUpLeg"),
    ("RightFoot", "ankle_r", "toe_r", "RightLeg"),
    ("RightToeBase", "toe_r", "toe_r", "RightFoot"),
)

DEFORM = tuple(suffix for suffix, *_ in _BONES)


def bone_name(suffix, digits=""):
    return "mixamorig%s%s" % (digits, suffix)


def joint(key, height=HUMANOID_HEIGHT, digits=""):
    """World-space joint position for a character of `height` metres."""
    s = height / HUMANOID_HEIGHT
    x, y, z = JOINTS[key]
    return Vector((x * s, y * s, z * s))


def build_armature(collection, name, digits="", height=HUMANOID_HEIGHT):
    """Create `<name>_Rig` in `collection` (the asset collection) and return it."""
    s = height / HUMANOID_HEIGHT
    arm_data = bpy.data.armatures.new(name + "_RigData")
    armature = bpy.data.objects.new(name + "_Rig", arm_data)
    collection.objects.link(armature)
    armature.show_in_front = True

    bpy.ops.object.select_all(action="DESELECT")
    armature.select_set(True)
    bpy.context.view_layer.objects.active = armature
    bpy.ops.object.mode_set(mode="EDIT")

    root = arm_data.edit_bones.new(bone_name("Root", digits))
    root.head = Vector((0, 0, 0))
    root.tail = Vector((0, 0, 0.12))
    root.use_deform = False

    edit = {"Root": root}
    for suffix, head, tail, parent in _BONES:
        bone = arm_data.edit_bones.new(bone_name(suffix, digits))
        bone.head = Vector(JOINTS[head]) * s
        bone.tail = Vector(JOINTS[tail]) * s
        bone.use_deform = True
        edit[suffix] = bone
    for suffix, _head, _tail, parent in _BONES:
        edit[suffix].parent = edit[parent]

    bpy.ops.object.mode_set(mode="OBJECT")
    armature.select_set(False)
    for pose_bone in armature.pose.bones:
        pose_bone.rotation_mode = "QUATERNION"
    armature["naad_rig"] = "mixamo-flat/v1"
    armature["naad_height"] = round(height, 3)
    return armature


__all__ = ["build_armature", "bone_name", "joint", "HUMANOID_HEIGHT", "HIP_Z",
           "DEFORM", "JOINTS"]


# ---------------------------------------------------------------------------
# Skinned body
# ---------------------------------------------------------------------------

class Body(object):
    """Block-built humanoid body, skinned automatically and parented to the rig.

    Vertices are weighted from their position along the matching bone chain, so
    the mesh deforms without any weight painting. Parts stay separate objects on
    purpose: one SkinnedMeshRenderer/SkinnedMesh per material slot keeps the GLB
    tiny (one draw call per slot) and lets `CharacterAsset` recolour a character
    by swapping materials only.
    """

    def __init__(self, kit, armature, digits=""):
        self.kit = kit
        self.armature = armature
        self.digits = digits
        self.objects = []

    # -- weights ----------------------------------------------------------
    @staticmethod
    def _blend(value, lo, hi, bone_lo, bone_hi):
        """Smoothstep between two bones over [lo, hi]. Works for any axis."""
        if value <= lo:
            return [(bone_lo, 1.0)]
        if value >= hi:
            return [(bone_hi, 1.0)]
        t = (value - lo) / max(hi - lo, 1e-6)
        t = t * t * (3.0 - 2.0 * t)
        return [(bone_lo, 1.0 - t), (bone_hi, t)]

    def _finish(self, obj, weights):
        # Weight in world space: the primitives are placed in world coords, so bake
        # the object transform first and vertex.co *is* the world coordinate.
        bpy.ops.object.select_all(action="DESELECT")
        obj.select_set(True)
        bpy.context.view_layer.objects.active = obj
        bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
        obj.select_set(False)

        per_bone = {}
        for vert in obj.data.vertices:
            for bone, weight in weights(Vector(vert.co)):
                if weight > 1e-4:
                    per_bone.setdefault(bone, []).append((vert.index, weight))
        for suffix, pairs in per_bone.items():
            name = bone_name(suffix, self.digits)
            group = obj.vertex_groups.get(name) or obj.vertex_groups.new(name=name)
            for index, weight in pairs:
                group.add([index], min(weight, 1.0), "REPLACE")

        modifier = obj.modifiers.new("NAAD_Armature", "ARMATURE")
        modifier.object = self.armature
        obj.parent = self.armature
        obj.matrix_parent_inverse = self.armature.matrix_world.inverted()
        for poly in obj.data.polygons:
            poly.use_smooth = False
        self.objects.append(obj)
        return obj

    def segment(self, part, head, tail, thickness, material, weights, taper=1.0,
                depth=None, bevel=0.012):
        """Box spanning two joints, rotated to the bone direction."""
        head = Vector(head)
        tail = Vector(tail)
        axis = tail - head
        rot = axis.to_track_quat("Z", "Y").to_euler()
        size = (thickness, depth if depth is not None else thickness * taper,
                max(axis.length, 0.02))
        obj = self.kit.box(part, tuple((head + tail) * 0.5), size, material,
                           rot=tuple(rot), bevel=bevel)
        return self._finish(obj, weights)

    # -- parts ------------------------------------------------------------
    def torso(self, shirt, height=HUMANOID_HEIGHT, waist=0.30):
        """Chamfered chest + matching pelvis, weighted by height along the spine.

        Values below are for 1.72 m and scale with `height`; a wide box reads as
        a wardrobe at one-third character scale, so the chest tapers at the top.
        """
        s = height / HUMANOID_HEIGHT
        hips = joint("hips", height)
        neck = joint("neck", height)
        w = waist * s

        def weights(co):
            z = co.z
            if z <= hips.z - 0.12 * s:
                return [("Hips", 1.0)]
            if z <= hips.z + 0.06 * s:
                return self._blend(z, hips.z - 0.12 * s, hips.z + 0.06 * s,
                                   "Hips", "Spine")
            if z <= hips.z + 0.22 * s:
                return self._blend(z, hips.z + 0.06 * s, hips.z + 0.22 * s,
                                   "Spine", "Spine1")
            return self._blend(z, hips.z + 0.22 * s, neck.z, "Spine1", "Spine2")

        pelvis = self.kit.box("Pelvis", (0, 0, hips.z - 0.01 * s),
                              (w, 0.20 * s, 0.26 * s), shirt, bevel=0.025)
        self._finish(pelvis, weights)
        # Lower chest (belly to pecs) then shoulder yoke: two boxes, one gradient.
        chest = self.kit.box("Chest", (0, 0, hips.z + 0.20 * s),
                             (w * 1.02, 0.205 * s, 0.30 * s), shirt, bevel=0.03)
        self._finish(chest, weights)
        yoke = self.kit.sphere("Shoulders", (0, 0, hips.z + 0.36 * s), 1.0, shirt,
                               segments=10, rings=4)
        yoke.scale = (w * 0.62, 0.105 * s, 0.085 * s)
        bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
        self._finish(yoke, weights)

        return self

    def head(self, skin, height=HUMANOID_HEIGHT):
        neck_j = joint("neck", height)
        head_j = joint("head", height)
        top = joint("head_top", height)

        neck = self.kit.cyl("Neck", (0, 0, neck_j.z), 0.05, 0.10, skin, verts=10)
        self._finish(neck, lambda co: self._blend(co.z, neck_j.z - 0.05, head_j.z,
                                                  "Spine2", "Neck"))
        head = self.kit.box("Head", (0, 0, (head_j.z + top.z) * 0.5),
                            (0.165, 0.19, top.z - head_j.z), skin, bevel=0.035)
        self._finish(head, lambda co: [("Head", 1.0)])
        return self

    def hair(self, material, style="short", height=HUMANOID_HEIGHT):
        s = height / HUMANOID_HEIGHT
        top = joint("head_top", height)
        head_only = lambda co: [("Head", 1.0)]  # noqa: E731
        cap = self.kit.box("Hair", (0, 0.008 * s, top.z - 0.035 * s),
                           (0.178, 0.196, 0.085 * s), material, bevel=0.03)
        self._finish(cap, head_only)
        if style == "bun":
            self._finish(self.kit.sphere("HairBun", (0, 0.105 * s, top.z - 0.015 * s),
                                         0.055 * s, material, segments=10, rings=5),
                         head_only)
        elif style == "braids":
            for i, x in enumerate((-0.055, 0.0, 0.055)):
                self._finish(self.kit.box("Braid_%d" % (i + 1),
                                          (x * s, 0.10 * s, top.z - 0.22 * s),
                                          (0.028, 0.03, 0.28 * s), material), head_only)
        elif style == "long":
            self._finish(self.kit.box("HairBack", (0, 0.098 * s, top.z - 0.24 * s),
                                      (0.168, 0.05, 0.28 * s), material), head_only)
        elif style == "braid":
            self._finish(self.kit.box("HairPlaits", (0, 0.10 * s, top.z - 0.22 * s),
                                      (0.15, 0.045, 0.26 * s), material), head_only)
        elif style == "cap":
            self._finish(self.kit.cyl("CapCrown", (0, 0, top.z - 0.01 * s), 0.098 * s,
                                      0.075 * s, material, verts=14), head_only)
            self._finish(self.kit.box("CapPeak", (0, -0.115 * s, top.z - 0.03 * s),
                                      (0.16, 0.10, 0.016), material), head_only)
        elif style == "wrap":
            self._finish(self.kit.cyl("WrapBand", (0, 0, top.z - 0.03 * s), 0.098 * s,
                                      0.07 * s, material, verts=14), head_only)
            self._finish(self.kit.sphere("WrapTop", (0, 0.01 * s, top.z + 0.02 * s),
                                         0.072 * s, material, segments=10, rings=5,
                                         scale=(1, 1, 0.6)), head_only)
        return self

    def arms(self, sleeve, skin, height=HUMANOID_HEIGHT, covered=0.55, phone=None):
        """`covered` = fraction of the upper arm the sleeve covers (0 = vest)."""
        for sign, tag in ((1.0, "Left"), (-1.0, "Right")):
            key = "l" if sign > 0 else "r"
            shoulder = joint("shoulder_%s" % key, height)
            elbow = joint("elbow_%s" % key, height)
            wrist = joint("wrist_%s" % key, height)
            hand_end = joint("hand_end_%s" % key, height)

            upper = lambda co, tag=tag, shoulder=shoulder, elbow=elbow: \
                self._blend(-co.z, -shoulder.z, -elbow.z, "%sArm" % tag,
                            "%sForeArm" % tag)  # noqa: E731
            lower = lambda co, tag=tag, elbow=elbow, wrist=wrist: \
                self._blend(-co.z, -elbow.z, -wrist.z, "%sForeArm" % tag,
                            "%sHand" % tag)  # noqa: E731
            hand = lambda co, tag=tag: [("%sHand" % tag, 1.0)]  # noqa: E731

            elbow_pt = shoulder.lerp(elbow, max(0.0, min(1.0, covered)))
            if covered > 0.0:
                self.segment("%sSleeve" % tag, shoulder, elbow_pt, 0.085, sleeve,
                             upper, taper=0.95)
            if covered < 1.0:
                self.segment("%sUpperArm" % tag, elbow_pt, elbow, 0.072, skin, upper)
            self.segment("%sForeArm" % tag, elbow, wrist, 0.064, skin, lower,
                         taper=0.9)
            self._finish(self.kit.box("%sHand" % tag,
                                      tuple(wrist.lerp(hand_end, 0.55)),
                                      (0.070, 0.062, 0.10), skin, bevel=0.018), hand)
        if phone is not None:
            # Parented to the hand so the PhoneHold clip carries it.
            right = joint("wrist_r", height)
            self._finish(self.kit.box("Phone", (right.x, right.y - 0.05, right.z - 0.06),
                                      (0.062, 0.012, 0.11), phone, rot=(0.35, 0, 0)),
                         lambda co: [("RightHand", 1.0)])
        return self

    def legs(self, trousers, shoes, height=HUMANOID_HEIGHT, shin=None, thickness=0.115):
        for sign, tag in ((1.0, "Left"), (-1.0, "Right")):
            key = "l" if sign > 0 else "r"
            hip = joint("hip_%s" % key, height)
            knee = joint("knee_%s" % key, height)
            ankle = joint("ankle_%s" % key, height)
            toe = joint("toe_%s" % key, height)

            thigh_w = lambda co, tag=tag, hip=hip, knee=knee: \
                self._blend(-co.z, -hip.z, -knee.z, "%sUpLeg" % tag,
                            "%sLeg" % tag)  # noqa: E731
            shin_w = lambda co, tag=tag, knee=knee, ankle=ankle: \
                self._blend(-co.z, -knee.z, -ankle.z, "%sLeg" % tag,
                            "%sFoot" % tag)  # noqa: E731
            foot_w = lambda co, tag=tag: [("%sFoot" % tag, 1.0)]  # noqa: E731

            self.segment("%sThigh" % tag, hip, knee, thickness, trousers, thigh_w,
                         taper=0.86, bevel=0.018)
            self.segment("%sShin" % tag, knee, ankle, thickness * 0.82,
                         shin if shin is not None else trousers, shin_w, taper=0.88,
                         bevel=0.014)
            foot_mid = ankle.lerp(toe, 0.45)
            self._finish(self.kit.box("%sShoe" % tag,
                                      (foot_mid.x, foot_mid.y - 0.03, 0.05),
                                      (0.10, 0.27, 0.095), shoes, bevel=0.02), foot_w)
        return self


def repose(armature):
    """Relax the A-pose so the still reads as a standing person.

    Pose-only: the FBX is exported straight after, and the exporter applies this
    while the bind pose (Unity's avatar) stays the rest pose.
    """
    for sign, tag in ((1.0, "Left"), (-1.0, "Right")):
        for suffix, euler in (("%sArm" % tag, (0.0, sign * -0.10, 0.0)),
                              ("%sForeArm" % tag, (-0.18, 0.0, 0.0)),
                              ("%sUpLeg" % tag, (0.0, 0.0, 0.03 * sign))):
            bone = armature.pose.bones.get(bone_name(suffix))
            if bone is not None:
                bone.rotation_mode = "XYZ"
                bone.rotation_euler = euler
    return armature
