"""Priority 3 - Nightclub (Club 57), Victoria Island.

A Lagos nightclub is neon, bass, and VIP rope. The props are sized for a
player-height eye line (1.62 m), so the bar reads waist-high and the DJ booth
clears head height. Every asset faces -Y.
"""

import math

from naad_spec import AssetSpec


# ---------------------------------------------------------------------------
# PRP_Bar_NCL_v001 - L-shaped bar, 2.5 x 1.2 m, bottle rail, taps
# ---------------------------------------------------------------------------
def bar(kit):
    p = kit.p
    # Main counter with bottle rail
    kit.box("Top", (0, 0, 1.05), (2.50, 1.20, 0.08), p["steel"], bevel=0.015)
    kit.box("Front", (0, 0, 0.52), (2.50, 0.10, 1.05), p["canvas_red"], bevel=0.01)
    kit.box("Rail", (0, 0.45, 1.10), (2.40, 0.08, 0.12), p["alu"], bevel=0.008)
    # Bottle line on the rail
    for i in range(8):
        x = -1.05 + i * 0.30
        kit.cyl("Bottle_%d" % (i + 1), (x, 0.45, 1.20), 0.035, 0.22,
                p["glass_dark"] if i % 2 else p["glass"], verts=8)
        kit.cyl("Cap_%d" % (i + 1), (x, 0.45, 1.32), 0.02, 0.03, p["steel"], verts=8)
    # Underbar storage
    kit.box("Shelf", (0, 0, 0.35), (2.30, 0.40, 0.04), p["steel"])
    kit.box("GlassRack", (0, 0.20, 0.65), (1.80, 0.06, 0.25), p["alu"])
    for i in range(5):
        kit.cyl("Glass_%d" % (i + 1), (-0.60 + i * 0.30, 0.20, 0.72), 0.04, 0.12,
                p["glass"], verts=8)


# ---------------------------------------------------------------------------
# PRP_Booth_NCL_v001 - VIP booth, 1.8 x 1.2 m, velvet bench, table
# ---------------------------------------------------------------------------
def booth(kit):
    p = kit.p
    # U-shaped bench
    kit.box("BenchBack", (0, 0.45, 0.45), (1.80, 0.35, 0.45), p["fabric_plum"], bevel=0.02)
    kit.box("BenchL", (-0.70, 0, 0.45), (0.35, 0.80, 0.45), p["fabric_plum"], bevel=0.02)
    kit.box("BenchR", (0.70, 0, 0.45), (0.35, 0.80, 0.45), p["fabric_plum"], bevel=0.02)
    # Table
    kit.box("TableTop", (0, 0, 0.72), (1.20, 0.70, 0.04), p["glass"], bevel=0.01)
    kit.box("TableBase", (0, 0, 0.36), (0.60, 0.35, 0.70), p["steel"], bevel=0.015)
    # Rope stanchions
    for x in (-0.90, 0.90):
        kit.cyl("Post_%s" % ("L" if x < 0 else "R"), (x, -0.60, 0.45), 0.04, 0.90,
                p["brass"], verts=8)
    kit.box("Rope", (0, -0.60, 0.85), (1.80, 0.02, 0.02), p["neon_pink"])


# ---------------------------------------------------------------------------
# PRP_DJBooth_NCL_v001 - raised DJ platform, decks, mixer
# ---------------------------------------------------------------------------
def dj_booth(kit):
    p = kit.p
    # Platform
    kit.box("Platform", (0, 0, 0.30), (1.40, 0.90, 0.60), p["black"], bevel=0.01)
    kit.box("Railing", (0, 0, 0.75), (1.40, 0.04, 0.15), p["steel"])
    # Decks
    for i, x in enumerate((-0.35, 0.35)):
        kit.box("Deck_%d" % (i + 1), (x, 0, 0.95), (0.35, 0.30, 0.08), p["black"])
        kit.cyl("Platter_%d" % (i + 1), (x, 0, 0.99), 0.12, 0.02, p["alu"], verts=16)
    # Mixer
    kit.box("Mixer", (0, 0, 0.95), (0.40, 0.25, 0.06), p["black"])
    for i in range(4):
        kit.cyl("Knob_%d" % (i + 1), (-0.12 + i * 0.08, 0, 0.99), 0.015, 0.02,
                p["alu"], verts=8)
    # Monitors
    for x in (-0.55, 0.55):
        kit.box("Monitor_%s" % ("L" if x < 0 else "R"), (x, -0.30, 1.05),
                (0.25, 0.20, 0.35), p["black"], bevel=0.01)
        kit.box("Screen_%s" % ("L" if x < 0 else "R"), (x, -0.30, 1.15),
                (0.20, 0.02, 0.25), p["screen"])


# ---------------------------------------------------------------------------
# PRP_Speaker_NCL_v001 - PA stack, 0.6 x 0.4 m, dual drivers
# ---------------------------------------------------------------------------
def speaker(kit):
    p = kit.p
    kit.box("Cabinet", (0, 0, 0.60), (0.60, 0.40, 1.20), p["black"], bevel=0.015)
    kit.cyl("Woofer", (0, 0, 0.45), 0.22, 0.08, p["black"], verts=16)
    kit.cyl("WooferCone", (0, 0, 0.45), 0.18, 0.06, p["black"], verts=16)
    kit.cyl("Tweeter", (0, 0, 0.85), 0.08, 0.04, p["alu"], verts=12)
    kit.box("Grille", (0, 0.20, 0.45), (0.50, 0.02, 0.50), p["steel"])
    kit.box("GrilleTop", (0, 0.20, 0.85), (0.20, 0.02, 0.18), p["steel"])


# ---------------------------------------------------------------------------
# PRP_LightBeam_NCL_v001 - moving head spotlight, 0.3 m tall
# ---------------------------------------------------------------------------
def light_beam(kit):
    p = kit.p
    kit.box("Base", (0, 0, 0.12), (0.22, 0.22, 0.24), p["black"], bevel=0.01)
    kit.box("Yoke", (0, 0, 0.30), (0.16, 0.08, 0.20), p["black"])
    kit.box("Head", (0, 0, 0.45), (0.18, 0.25, 0.20), p["black"], bevel=0.01)
    kit.cyl("Lens", (0, 0.13, 0.45), 0.08, 0.04, p["glass"], verts=12)
    kit.box("LensRing", (0, 0.13, 0.45), (0.18, 0.02, 0.18), p["alu"])
    # The beam itself (emissive cylinder)
    kit.cyl("Beam", (0, 0.50, 0.45), 0.06, 1.20, p["bulb_cool"],
            rot=(math.pi / 2, 0, 0), verts=8)


# ---------------------------------------------------------------------------
# PRP_Stool_NCL_v001 - bar stool, 0.75 m seat, chrome base
# ---------------------------------------------------------------------------
def stool(kit):
    p = kit.p
    kit.cyl("Seat", (0, 0, 0.75), 0.22, 0.05, p["fabric_teal"], verts=12)
    kit.cyl("SeatRim", (0, 0, 0.73), 0.21, 0.03, p["alu"], verts=12)
    kit.cyl("Stem", (0, 0, 0.38), 0.025, 0.70, p["steel"], verts=8)
    kit.box("Foot", (0, 0, 0.03), (0.35, 0.35, 0.04), p["steel"], bevel=0.01)


# ---------------------------------------------------------------------------
# PRP_Table_NCL_v001 - high cocktail table, 0.9 m top
# ---------------------------------------------------------------------------
def table(kit):
    p = kit.p
    kit.cyl("Top", (0, 0, 0.90), 0.40, 0.04, p["glass"], verts=14)
    kit.cyl("TopRim", (0, 0, 0.88), 0.38, 0.03, p["alu"], verts=14)
    kit.cyl("Column", (0, 0, 0.45), 0.04, 0.85, p["steel"], verts=8)
    kit.box("Base", (0, 0, 0.03), (0.50, 0.50, 0.04), p["steel"], bevel=0.01)
    # Cocktail glass
    kit.cyl("Glass", (0.10, 0.05, 0.95), 0.04, 0.12, p["glass"], verts=8)
    kit.cyl("Liquid", (0.10, 0.05, 0.92), 0.035, 0.08, p["neon_cyan"], verts=8)


# ---------------------------------------------------------------------------
# PRP_Coaster_NCL_v001 - neon coaster, 0.1 m diameter
# ---------------------------------------------------------------------------
def coaster(kit):
    p = kit.p
    kit.cyl("Base", (0, 0, 0.01), 0.06, 0.02, p["black"], verts=12)
    kit.cyl("Ring", (0, 0, 0.02), 0.05, 0.015, p["neon_pink"], verts=12)


# ---------------------------------------------------------------------------
# PRP_Bottle_NCL_v001 - premium bottle, 0.3 m tall
# ---------------------------------------------------------------------------
def bottle(kit):
    p = kit.p
    kit.cyl("Body", (0, 0, 0.15), 0.06, 0.22, p["glass_dark"], verts=10)
    kit.cyl("Neck", (0, 0, 0.30), 0.025, 0.08, p["glass_dark"], verts=8)
    kit.cyl("Cap", (0, 0, 0.35), 0.03, 0.04, p["brass"], verts=8)
    kit.box("Label", (0, 0, 0.18), (0.08, 0.01, 0.12), p["plastic_white"])


# ---------------------------------------------------------------------------
# PRP_NeonSign_NCL_v001 - club sign, 1.2 x 0.4 m
# ---------------------------------------------------------------------------
def neon_sign(kit):
    p = kit.p
    kit.box("Back", (0, 0, 0.60), (1.20, 0.08, 0.40), p["black"], bevel=0.01)
    kit.box("Text", (0, 0, 0.62), (1.10, 0.02, 0.30), p["neon_pink"])
    kit.box("Border", (0, 0, 0.60), (1.18, 0.02, 0.38), p["neon_cyan"])
    kit.box("Frame", (0, 0, 0.60), (1.22, 0.04, 0.42), p["steel"], bevel=0.008)


# ---------------------------------------------------------------------------
# NCL_Club57_NCL_v001 - the whole club shell: floor, walls, ceiling, dance floor
# ---------------------------------------------------------------------------
def club_shell(kit):
    p = kit.p
    W, D = 12.0, 16.0
    # Floor
    kit.box("Floor", (0, 0, -0.02), (W, D, 0.04), p["concrete_light"])
    # Dance floor (central, reflective)
    kit.box("DanceFloor", (0, 0, 0.02), (6.0, 8.0, 0.04), p["glass"], bevel=0.01)
    # Walls
    kit.box("WallBack", (0, D / 2 - 0.06, 3.0), (W, 0.12, 6.0), p["concrete"], bevel=0.01)
    kit.box("WallFront", (0, -D / 2 + 0.06, 3.0), (W, 0.12, 6.0), p["concrete"], bevel=0.01)
    kit.box("WallLeft", (-W / 2 + 0.06, 0, 3.0), (0.12, D, 6.0), p["concrete"], bevel=0.01)
    kit.box("WallRight", (W / 2 - 0.06, 0, 3.0), (0.12, D, 6.0), p["concrete"], bevel=0.01)
    # Ceiling with acoustic panels
    kit.box("Ceiling", (0, 0, 5.98), (W, D, 0.12), p["concrete"], bevel=0.01)
    for i in range(6):
        for j in range(8):
            x = -W / 2 + 0.5 + i * 2.0
            y = -D / 2 + 0.5 + j * 2.0
            kit.box("Panel_%d_%d" % (i + 1, j + 1), (x, y, 5.90),
                    (1.8, 1.8, 0.06), p["fabric_dark"])
    # DJ booth platform (raised)
    kit.box("DJPlatform", (0, -D / 2 + 2.0, 0.30), (3.0, 2.0, 0.60), p["black"], bevel=0.01)
    # VIP booths
    for x in (-4.0, 4.0):
        kit.box("BoothFloor_%s" % ("L" if x < 0 else "R"), (x, D / 2 - 2.0, 0.15),
                (2.5, 2.0, 0.30), p["wood_dark"], bevel=0.01)
    # Bar along one wall
    kit.box("BarCounter", (W / 2 - 1.5, 0, 1.05), (2.5, 0.8, 0.10), p["steel"], bevel=0.01)
    # Speaker stacks
    for x in (-W / 2 + 1.0, W / 2 - 1.0):
        kit.box("SpeakerStack_%s" % ("L" if x < 0 else "R"), (x, -D / 2 + 1.0, 0.60),
                (0.60, 0.40, 1.20), p["black"], bevel=0.015)
    # Neon strip along ceiling
    kit.box("NeonStrip", (0, 0, 5.85), (W - 0.5, 0.04, 0.04), p["neon_cyan"])


SPECS = [
    AssetSpec("PRP_Bar_NCL_v001", (2.50, 1.20, 1.32), bar,
              note="L-shaped bar with bottle rail"),
    AssetSpec("PRP_Booth_NCL_v001", (1.80, 1.20, 0.90), booth,
              note="VIP booth with velvet bench"),
    AssetSpec("PRP_DJBooth_NCL_v001", (1.40, 0.90, 1.20), dj_booth,
              note="raised DJ platform with decks"),
    AssetSpec("PRP_Speaker_NCL_v001", (0.60, 0.40, 1.20), speaker,
              note="PA stack with dual drivers"),
    AssetSpec("PRP_LightBeam_NCL_v001", (0.30, 1.70, 1.65), light_beam,
              note="moving head spotlight"),
    AssetSpec("PRP_Stool_NCL_v001", (0.35, 0.35, 0.80), stool,
              note="bar stool with chrome base"),
    AssetSpec("PRP_Table_NCL_v001", (0.50, 0.50, 0.95), table,
              note="high cocktail table"),
    AssetSpec("PRP_Coaster_NCL_v001", (0.10, 0.10, 0.04), coaster,
              note="neon coaster"),
    AssetSpec("PRP_Bottle_NCL_v001", (0.12, 0.12, 0.36), bottle,
              note="premium bottle"),
    AssetSpec("PRP_NeonSign_NCL_v001", (1.20, 0.08, 0.42), neon_sign,
              note="club neon sign"),
    AssetSpec("NCL_Club57_NCL_v001", (12.0, 16.0, 6.0), club_shell,
              note="12x16m club shell: floor, walls, ceiling, dance floor"),
]

