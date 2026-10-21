"""Priority 4 - Suya spot (Mallam street food), Suya / Mainland Streets.

A suya spot is a man, a glowing charcoal grill under an umbrella, a folding table
of plastic chairs and a cooler of malt. The props are sized for a *player-height*
eye line (Mama Seyi is 1.62 m), so the counter reads waist-high and the umbrella
clears head height. Every asset faces -Y.
"""

import math

from naad_spec import AssetSpec


# ---------------------------------------------------------------------------
# PRP_Grill_ML_v001 - trough grill on legs, 1.4 x 0.55 m, hot coals + skewers
# ---------------------------------------------------------------------------
def grill(kit):
    p = kit.p
    # Trough: sheet body, brick-lined firebox, then live coals.
    kit.box("Trough", (0, 0, 0.86), (1.40, 0.55, 0.26), p["steel"], bevel=0.02)
    kit.box("TroughLiner", (0, 0, 0.97), (1.26, 0.43, 0.05), p["charcoal"])
    kit.box("AshBed", (0, 0, 0.945), (1.22, 0.40, 0.03), p["rust"])
    for i in range(11):
        x = -0.54 + i * 0.108
        kit.box("Coal_%d" % (i + 1), (x, -0.10 + (i % 3) * 0.10, 0.975),
                (0.09, 0.09, 0.045), p["fire"] if i % 3 else p["charcoal"],
                rot=(0, 0, i * 0.4), bevel=0.015)
    # Grill bars over the coals.
    for i in range(7):
        kit.box("Bar_%d" % (i + 1), (-0.60 + i * 0.20, 0, 1.01), (0.025, 0.55, 0.015),
                p["metal"])
    # Skewers of suya: thin sticks with a chunk cluster each. Chunks sit one
    # bar-gap above the coals and are narrower than the coal spacing so the
    # meat and the fire never fuse into one red mass.
    for i in range(6):
        x = -0.50 + i * 0.20
        kit.box("Skewer_%d" % (i + 1), (x, 0, 1.055), (0.012, 0.52, 0.012), p["wood_light"])
        for j in range(4):
            kit.box("Meat_%d_%d" % (i + 1, j + 1), (x, -0.20 + j * 0.10, 1.075),
                    (0.04, 0.06, 0.055), p["canvas_red"], bevel=0.012)
    # Legs, shelf, and the fuel bucket underneath.
    for x in (-0.60, 0.60):
        for y in (-0.20, 0.20):
            kit.box("Leg_%s_%s" % ("L" if x < 0 else "R", "F" if y < 0 else "B"),
                    (x, y, 0.40), (0.05, 0.05, 0.80), p["metal"])
    kit.box("Shelf", (0, 0, 0.28), (1.24, 0.46, 0.04), p["steel"])
    kit.cyl("FuelBucket", (0.34, 0.0, 0.16), 0.15, 0.28, p["charcoal"], verts=12)
    # Trays and the pepper tin on the working end.
    kit.cyl("PepperTin", (-0.58, -0.18, 1.20), 0.07, 0.12, p["plastic_red"], verts=10)
    kit.box("LadleTray", (0.50, -0.20, 1.15), (0.30, 0.14, 0.04), p["steel"])


# ---------------------------------------------------------------------------
# PRP_Table_ML_v001 - folding outdoor table, 0.8 x 0.8 m, 0.74 m top
# ---------------------------------------------------------------------------
def table(kit):
    p = kit.p
    kit.box("Top", (0, 0, 0.72), (0.80, 0.80, 0.04), p["plastic_white"], bevel=0.02)
    kit.box("Rim", (0, 0, 0.69), (0.74, 0.74, 0.03), p["plastic_white"])
    for x in (-0.31, 0.31):
        for y in (-0.31, 0.31):
            kit.box("Leg_%s_%s" % ("L" if x < 0 else "R", "F" if y < 0 else "B"),
                    (x, y, 0.36), (0.035, 0.06, 0.69), p["plastic_white"], bevel=0.01)
    kit.box("UnderRail", (0, 0, 0.12), (0.66, 0.05, 0.04), p["plastic_white"])
    # A table without evidence of eating is just furniture.
    kit.cyl("Bottle", (0.16, -0.10, 0.79), 0.045, 0.20, p["canvas_green"], verts=10)
    kit.cyl("BottleNeck", (0.16, -0.10, 0.91), 0.02, 0.06, p["glass_dark"], verts=8)
    kit.cyl("BottleCap", (0.16, -0.10, 0.95), 0.022, 0.02, p["steel"], verts=8)
    kit.cyl("NapkinHolder", (-0.18, 0.10, 0.76), 0.06, 0.07, p["steel"], verts=10)
    kit.box("Paper_1", (-0.18, 0.10, 0.81), (0.10, 0.10, 0.03), p["plastic_white"],
            rot=(0, 0, 0.3))
    kit.box("Plate", (0.18, 0.20, 0.745), (0.20, 0.20, 0.015), p["plastic_white"],
            bevel=0.06)


# ---------------------------------------------------------------------------
# PRP_Stool_ML_v001 - stackable plastic stool, 0.34 m seat
# ---------------------------------------------------------------------------
def stool(kit):
    p = kit.p
    kit.cyl("Seat", (0, 0, 0.33), 0.19, 0.03, p["plastic_white"], verts=12)
    kit.cyl("SeatLip", (0, 0, 0.315), 0.185, 0.02, p["plastic_white"], verts=12)
    for i in range(4):
        angle = math.pi / 4 + i * math.pi / 2
        # Legs splay outward as they descend: base sits wider than the seat, so
        # the brace ring at 0.13 m actually touches each leg instead of being
        # swallowed inside them.
        top_x = math.cos(angle) * 0.12
        top_y = math.sin(angle) * 0.12
        bot_x = math.cos(angle) * 0.15
        bot_y = math.sin(angle) * 0.15
        kit.cyl("Leg_%d" % (i + 1), ((top_x + bot_x) / 2, (top_y + bot_y) / 2, 0.155),
                0.018, 0.31, p["plastic_white"],
                rot=(0.10 * math.sin(angle), -0.10 * math.cos(angle), 0), verts=8)
    kit.torus("Brace", (0, 0, 0.10), 0.135, 0.012, p["plastic_white"], major_seg=12,
              minor_seg=4)


# ---------------------------------------------------------------------------
# PRP_Fan_ML_v001 - 0.5 m floor fan, wire cage, on a tripod
# ---------------------------------------------------------------------------
def fan(kit):
    p = kit.p
    for i, (x, y) in enumerate(((-0.16, -0.12), (0.16, -0.12), (0.0, 0.18))):
        kit.cyl("StandLeg_%d" % (i + 1), (x, y, 0.20), 0.018, 0.42, p["metal"],
                rot=(0.30 if y < 0 else -0.20, 0, 0.5 * math.sin(i)), verts=6)
    kit.cyl("StandHub", (0, 0, 0.42), 0.06, 0.10, p["metal"], verts=10)
    kit.cyl("Pole", (0, 0, 0.62), 0.022, 0.40, p["metal"], verts=8)
    kit.cyl("Motor", (0, -0.09, 0.84), 0.075, 0.18, p["steel"],
            rot=(math.pi / 2, 0, 0), verts=12)
    kit.torus("CageOuter", (0, -0.20, 0.84), 0.21, 0.012, p["steel"],
              rot=(math.pi / 2, 0, 0), major_seg=16, minor_seg=4)
    for i in range(3):
        kit.torus("CageRing_%d" % (i + 1), (0, -0.20, 0.84), 0.07 + i * 0.06, 0.008,
                  p["steel"], rot=(math.pi / 2, 0, 0), major_seg=14, minor_seg=3)
    # Three blades radiating from the hub, each offset along its own radius so
    # the group reads as a propeller instead of a cross at the centre.
    for i in range(3):
        angle = i * math.pi * 2 / 3
        bx = math.cos(angle) * 0.11
        bz = 0.84 + math.sin(angle) * 0.11
        kit.box("Blade_%d" % (i + 1), (bx, -0.20, bz), (0.22, 0.012, 0.07),
                p["plastic_white"], rot=(0.25, 0, angle))
    kit.cyl("HubCap", (0, -0.22, 0.84), 0.03, 0.04, p["plastic_white"],
            rot=(math.pi / 2, 0, 0), verts=10)
    for i in range(3):
        kit.box("Foot_%d" % (i + 1), (0, 0, 0.02), (0.08, 0.08, 0.03), p["black"],
                rot=(0, 0, i * math.pi * 2 / 3))


# ---------------------------------------------------------------------------
# PRP_Cooler_ML_v001 - 0.55 m drinks cooler with bottles on ice
# ---------------------------------------------------------------------------
def cooler(kit):
    p = kit.p
    kit.box("Body", (0, 0, 0.22), (0.55, 0.36, 0.38), p["plastic_blue"], bevel=0.03)
    kit.box("Lid", (0, 0.02, 0.43), (0.57, 0.38, 0.05), p["plastic_white"], bevel=0.02)
    kit.box("Hinge", (0, 0.19, 0.44), (0.40, 0.03, 0.03), p["steel"])
    kit.box("Handle", (0, -0.20, 0.30), (0.30, 0.03, 0.04), p["steel"])
    for i, x in enumerate((-0.22, 0.22)):
        kit.box("Tread_%d" % (i + 1), (x, 0, 0.03), (0.06, 0.34, 0.05), p["black"])
    kit.box("Sticker", (0, -0.185, 0.26), (0.26, 0.006, 0.12), p["plastic_yellow"])
    # Bottles poking out of the open lid: malt, water, soft drink.
    for i, (x, y, color) in enumerate(((-0.15, -0.05, p["canvas_green"]),
                                       (0.0, 0.04, p["glass"]),
                                       (0.15, -0.03, p["canvas_red"]))):
        kit.cyl("Bottle_%d" % (i + 1), (x, y, 0.52), 0.035, 0.22, color, verts=8)
        kit.cyl("Cap_%d" % (i + 1), (x, y, 0.64), 0.02, 0.03, p["steel"], verts=8)


# ---------------------------------------------------------------------------
# PRP_Skewer_ML_v001 - server plate of suya: skewers, onions, pepper dust
# ---------------------------------------------------------------------------
def skewer(kit):
    p = kit.p
    kit.cyl("Plate", (0, 0, 0.02), 0.18, 0.025, p["plastic_white"], verts=14)
    kit.cyl("Paper", (0, 0, 0.035), 0.16, 0.006, p["linen"], verts=14)
    for i in range(4):
        angle = i * math.pi / 4
        x = math.cos(angle) * 0.03
        y = math.sin(angle) * 0.03
        kit.box("Stick_%d" % (i + 1), (x, y, 0.07), (0.012, 0.30, 0.012),
                p["wood_light"], rot=(0, 0, angle + math.pi / 2))
        for j in range(3):
            t = -0.09 + j * 0.07
            kit.box("Cube_%d_%d" % (i + 1, j + 1),
                    (x - math.sin(angle) * t, y + math.cos(angle) * t, 0.09),
                    (0.05, 0.05, 0.05), p["canvas_red"], rot=(0, 0, angle), bevel=0.008)
    for i, (x, y) in enumerate(((0.06, 0.05), (-0.07, 0.04), (0.01, -0.06))):
        kit.sphere("Onion_%d" % (i + 1), (x, y, 0.055), 0.028, p["cloth_white"],
                   segments=7, rings=4, scale=(1.0, 0.8, 0.7))
    kit.cyl("PepperTin", (0.10, -0.08, 0.06), 0.035, 0.05, p["rust"], verts=8)
    kit.box("Napkin", (-0.10, -0.08, 0.04), (0.10, 0.08, 0.02), p["plastic_white"])


# ---------------------------------------------------------------------------
# PRP_Napkin_ML_v001 - steel napkin holder with a paper stack
# ---------------------------------------------------------------------------
def napkin(kit):
    p = kit.p
    kit.box("Base", (0, 0, 0.012), (0.16, 0.11, 0.024), p["steel"], bevel=0.006)
    for i, y in enumerate((-0.045, 0.045)):
        kit.box("Wall_%d" % (i + 1), (0, y, 0.08), (0.16, 0.012, 0.16), p["steel"])
    kit.box("PaperStack", (0, 0, 0.075), (0.145, 0.075, 0.11), p["plastic_white"],
            bevel=0.004)
    for i in range(4):
        kit.box("Sheet_%d" % (i + 1), (0, 0, 0.03 + i * 0.028), (0.15, 0.078, 0.003),
                p["linen"])
    # Shakers sit just inboard of the holder's ends so the pair fills the
    # 0.28 m spec footprint without spilling past it: 2 x (0.10 + 0.03) = 0.26 m.
    kit.cyl("PepperShaker", (0.10, -0.12, 0.05), 0.03, 0.09, p["glass"], verts=8)
    kit.cyl("SaltShaker", (-0.10, -0.12, 0.05), 0.03, 0.09, p["plastic_white"], verts=8)


# ---------------------------------------------------------------------------
# SYU_SuyaSpot_ML_v001 - the whole stall: counter, canopy, banners, seating
# ---------------------------------------------------------------------------
def suya_spot(kit):
    p = kit.p
    W, D = 4.4, 3.0
    kit.box("Ground", (0, 0, -0.02), (W, D, 0.04), p["concrete"])
    # Back shed: corrugated wall + counter opening, a corral for the prep side.
    kit.box("BackWall", (0, D / 2 - 0.06, 1.20), (W, 0.12, 2.40), p["rust"], bevel=0.01)
    for i in range(11):
        x = -W / 2 + 0.20 + i * 0.40
        kit.box("Corrugation_%d" % (i + 1), (x, D / 2 - 0.13, 1.20), (0.06, 0.03, 2.38),
                p["rust"])
    kit.box("SideWall_L", (-W / 2 + 0.06, 0.60, 1.20), (0.12, 1.80, 2.40), p["wood_dark"])
    kit.box("SideWall_R", (W / 2 - 0.06, 0.60, 1.20), (0.12, 1.80, 2.40), p["wood_dark"])
    # Counter: the thing you walk up to, and the grill working surface.
    kit.box("Counter", (0, -0.55, 0.86), (3.60, 0.55, 0.10), p["steel"], bevel=0.01)
    kit.box("CounterFront", (0, -0.55, 0.44), (3.60, 0.10, 0.84), p["canvas_green"])
    for i in range(5):
        x = -1.50 + i * 0.75
        kit.box("CounterRib_%d" % (i + 1), (x, -0.61, 0.44), (0.05, 0.02, 0.80),
                p["canvas_red"])
    # Back lean-to over the prep side only, so the sky stays open above the grill.
    # A fascia board and gutter along its open edge give the shed a deliberate
    # silhouette from the street instead of a bare sheet floating in space.
    # The -0.14 tilt swings the rear edge back, so the sheet is centred forward
    # of the beam and trimmed to keep its highest corner inside Y +1.50.
    kit.box("Roof", (0, D / 2 - 0.75, 2.55), (W, 1.42, 0.06), p["steel"],
            rot=(-0.14, 0, 0))
    kit.box("RoofFascia", (0, D / 2 - 1.40, 2.40), (W, 0.04, 0.16), p["wood_dark"])
    kit.box("RoofGutter", (0, D / 2 - 1.44, 2.34), (W, 0.10, 0.06), p["alu"])
    for i, x in enumerate((-W / 2 + 0.10, -0.60, 0.60, W / 2 - 0.10)):
        kit.box("Post_%d" % (i + 1), (x, D / 2 - 0.75, 1.25), (0.10, 0.10, 2.50),
                p["wood_dark"])
    kit.box("Beam", (0, D / 2 - 0.75, 2.42), (W, 0.10, 0.12), p["wood_dark"])
    # Front frame: the lean-to only covers the prep side, so the street face needs
    # its own posts and header to carry the banners and the bulb string above the
    # counter. Height matches the beam so the stall reads as one structure.
    for i, x in enumerate((-W / 2 + 0.12, W / 2 - 0.12)):
        kit.box("FrontPost_%d" % (i + 1), (x, -D / 2 + 0.12, 1.22),
                (0.10, 0.10, 2.44), p["wood_dark"])
    kit.box("FrontHeader", (0, -D / 2 + 0.12, 2.42), (W, 0.10, 0.12), p["wood_dark"])
    kit.box("FrontFascia", (0, -D / 2 + 0.12, 2.28), (W - 0.24, 0.04, 0.16),
            p["canvas_green"])
    # The umbrella: a street suya parasol stands on its own weighted base beside
    # the grill and cantilevers the canopy out over the fire, so the pole never
    # spears the counter or the coals. Canopy centre lands over the grill at
    # X -0.85, Y -0.55; radius 0.80 m puts its rear edge at Y +0.25, just clear
    # of the lean-to's front edge (+0.247) and still covering the 1.30 m pit.
    R = 0.80
    PX, PY = -1.95, -0.55          # where the pole is actually planted
    CX, CY = -0.85, -0.55          # where the canopy hangs
    kit.cyl("UmbrellaBase", (PX, PY, 0.045), 0.24, 0.09, p["charcoal"], verts=12)
    kit.cyl("UmbrellaBaseDome", (PX, PY, 0.12), 0.15, 0.06, p["steel"], verts=12)
    kit.cyl("UmbrellaPole", (PX, PY, 1.35), 0.035, 2.50, p["metal"], verts=8)
    kit.box("UmbrellaBoom", ((PX + CX) / 2, PY, 2.58),
            (CX - PX, 0.045, 0.045), p["metal"])
    kit.box("UmbrellaBrace", ((PX + CX) / 2, PY, 2.40),
            (CX - PX, 0.03, 0.03), p["metal"], rot=(0, 0.30, 0))
    kit.cyl("UmbrellaHub", (CX, CY, 2.56), 0.06, 0.12, p["metal"], verts=10)
    kit.cone("UmbrellaCanopy", (CX, CY, 2.80), R, 0.06, 0.45,
             p["canvas_red"], verts=16)
    kit.cone("UmbrellaLining", (CX, CY, 2.72), R * 0.93, 0.06, 0.34,
             p["cloth_gold"], verts=16)
    kit.sphere("UmbrellaFinial", (CX, CY, 3.06), 0.045, p["metal"],
               segments=8, rings=5)
    for i in range(8):
        a = i * math.pi / 4
        kit.box("UmbrellaRib_%d" % (i + 1),
                (CX + math.cos(a) * R * 0.5, CY + math.sin(a) * R * 0.5, 2.58),
                (R * 1.02, 0.02, 0.02), p["metal"], rot=(0.32 * math.sin(a),
                -0.32 * math.cos(a), a))
    # Hanging banners either side of the counter - the street identity.
    for i, x in enumerate((-1.55, 1.55)):
        kit.box("Banner_%d" % (i + 1), (x, -D / 2 - 0.02, 1.95), (0.90, 0.02, 0.70),
                p["canvas_green"] if i == 0 else p["canvas_red"])
        kit.box("BannerTrim_%d" % (i + 1), (x, -D / 2 - 0.03, 1.62), (0.92, 0.02, 0.08),
                p["plastic_yellow"])
    # Grill station (the star), cooler, stool line, table.
    kit.box("GrillBody", (-0.85, -0.55, 1.02), (1.30, 0.50, 0.24), p["steel"], bevel=0.01)
    kit.box("Coals", (-0.85, -0.55, 1.13), (1.16, 0.38, 0.03), p["fire"])
    for i in range(6):
        kit.box("SkewerRow_%d" % (i + 1), (-1.35 + i * 0.20, -0.55, 1.15),
                (0.02, 0.44, 0.02), p["wood_light"])
    # No extractor hood here: a street suya spot vents straight to the sky, and a
    # hood duct would clash with the parasol occupying the same airspace.
    kit.box("CoolerBox", (1.20, -0.55, 1.10), (0.55, 0.36, 0.36), p["plastic_blue"],
            bevel=0.02)
    kit.cyl("MaltBottle_1", (1.08, -0.55, 1.38), 0.04, 0.22, p["canvas_green"], verts=8)
    kit.cyl("MaltBottle_2", (1.26, -0.52, 1.38), 0.04, 0.22, p["canvas_green"], verts=8)
    for i, (x, y) in enumerate(((-1.60, -0.90), (-0.90, -0.90), (0.90, -0.90),
                                (1.60, -0.90))):
        kit.cyl("Stool_%d" % (i + 1), (x, y, 0.33), 0.18, 0.03, p["plastic_white"],
                verts=10)
        kit.cyl("StoolLeg_%d" % (i + 1), (x, y, 0.16), 0.05, 0.30, p["plastic_white"],
                verts=8)
    # Table pulled inboard so its 0.8 m top sits wholly inside the 3.0 m depth:
    # centre Y -1.08 leaves 0.38 m to the front edge (Y -1.50) and clears the stools.
    kit.box("TableTop", (1.55, -1.08, 0.72), (0.80, 0.80, 0.04), p["plastic_white"],
            bevel=0.01)
    for x in (1.24, 1.86):
        for y in (-1.39, -0.77):
            kit.box("TableLeg_%s_%s" % (x, y), (x, y, 0.36), (0.04, 0.04, 0.70),
                    p["plastic_white"])
    # Bulb string across the front: what makes a suya spot visible from far away.
    kit.box("BulbCable", (0, -D / 2 + 0.05, 2.30), (W - 0.4, 0.02, 0.02), p["black"])
    for i in range(7):
        x = -1.8 + i * 0.60
        kit.sphere("Bulb_%d" % (i + 1), (x, -D / 2 + 0.05, 2.22), 0.055, p["bulb_warm"],
                   segments=8, rings=5)
    kit.box("PriceBoard", (0.30, -0.80, 1.55), (0.50, 0.03, 0.36), p["charcoal"])
    for i in range(3):
        kit.box("PriceLine_%d" % (i + 1), (0.30, -0.82, 1.64 - i * 0.10),
                (0.34, 0.01, 0.03), p["neon_amber"])


SPECS = [
    AssetSpec("PRP_Grill_ML_v001", (1.40, 0.55, 1.26), grill,
              note="suya trough grill, live coals + 6 skewers"),
    AssetSpec("PRP_Table_ML_v001", (0.80, 0.80, 0.98), table,
              note="folding outdoor table with malt + plate"),
    AssetSpec("PRP_Stool_ML_v001", (0.38, 0.38, 0.35), stool, note="stackable stool"),
    AssetSpec("PRP_Fan_ML_v001", (0.46, 0.55, 1.08), fan, note="floor fan, wire cage"),
    AssetSpec("PRP_Cooler_ML_v001", (0.57, 0.40, 0.66), cooler,
              note="drinks cooler, lid open, 3 bottles"),
    AssetSpec("PRP_Skewer_ML_v001", (0.36, 0.36, 0.12), skewer,
              note="plated suya: 4 skewers, onions, pepper"),
    AssetSpec("PRP_Napkin_ML_v001", (0.28, 0.22, 0.17), napkin,
              note="steel napkin holder + shakers"),
    AssetSpec("SYU_SuyaSpot_ML_v001", (4.40, 3.06, 3.15), suya_spot,
              note="4.4x3m stall: counter, grill, parasol, banners, seating"),
]
