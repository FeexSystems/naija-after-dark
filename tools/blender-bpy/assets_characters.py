"""Priority 1 - Characters (player + NPCs), Lagos.

All characters share the Mixamo-compatible humanoid rig from `naad_humanoid.py`.
Each is a block-built body skinned automatically, exported as FBX for Unity's
Humanoid Avatar system. Heights vary from 1.55 m to 1.80 m.
"""

from naad_spec import AssetSpec
from naad_humanoid import build_armature, Body, repose, HUMANOID_HEIGHT


# ---------------------------------------------------------------------------
# CHR_Player_STD_v001 - player character, 1.72 m, casual wear
# ---------------------------------------------------------------------------
def player(kit):
    p = kit.p
    armature = build_armature(kit.collection, "Player", height=HUMANOID_HEIGHT)
    body = Body(kit, armature)
    body.torso(p["cloth_khaki"], height=HUMANOID_HEIGHT, waist=0.32)
    body.head(p["skin_3"], height=HUMANOID_HEIGHT)
    body.hair(p["hair_black"], style="short", height=HUMANOID_HEIGHT)
    body.arms(p["cloth_khaki"], p["skin_3"], height=HUMANOID_HEIGHT, covered=0.60)
    body.legs(p["cloth_navy"], p["black"], height=HUMANOID_HEIGHT)
    repose(armature)
    kit.spec.armature = armature


# ---------------------------------------------------------------------------
# CHR_MamaSeyi_ML_v001 - suya vendor, 1.62 m, head wrap
# ---------------------------------------------------------------------------
def mama_seyi(kit):
    p = kit.p
    height = 1.62
    armature = build_armature(kit.collection, "MamaSeyi", height=height)
    body = Body(kit, armature)
    body.torso(p["canvas_red"], height=height, waist=0.34)
    body.head(p["skin_2"], height=height)
    body.hair(p["cloth_gold"], style="wrap", height=height)
    body.arms(p["canvas_red"], p["skin_2"], height=height, covered=0.70)
    body.legs(p["fabric_orange"], p["black"], height=height)
    repose(armature)
    kit.spec.armature = armature


# ---------------------------------------------------------------------------
# CHR_Mallam_ML_v001 - suya grill man, 1.75 m, cap
# ---------------------------------------------------------------------------
def mallam(kit):
    p = kit.p
    height = 1.75
    armature = build_armature(kit.collection, "Mallam", height=height)
    body = Body(kit, armature)
    body.torso(p["cloth_olive"], height=height, waist=0.30)
    body.head(p["skin_1"], height=height)
    body.hair(p["hair_black"], style="cap", height=height)
    body.arms(p["cloth_olive"], p["skin_1"], height=height, covered=0.65)
    body.legs(p["fabric_dark"], p["black"], height=height)
    repose(armature)
    kit.spec.armature = armature


# ---------------------------------------------------------------------------
# CHR_DJ_NCL_v001 - nightclub DJ, 1.70 m, headphones
# ---------------------------------------------------------------------------
def dj(kit):
    p = kit.p
    height = 1.70
    armature = build_armature(kit.collection, "DJ", height=height)
    body = Body(kit, armature)
    body.torso(p["black"], height=height, waist=0.28)
    body.head(p["skin_4"], height=height)
    body.hair(p["hair_black"], style="bun", height=height)
    body.arms(p["black"], p["skin_4"], height=height, covered=0.50)
    body.legs(p["fabric_dark"], p["black"], height=height)
    # Headphones (parented to head)
    kit.box("HeadphoneL", (-0.095, 0, 1.52), (0.04, 0.06, 0.12), p["black"], bevel=0.01)
    kit.box("HeadphoneR", (0.095, 0, 1.52), (0.04, 0.06, 0.12), p["black"], bevel=0.01)
    kit.box("Headband", (0, 0.05, 1.58), (0.25, 0.02, 0.02), p["black"])
    repose(armature)
    kit.spec.armature = armature


# ---------------------------------------------------------------------------
# CHR_Bouncer_NCL_v001 - club bouncer, 1.85 m, suit
# ---------------------------------------------------------------------------
def bouncer(kit):
    p = kit.p
    height = 1.85
    armature = build_armature(kit.collection, "Bouncer", height=height)
    body = Body(kit, armature)
    body.torso(p["black"], height=height, waist=0.36)
    body.head(p["skin_1"], height=height)
    body.hair(p["hair_black"], style="short", height=height)
    body.arms(p["black"], p["skin_1"], height=height, covered=0.80)
    body.legs(p["black"], p["black"], height=height)
    repose(armature)
    kit.spec.armature = armature


# ---------------------------------------------------------------------------
# CHR_BeachVendor_BCH_v001 - beach vendor, 1.60 m, braids
# ---------------------------------------------------------------------------
def beach_vendor(kit):
    p = kit.p
    height = 1.60
    armature = build_armature(kit.collection, "BeachVendor", height=height)
    body = Body(kit, armature)
    body.torso(p["fabric_teal"], height=height, waist=0.32)
    body.head(p["skin_2"], height=height)
    body.hair(p["hair_black"], style="braids", height=height)
    body.arms(p["fabric_teal"], p["skin_2"], height=height, covered=0.55)
    body.legs(p["cloth_khaki"], p["black"], height=height)
    repose(armature)
    kit.spec.armature = armature


# ---------------------------------------------------------------------------
# CHR_Crowd01_STR_v001 - crowd member 1, 1.68 m, t-shirt
# ---------------------------------------------------------------------------
def crowd_01(kit):
    p = kit.p
    height = 1.68
    armature = build_armature(kit.collection, "Crowd" + "01", height=height)
    body = Body(kit, armature)
    body.torso(p["plastic_blue"], height=height, waist=0.30)
    body.head(p["skin_3"], height=height)
    body.hair(p["hair_black"], style="short", height=height)
    body.arms(p["plastic_blue"], p["skin_3"], height=height, covered=0.40)
    body.legs(p["fabric_dark"], p["black"], height=height)
    repose(armature)
    kit.spec.armature = armature


# ---------------------------------------------------------------------------
# CHR_Crowd02_STR_v001 - crowd member 2, 1.65 m, long hair
# ---------------------------------------------------------------------------
def crowd_02(kit):
    p = kit.p
    height = 1.65
    armature = build_armature(kit.collection, "Crowd" + "02", height=height)
    body = Body(kit, armature)
    body.torso(p["fabric_cream"], height=height, waist=0.30)
    body.head(p["skin_4"], height=height)
    body.hair(p["hair_black"], style="long", height=height)
    body.arms(p["fabric_cream"], p["skin_4"], height=height, covered=0.45)
    body.legs(p["fabric_orange"], p["black"], height=height)
    repose(armature)
    kit.spec.armature = armature


# ---------------------------------------------------------------------------
# CHR_Crowd03_STR_v001 - crowd member 3, 1.72 m, cap
# ---------------------------------------------------------------------------
def crowd_03(kit):
    p = kit.p
    height = 1.72
    armature = build_armature(kit.collection, "Crowd" + "03", height=height)
    body = Body(kit, armature)
    body.torso(p["cloth_navy"], height=height, waist=0.32)
    body.head(p["skin_2"], height=height)
    body.hair(p["hair_black"], style="cap", height=height)
    body.arms(p["cloth_navy"], p["skin_2"], height=height, covered=0.55)
    body.legs(p["fabric_dark"], p["black"], height=height)
    repose(armature)
    kit.spec.armature = armature


SPECS = [
    AssetSpec("CHR_Player_STD_v001", (0.60, 0.30, 1.72), player,
              humanoid=True, district="STD", note="player character"),
    AssetSpec("CHR_MamaSeyi_ML_v001", (0.55, 0.34, 1.62), mama_seyi,
              humanoid=True, district="ML", note="suya vendor"),
    AssetSpec("CHR_Mallam_ML_v001", (0.55, 0.30, 1.75), mallam,
              humanoid=True, district="ML", note="suya grill man"),
    AssetSpec("CHR_DJ_NCL_v001", (0.55, 0.28, 1.70), dj,
              humanoid=True, district="NCL", note="nightclub DJ"),
    AssetSpec("CHR_Bouncer_NCL_v001", (0.60, 0.36, 1.85), bouncer,
              humanoid=True, district="NCL", note="club bouncer"),
    AssetSpec("CHR_BeachVendor_BCH_v001", (0.55, 0.32, 1.60), beach_vendor,
              humanoid=True, district="BCH", note="beach vendor"),
    AssetSpec("CHR_Crowd01_STR_v001", (0.55, 0.30, 1.68), crowd_01,
              humanoid=True, district="STR", note="crowd member 1"),
    AssetSpec("CHR_Crowd02_STR_v001", (0.55, 0.30, 1.65), crowd_02,
              humanoid=True, district="STR", note="crowd member 2"),
    AssetSpec("CHR_Crowd03_STR_v001", (0.55, 0.32, 1.72), crowd_03,
              humanoid=True, district="STR", note="crowd member 3"),
]

