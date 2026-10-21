"""Priority 3 - Beach (Coconut Grove), Lekki.

A Lagos beach is sand, palm trees, and ocean breeze. The props are sized for a
player-height eye line (1.62 m), so the umbrellas clear head height and the chairs
read as lounge furniture. Every asset faces -Y.
"""

import math

from naad_spec import AssetSpec


# ---------------------------------------------------------------------------
# PRP_Umbrella_BCH_v001 - beach umbrella, 2.5 m diameter, striped
# ---------------------------------------------------------------------------
def umbrella(kit):
    p = kit.p
    R = 1.25
    kit.cyl("Pole", (0, 0, 1.25), 0.04, 2.50, p["wood_light"], verts=8)
    kit.cone("Canopy", (0, 0, 2.50), R, 0.06, 0.45, p["canvas_red"], verts=16)
    # Striped pattern
    for i in range(8):
        a = i * math.pi / 4
        if i % 2 == 0:
            kit.cone("Stripe_%d" % (i + 1), (0, 0, 2.48), R * 0.92, 0.06, 0.43,
                     p["canvas_green"], verts=16)
    kit.sphere("Finial", (0, 0, 2.95), 0.05, p["wood_light"], segments=8, rings=5)
    for i in range(8):
        a = i * math.pi / 4
        kit.box("Rib_%d" % (i + 1),
                (math.cos(a) * R * 0.5, math.sin(a) * R * 0.5, 2.50),
                (R * 1.02, 0.02, 0.02), p["wood_light"],
                rot=(0.32 * math.sin(a), -0.32 * math.cos(a), a))


# ---------------------------------------------------------------------------
# PRP_LoungeChair_BCH_v001 - reclining beach chair, 0.6 m wide
# ---------------------------------------------------------------------------
def lounge_chair(kit):
    p = kit.p
    # Frame
    kit.box("Seat", (0, 0, 0.35), (0.60, 1.20, 0.08), p["plastic_white"], bevel=0.015)
    kit.box("Back", (0, -0.55, 0.70), (0.60, 0.08, 0.70), p["plastic_white"], bevel=0.015)
    kit.box("LegFront", (0, 0.55, 0.18), (0.55, 0.06, 0.35), p["plastic_white"], bevel=0.01)
    kit.box("LegBack", (0, -0.55, 0.18), (0.55, 0.06, 0.35), p["plastic_white"], bevel=0.01)
    # Fabric
    kit.box("Fabric", (0, 0, 0.40), (0.52, 1.10, 0.04), p["fabric_cream"], bevel=0.01)
    kit.box("FabricBack", (0, -0.55, 0.75), (0.52, 0.04, 0.60), p["fabric_cream"], bevel=0.01)
    # Armrests
    for x in (-0.30, 0.30):
        kit.box("Armrest_%s" % ("L" if x < 0 else "R"), (x, 0, 0.50),
                (0.08, 1.00, 0.06), p["plastic_white"], bevel=0.01)


# ---------------------------------------------------------------------------
# PRP_Cooler_BCH_v001 - beach cooler, 0.4 m cube
# ---------------------------------------------------------------------------
def cooler(kit):
    p = kit.p
    kit.box("Body", (0, 0, 0.20), (0.40, 0.40, 0.40), p["plastic_blue"], bevel=0.02)
    kit.box("Lid", (0, 0.02, 0.42), (0.42, 0.42, 0.05), p["plastic_white"], bevel=0.015)
    kit.box("Handle", (0, -0.22, 0.30), (0.20, 0.03, 0.04), p["steel"])
    kit.box("Drain", (0, 0.18, 0.02), (0.08, 0.04, 0.02), p["black"])


# ---------------------------------------------------------------------------
# PRP_PalmTree_BCH_v001 - coconut palm, 4 m tall
# ---------------------------------------------------------------------------
def palm_tree(kit):
    p = kit.p
    # Trunk
    kit.cyl("Trunk", (0, 0, 2.0), 0.25, 4.0, p["bark"], verts=12)
    # Fronds
    for i in range(8):
        angle = i * math.pi / 4
        dx = math.cos(angle) * 0.3
        dy = math.sin(angle) * 0.3
        kit.box("Frond_%d" % (i + 1), (dx, dy, 3.8),
                (0.08, 2.5, 0.04), p["leaf"],
                rot=(0.4, 0, angle + math.pi / 2))
    # Coconuts
    for i in range(3):
        angle = i * math.pi * 2 / 3
        kit.sphere("Coconut_%d" % (i + 1),
                   (math.cos(angle) * 0.15, math.sin(angle) * 0.15, 3.6),
                   0.08, p["wood_dark"], segments=8, rings=5)


# ---------------------------------------------------------------------------
# PRP_SandCastle_BCH_v001 - small sand castle, 0.5 m wide
# ---------------------------------------------------------------------------
def sand_castle(kit):
    p = kit.p
    kit.box("Base", (0, 0, 0.10), (0.50, 0.50, 0.20), p["sand"], bevel=0.02)
    kit.box("Tower_L", (-0.12, 0, 0.25), (0.18, 0.18, 0.35), p["sand"], bevel=0.015)
    kit.box("Tower_R", (0.12, 0, 0.25), (0.18, 0.18, 0.35), p["sand"], bevel=0.015)
    kit.box("Tower_C", (0, -0.12, 0.25), (0.18, 0.18, 0.30), p["sand"], bevel=0.015)
    kit.cone("Roof_L", (-0.12, 0, 0.45), 0.12, 0.06, 0.15, p["sand_dry"], verts=8)
    kit.cone("Roof_R", (0.12, 0, 0.45), 0.12, 0.06, 0.15, p["sand_dry"], verts=8)
    kit.cone("Roof_C", (0, -0.12, 0.42), 0.12, 0.06, 0.12, p["sand_dry"], verts=8)
    # Moat
    kit.box("Moat", (0, 0.35, 0.02), (0.60, 0.08, 0.04), p["foam"])


# ---------------------------------------------------------------------------
# PRP_Surfboard_BCH_v001 - surfboard, 2.0 m long
# ---------------------------------------------------------------------------
def surfboard(kit):
    p = kit.p
    kit.box("Board", (0, 0, 0.08), (0.50, 2.00, 0.06), p["plastic_yellow"], bevel=0.03)
    kit.box("Nose", (0, -0.95, 0.08), (0.35, 0.15, 0.06), p["plastic_yellow"], bevel=0.03)
    kit.box("Tail", (0, 0.95, 0.08), (0.40, 0.12, 0.06), p["plastic_yellow"], bevel=0.025)
    kit.box("Stripe", (0, 0, 0.09), (0.08, 1.60, 0.02), p["neon_cyan"])
    # Fin
    kit.box("Fin", (0, 0.85, 0.02), (0.04, 0.15, 0.12), p["plastic_red"], bevel=0.01)


# ---------------------------------------------------------------------------
# PRP_BeachBall_BCH_v001 - inflatable beach ball, 0.4 m diameter
# ---------------------------------------------------------------------------
def beach_ball(kit):
    p = kit.p
    kit.sphere("Ball", (0, 0, 0.20), 0.20, p["plastic_red"], segments=12, rings=8)
    # Color bands
    for i in range(4):
        angle = i * math.pi / 2
        dx = math.cos(angle) * 0.18
        dy = math.sin(angle) * 0.18
        kit.sphere("Band_%d" % (i + 1), (dx, dy, 0.20), 0.18,
                   p["plastic_white"] if i % 2 else p["plastic_blue"],
                   segments=10, rings=6, scale=(0.6, 0.6, 1.0))


# ---------------------------------------------------------------------------
# BCH_Shore_BCH_v001 - the whole shore shell: sand, water, palm grove
# ---------------------------------------------------------------------------
def shore_shell(kit):
    p = kit.p
    W, D = 20.0, 30.0
    # Sand beach
    kit.box("Sand", (0, 5.0, -0.02), (W, D * 0.6, 0.08), p["sand"], bevel=0.05)
    # Wet sand near water
    kit.box("WetSand", (0, -D * 0.1, -0.02), (W, D * 0.25, 0.06), p["sand_dry"], bevel=0.03)
    # Ocean water
    kit.box("Water", (0, -D * 0.25, -0.05), (W, D * 0.35, 0.10), p["glass"], bevel=0.02)
    # Foam line
    kit.box("Foam", (0, -D * 0.12, 0.02), (W, 0.15, 0.04), p["foam"], bevel=0.01)
    # Palm grove
    for i in range(6):
        for j in range(3):
            x = -W / 2 + 2.0 + i * 3.5
            y = D * 0.35 + j * 4.0
            kit.cyl("Palm_%d_%d" % (i + 1, j + 1), (x, y, 2.0), 0.25, 4.0,
                    p["bark"], verts=12)
            # Fronds for each palm
            for k in range(8):
                angle = k * math.pi / 4
                dx = math.cos(angle) * 0.3
                dy = math.sin(angle) * 0.3
                kit.box("Frond_%d_%d_%d" % (i + 1, j + 1, k + 1),
                        (x + dx, y + dy, 3.8),
                        (0.08, 2.5, 0.04), p["leaf"],
                        rot=(0.4, 0, angle + math.pi / 2))
    # Beach umbrellas scattered
    for x, y in ((-5.0, 8.0), (5.0, 6.0), (0, 12.0), (-3.0, 15.0), (4.0, 10.0)):
        kit.cyl("UmbrellaPole_%s_%s" % (x, y), (x, y, 1.25), 0.04, 2.50,
                p["wood_light"], verts=8)
        kit.cone("UmbrellaCanopy_%s_%s" % (x, y), (x, y, 2.50), 1.25, 0.06, 0.45,
                 p["canvas_red"], verts=16)
    # Lounge chairs
    for x, y in ((-4.0, 5.0), (3.0, 4.0), (-2.0, 7.0), (5.0, 8.0)):
        kit.box("Lounge_%s_%s" % (x, y), (x, y, 0.35), (0.60, 1.20, 0.08),
                p["plastic_white"], bevel=0.015)


SPECS = [
    AssetSpec("PRP_Umbrella_BCH_v001", (2.50, 2.50, 2.95), umbrella,
              note="beach umbrella, striped"),
    AssetSpec("PRP_LoungeChair_BCH_v001", (0.60, 1.20, 0.80), lounge_chair,
              note="reclining beach chair"),
    AssetSpec("PRP_Cooler_BCH_v001", (0.40, 0.40, 0.47), cooler,
              note="beach cooler"),
    AssetSpec("PRP_PalmTree_BCH_v001", (1.0, 1.0, 4.0), palm_tree,
              note="coconut palm"),
    AssetSpec("PRP_SandCastle_BCH_v001", (0.50, 0.50, 0.60), sand_castle,
              note="small sand castle"),
    AssetSpec("PRP_Surfboard_BCH_v001", (0.50, 2.00, 0.14), surfboard,
              note="surfboard"),
    AssetSpec("PRP_BeachBall_BCH_v001", (0.40, 0.40, 0.40), beach_ball,
              note="inflatable beach ball"),
    AssetSpec("BCH_Shore_BCH_v001", (20.0, 30.0, 4.0), shore_shell,
              note="20x30m shore: sand, water, palm grove"),
]

