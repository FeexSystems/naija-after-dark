"""Asset registry primitives: `AssetSpec` and small shared helpers."""

import math


class AssetSpec(object):
    """One NAAD asset: name, intended footprint, builder function.

    `specs` is the intended (X, Y, Z) envelope in metres; `verify_assets.py`
    asserts the measured bounds fit it (within `naad_kit.SCALE_TOLERANCE`).
    `humanoid` assets keep an armature (`armature` is set by the builder) and are
    exported unjoined for a Unity Humanoid Avatar import.
    """

    def __init__(self, name, specs, build, humanoid=False, district=None, note=""):
        self.name = name
        self.specs = tuple(float(v) for v in specs)   # (X, Y, Z) metres
        self.build = build
        self.humanoid = humanoid
        self.district = district or name.split("_")[2]
        self.note = note
        self.armature = None   # set by the builder for rigged assets

    @property
    def part(self):
        return self.name.split("_")[1]

    def __repr__(self):
        return "<AssetSpec %s %s>" % (self.name, self.specs)


def hair(kit, p, z, color, style="short", w=0.20):
    """Hair cap + optional bun/braids. Cheap and readable at game distance."""
    kit.sphere("Hair", (0, 0.008, z + 0.015), 0.115, color,
               segments=12, rings=6, scale=(1.0, 1.02, 0.75))
    if style == "bun":
        kit.sphere("HairBun", (0, 0.115, z + 0.03), 0.065, color, segments=10, rings=5)
    elif style == "braids":
        for i, x in enumerate((-0.075, -0.025, 0.025, 0.075)):
            kit.box("HairBraid_%d" % (i + 1), (x, 0.125, z - 0.10), (0.030, 0.030, 0.24),
                    color)
    elif style == "wrap":
        kit.torus("HairWrap", (0, 0.01, z + 0.02), 0.105, 0.030, color,
                  rot=(0, 0, 0), major_seg=16, minor_seg=6)
        kit.sphere("HairWrapHigh", (0, 0.02, z + 0.10), 0.085, color,
                   segments=10, rings=5, scale=(1.0, 1.0, 0.7))
    elif style == "long":
        kit.box("HairBack", (0, 0.115, z - 0.11), (0.19, 0.045, 0.26), color)
    elif style == "cap":
        kit.cyl("CapCrown", (0, 0.005, z + 0.045), 0.112, 0.09, color, verts=14)
        kit.box("CapPeak", (0, -0.105, z + 0.02), (0.19, 0.11, 0.018), color)
    return None


def simple_legs(kit, p, hip_z=0.50, foot_z=0.06, x=0.10, top_mat=None, bottom_mat=None):
    top_mat = top_mat or p["fabric_dark"]
    bottom_mat = bottom_mat or top_mat
    for i, sx in enumerate((-x, x)):
        kit.cyl("Thigh_%d" % (i + 1), (sx, 0, hip_z - 0.12), 0.078, 0.40, bottom_mat,
                verts=10)
        kit.cyl("Shin_%d" % (i + 1), (sx, 0, 0.24), 0.062, 0.38, top_mat, verts=10)
        kit.box("Shoe_%d" % (i + 1), (sx, -0.045, foot_z - 0.02), (0.11, 0.26, 0.09),
                p["black"], bevel=0.015)


def simple_arms(kit, p, shoulder_z, mat, skin, hand_z=None, inward=0.0):
    hand_z = hand_z if hand_z is not None else shoulder_z - 0.52
    for i, sx in enumerate((-0.255, 0.255)):
        kit.cyl("UpperArm_%d" % (i + 1), (sx, 0, shoulder_z - 0.14), 0.060, 0.30, mat,
                verts=10)
        kit.cyl("Forearm_%d" % (i + 1), (sx - inward, 0, shoulder_z - 0.42), 0.050, 0.28,
                skin, verts=10)
        kit.sphere("Hand_%d" % (i + 1), (sx - inward, 0, hand_z), 0.055, skin,
                   segments=8, rings=5)


def neon_tube(kit, part, loc, size, material, rot=(0, 0, 0)):
    return kit.box(part, loc, size, material, rot=rot)


def angle_rad(deg):
    return math.radians(deg)
