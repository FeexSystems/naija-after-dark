"""Priority 2 - Apartment (Night District): furniture + room shell."""

from naad_spec import AssetSpec


# ---------------------------------------------------------------------------
# PRP_Bed_ND_v001 - double bed, 1.6 x 2.1 m footprint, headboard at +Y
# ---------------------------------------------------------------------------
def bed(kit):
    p = kit.p
    kit.box("Base", (0, 0, 0.20), (1.58, 2.08, 0.30), p["wood_dark"], bevel=0.02)
    for i, x in enumerate((-0.72, 0.72)):
        for j, y in enumerate((-0.98, 0.98)):
            kit.box("Foot_%d%d" % (i + 1, j + 1), (x, y, 0.05), (0.09, 0.09, 0.10),
                    p["black"])
    kit.box("Mattress", (0, 0, 0.46), (1.50, 2.00, 0.24), p["linen"], bevel=0.05)
    kit.box("Duvet", (0, -0.28, 0.60), (1.54, 1.44, 0.10), p["fabric_plum"], bevel=0.035)
    kit.box("DuvetFold", (0, 0.42, 0.63), (1.55, 0.30, 0.09), p["fabric_cream"],
            bevel=0.03)
    for i, x in enumerate((-0.36, 0.36)):
        kit.box("Pillow_%d" % (i + 1), (x, 0.80, 0.65), (0.62, 0.36, 0.15),
                p["fabric_cream"], rot=(0, 0, 0.05 if x > 0 else -0.05), bevel=0.05)
    kit.box("Headboard", (0, 1.06, 0.62), (1.66, 0.09, 0.86), p["wood_dark"], bevel=0.025)
    kit.box("HeadboardTrim", (0, 1.10, 1.04), (1.70, 0.05, 0.05), p["fabric_plum"])
    kit.box("Nightstand", (0.98, 0.86, 0.24), (0.44, 0.40, 0.48), p["wood_mid"],
            bevel=0.015)
    kit.box("LampShade", (0.98, 0.86, 0.58), (0.20, 0.20, 0.16), p["bulb_warm"])
    kit.cyl("LampStem", (0.98, 0.86, 0.50), 0.014, 0.16, p["metal"], verts=8)


# ---------------------------------------------------------------------------
# PRP_Sofa_ND_v001 - 3-seat sofa, back at +Y
# ---------------------------------------------------------------------------
def sofa(kit):
    p = kit.p
    kit.box("Plinth", (0, 0, 0.10), (2.14, 0.92, 0.18), p["wood_dark"])
    for i, x in enumerate((-1.0, 1.0)):
        for j, y in enumerate((-0.38, 0.38)):
            kit.box("Foot_%d%d" % (i + 1, j + 1), (x, y, 0.03), (0.08, 0.08, 0.06),
                    p["black"])
    kit.box("SeatBase", (0, 0, 0.33), (2.10, 0.90, 0.28), p["fabric_plum"], bevel=0.03)
    for i, x in enumerate((-0.70, 0.0, 0.70)):
        kit.box("Cushion_%d" % (i + 1), (x, -0.02, 0.50), (0.68, 0.84, 0.14),
                p["fabric_plum"], bevel=0.04)
    kit.box("BackRest", (0, 0.38, 0.70), (2.14, 0.16, 0.62), p["fabric_plum"],
            bevel=0.03)
    for i, x in enumerate((-0.70, 0.0, 0.70)):
        kit.box("BackCushion_%d" % (i + 1), (x, 0.27, 0.74), (0.66, 0.16, 0.55),
                p["fabric_plum"], rot=(-0.10, 0, 0), bevel=0.04)
    for i, x in enumerate((-1.02, 1.02)):
        kit.box("Arm_%d" % (i + 1), (x, 0, 0.52), (0.18, 0.90, 0.44), p["fabric_plum"],
                bevel=0.03)
    kit.box("Throw", (-0.72, -0.04, 0.62), (0.62, 0.60, 0.03), p["fabric_orange"],
            rot=(0, 0, 0.12))
    kit.box("PillowDeco", (0.78, 0.20, 0.66), (0.34, 0.12, 0.34), p["fabric_cream"],
            rot=(0.1, 0, 0.2), bevel=0.04)


# ---------------------------------------------------------------------------
# PRP_Table_ND_v001 - coffee table 1.2 x 0.6, with lower shelf
# ---------------------------------------------------------------------------
def table(kit):
    p = kit.p
    kit.box("Top", (0, 0, 0.44), (1.20, 0.62, 0.05), p["wood_light"], bevel=0.008)
    kit.box("Apron", (0, 0, 0.38), (1.08, 0.52, 0.08), p["wood_mid"])
    for i, x in enumerate((-0.53, 0.53)):
        for j, y in enumerate((-0.24, 0.24)):
            kit.box("Leg_%d%d" % (i + 1, j + 1), (x, y, 0.22), (0.06, 0.06, 0.44),
                    p["wood_mid"])
    kit.box("Shelf", (0, 0, 0.13), (1.02, 0.48, 0.03), p["wood_mid"])
    kit.box("Remote", (0.28, -0.02, 0.47), (0.05, 0.19, 0.025), p["black"])
    kit.cyl("Mug", (-0.30, 0.06, 0.48), 0.045, 0.08, p["plastic_red"], verts=10)
    kit.box("Books", (-0.30, -0.16, 0.47), (0.22, 0.16, 0.03), p["canvas_red"])


# ---------------------------------------------------------------------------
# PRP_Chair_ND_v001 - dining chair, seat 0.45 m
# ---------------------------------------------------------------------------
def chair(kit):
    p = kit.p
    kit.box("Seat", (0, 0, 0.45), (0.46, 0.46, 0.05), p["wood_light"], bevel=0.01)
    for i, x in enumerate((-0.19, 0.19)):
        for j, y in enumerate((-0.19, 0.19)):
            kit.box("Leg_%d%d" % (i + 1, j + 1), (x, y, 0.225), (0.045, 0.045, 0.45),
                    p["wood_mid"])
    for i, x in enumerate((-0.19, 0.19)):
        kit.box("Post_%d" % (i + 1), (x, 0.20, 0.70), (0.045, 0.045, 0.50), p["wood_mid"])
    kit.box("BackRest", (0, 0.20, 0.90), (0.42, 0.04, 0.22), p["wood_light"],
            bevel=0.01)
    kit.box("BackSlat", (0, 0.20, 0.68), (0.38, 0.03, 0.12), p["wood_mid"])
    kit.box("SeatPad", (0, -0.01, 0.485), (0.42, 0.42, 0.03), p["fabric_cream"])
    for i, x in enumerate((-0.19, 0.19)):
        kit.box("Stretcher_%d" % (i + 1), (x, 0, 0.16), (0.03, 0.38, 0.03),
                p["wood_mid"])


# ---------------------------------------------------------------------------
# PRP_TV_ND_v001 - 55" wall TV on low console
# ---------------------------------------------------------------------------
def tv(kit):
    p = kit.p
    kit.box("Console", (0, 0.20, 0.24), (1.60, 0.40, 0.48), p["wood_dark"], bevel=0.015)
    for i, x in enumerate((-0.42, 0.42)):
        kit.box("Drawer_%d" % (i + 1), (x, -0.005, 0.28), (0.72, 0.02, 0.30),
                p["wood_mid"])
    for i, x in enumerate((-0.72, 0.72)):
        kit.box("Foot_%d" % (i + 1), (x, 0.20, 0.015), (0.08, 0.32, 0.03), p["black"])
    kit.cyl("Stand", (0, 0.20, 0.51), 0.14, 0.03, p["black"], verts=16)
    kit.box("StandNeck", (0, 0.20, 0.60), (0.10, 0.05, 0.16), p["black"])
    kit.box("Panel", (0, 0.16, 1.05), (1.24, 0.05, 0.72), p["black"], bevel=0.006)
    kit.box("Bezel", (0, 0.185, 1.05), (1.20, 0.01, 0.68), p["metal"])
    kit.box("Screen", (0, 0.135, 1.05), (1.16, 0.005, 0.64), p["screen"])
    kit.box("Soundbar", (0, 0.02, 0.52), (0.90, 0.09, 0.06), p["black"])


# ---------------------------------------------------------------------------
# PRP_Lamp_ND_v001 - floor lamp 1.55 m with warm shade
# ---------------------------------------------------------------------------
def lamp(kit):
    p = kit.p
    kit.cyl("Base", (0, 0, 0.02), 0.16, 0.04, p["metal"], verts=16)
    kit.cyl("Pole", (0, 0, 0.78), 0.018, 1.50, p["metal"], verts=10)
    kit.cyl("Joint", (0, 0, 0.55), 0.03, 0.05, p["brass"], verts=10)
    kit.cone("Shade", (0, 0, 1.55), 0.20, 0.13, 0.30, p["fabric_cream"], verts=16)
    kit.sphere("Bulb", (0, 0, 1.45), 0.06, p["bulb_warm"], segments=10, rings=5)


# ---------------------------------------------------------------------------
# PRP_Wardrobe_ND_v001 - 1.8 m double wardrobe
# ---------------------------------------------------------------------------
def wardrobe(kit):
    p = kit.p
    kit.box("Carcass", (0, 0.02, 1.06), (1.80, 0.60, 2.10), p["wood_mid"], bevel=0.01)
    for i, x in enumerate((-0.45, 0.45)):
        kit.box("Door_%d" % (i + 1), (x, -0.285, 1.08), (0.88, 0.03, 2.00),
                p["wood_dark"])
    for i, x in enumerate((-0.06, 0.06)):
        kit.cyl("Handle_%d" % (i + 1), (x, -0.315, 1.08), 0.012, 0.36, p["brass"],
                verts=8)
    kit.box("Plinth", (0, 0.02, 0.04), (1.74, 0.54, 0.08), p["black"])
    kit.box("Cornice", (0, 0.02, 2.13), (1.84, 0.64, 0.06), p["wood_dark"])
    kit.box("Mirror", (0.90, -0.30, 1.35), (0.44, 0.02, 1.10), p["glass_dark"])


# ---------------------------------------------------------------------------
# PRP_Window_ND_v001 - 1.2 x 1.35 window, 0.2 m reveal, glass in middle
# ---------------------------------------------------------------------------
def window(kit):
    p = kit.p
    kit.box("OuterFrame", (0, 0, 1.10), (1.20, 0.20, 1.35), p["alu"], bevel=0.01)
    kit.box("Glass", (0, 0, 1.10), (1.06, 0.02, 1.21), p["glass"])
    kit.box("SashVertical", (0, 0, 1.10), (0.06, 0.10, 1.25), p["alu"])
    kit.box("SashHorizontal", (0, 0, 1.42), (1.12, 0.10, 0.05), p["alu"])
    kit.box("Sill", (0, -0.10, 0.42), (1.34, 0.24, 0.06), p["concrete_light"])
    kit.box("BurglarBar_1", (0, -0.13, 1.10), (1.16, 0.02, 0.02), p["metal"])
    kit.box("BurglarBar_2", (0, -0.13, 0.92), (1.16, 0.02, 0.02), p["metal"])
    for i, x in enumerate((-0.36, -0.12, 0.12, 0.36)):
        kit.box("Bar_%d" % (i + 1), (x, -0.13, 1.10), (0.02, 0.02, 1.25), p["metal"])
    kit.box("Curtain", (0.62, 0.14, 1.12), (0.30, 0.06, 1.30), p["fabric_orange"])


# ---------------------------------------------------------------------------
# PRP_Door_ND_v001 - 0.9 x 2.05 apartment door in frame
# ---------------------------------------------------------------------------
def door(kit):
    p = kit.p
    kit.box("Frame_L", (-0.51, 0, 1.05), (0.06, 0.22, 2.10), p["wood_dark"])
    kit.box("Frame_R", (0.51, 0, 1.05), (0.06, 0.22, 2.10), p["wood_dark"])
    kit.box("Frame_Top", (0, 0, 2.07), (1.08, 0.22, 0.06), p["wood_dark"])
    kit.box("Leaf", (0, 0, 1.02), (0.94, 0.05, 2.02), p["wood_mid"], bevel=0.008)
    for i, z in enumerate((0.55, 1.45)):
        kit.box("Panel_%d" % (i + 1), (0, -0.032, z), (0.66, 0.02, 0.62), p["wood_dark"])
    kit.sphere("Handle", (0.36, -0.06, 1.02), 0.045, p["brass"], segments=10, rings=5)
    kit.cyl("HandleRose", (0.36, -0.035, 1.02), 0.055, 0.02, p["brass"],
            rot=(1.5708, 0, 0), verts=12)
    kit.box("Kick", (0, -0.03, 0.16), (0.90, 0.02, 0.18), p["metal"])
    kit.box("Threshold", (0, 0, 0.012), (1.02, 0.24, 0.024), p["steel"])


# ---------------------------------------------------------------------------
# INT_Apartment_ND_v001 - 8 x 6 m room shell, 2.8 m ceiling
# ---------------------------------------------------------------------------
def apartment(kit):
    p = kit.p
    W, D, H = 8.0, 6.0, 2.8
    kit.box("Floor", (0, 0, -0.05), (W, D, 0.10), p["wood_light"])
    for i in range(8):
        x = -3.5 + i
        kit.box("FloorBoard_%d" % (i + 1), (x, 0, 0.002), (0.02, D - 0.02, 0.004),
                p["wood_mid"])
    kit.box("Ceiling", (0, 0, H + 0.05), (W, D, 0.10), p["fabric_cream"])
    kit.box("WallBack", (0, D / 2 + 0.06, H / 2), (W, 0.12, H), p["fabric_cream"])
    kit.box("WallLeft", (-W / 2 - 0.06, 0, H / 2), (0.12, D, H), p["fabric_cream"])
    kit.box("WallRight", (W / 2 + 0.06, 0, H / 2), (0.12, D, H), p["concrete_light"])
    # Front wall with door + window openings (built from panels around holes)
    kit.box("Front_L", (-2.9, -D / 2 - 0.06, H / 2), (2.2, 0.12, H), p["fabric_cream"])
    kit.box("Front_R", (2.6, -D / 2 - 0.06, H / 2), (2.8, 0.12, H), p["fabric_cream"])
    kit.box("Front_OverDoor", (-1.1, -D / 2 - 0.06, 2.5), (1.4, 0.12, 0.6),
            p["fabric_cream"])
    kit.box("Skirting_Back", (0, D / 2 - 0.02, 0.06), (W, 0.03, 0.12), p["wood_dark"])
    kit.box("Skirting_Left", (-W / 2 + 0.02, 0, 0.06), (0.03, D, 0.12), p["wood_dark"])
    kit.box("Skirting_Right", (W / 2 - 0.02, 0, 0.06), (0.03, D, 0.12), p["wood_dark"])
    # Ceiling light
    kit.cyl("CeilingRose", (0, 0.4, H - 0.02), 0.18, 0.04, p["plastic_white"], verts=16)
    kit.cyl("BulbPanel", (0, 0.4, H - 0.05), 0.30, 0.03, p["bulb_cool"], verts=20)
    # Door + window in the shell
    kit.box("DoorFrame", (-1.1, -D / 2, 1.02), (1.10, 0.20, 2.05), p["wood_dark"])
    kit.box("DoorLeaf", (-1.1, -D / 2 + 0.02, 1.00), (0.96, 0.06, 1.98), p["wood_mid"])
    kit.box("WindowOpening", (2.2, -D / 2, 1.50), (1.6, 0.18, 1.20), p["glass"])
    kit.box("WindowFrameV", (2.2, -D / 2, 1.50), (0.07, 0.20, 1.24), p["alu"])
    kit.box("WindowFrameH", (2.2, -D / 2, 1.50), (1.64, 0.20, 0.06), p["alu"])
    # Rug + seating arrangement (reads as a furnished room at blockout scale)
    kit.box("Rug", (0, 0.2, 0.008), (3.4, 2.4, 0.016), p["fabric_dark"])
    kit.box("SofaSeat", (0, 1.9, 0.42), (2.2, 0.9, 0.30), p["fabric_plum"], bevel=0.03)
    kit.box("SofaBack", (0, 2.28, 0.72), (2.2, 0.16, 0.60), p["fabric_plum"], bevel=0.03)
    kit.box("CoffeeTop", (0, 0.2, 0.42), (1.2, 0.6, 0.05), p["wood_light"], bevel=0.01)
    for i, x in enumerate((-0.5, 0.5)):
        for j, y in enumerate((-0.22, 0.22)):
            kit.box("CoffeeLeg_%d%d" % (i + 1, j + 1), (x, 0.2 + y, 0.2), (0.05, 0.05, 0.4),
                    p["wood_mid"])
    kit.box("BedBase", (-2.7, 1.6, 0.25), (1.6, 2.05, 0.40), p["wood_dark"], bevel=0.02)
    kit.box("BedMat", (-2.7, 1.6, 0.50), (1.52, 1.98, 0.16), p["linen"], bevel=0.04)
    kit.box("Headboard", (-2.7, 2.62, 0.70), (1.66, 0.08, 0.90), p["wood_dark"])
    kit.box("KitchenCounter", (2.2, 2.55, 0.45), (2.6, 0.6, 0.90), p["concrete_light"],
            bevel=0.01)
    kit.box("CounterTop", (2.2, 2.55, 0.92), (2.66, 0.64, 0.05), p["steel"])
    kit.box("KettleUrn", (1.2, 2.5, 1.05), (0.24, 0.24, 0.22), p["steel"], bevel=0.03)
    kit.box("Wardrobe", (3.2, 0.6, 1.05), (1.4, 0.6, 2.10), p["wood_mid"], bevel=0.01)


SPECS = [
    AssetSpec("PRP_Bed_ND_v001", (2.0, 2.22, 1.10), bed, note="double bed + nightstand"),
    AssetSpec("PRP_Sofa_ND_v001", (2.14, 0.92, 0.96), sofa, note="3-seat sofa"),
    AssetSpec("PRP_Table_ND_v001", (1.20, 0.62, 0.47), table, note="coffee table"),
    AssetSpec("PRP_Chair_ND_v001", (0.46, 0.46, 1.01), chair, note="dining chair"),
    AssetSpec("PRP_TV_ND_v001", (1.60, 0.40, 1.41), tv, note="55in TV + console"),
    AssetSpec("PRP_Lamp_ND_v001", (0.40, 0.40, 1.70), lamp, note="floor lamp"),
    AssetSpec("PRP_Wardrobe_ND_v001", (1.84, 0.64, 2.16), wardrobe, note="double wardrobe"),
    AssetSpec("PRP_Window_ND_v001", (1.34, 0.24, 1.78), window, note="1.2x1.35 window"),
    AssetSpec("PRP_Door_ND_v001", (1.08, 0.24, 2.10), door, note="apartment door"),
    AssetSpec("INT_Apartment_ND_v001", (8.16, 6.16, 2.90), apartment,
              note="8x6x2.8m furnished shell"),
]
