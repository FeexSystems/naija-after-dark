"""Priority 3 - Street (Mainland Streets)."""

import math

from naad_spec import AssetSpec


# ---------------------------------------------------------------------------
# PRP_LampPost_ML_v001 - 9 m street lamp with cantilever warm head
# ---------------------------------------------------------------------------
def lamp_post(kit):
    p = kit.p
    kit.box("Base", (0, 0, 0.15), (0.42, 0.42, 0.30), p["concrete"], bevel=0.02)
    kit.cyl("Plinth", (0, 0, 0.34), 0.20, 0.10, p["concrete_light"], verts=12)
    kit.cyl("Pole", (0, 0, 4.7), 0.075, 8.8, p["metal"], verts=12)
    for i in range(4):
        kit.cyl("Collar_%d" % (i + 1), (0, 0, 1.2 + i * 1.9), 0.095, 0.07,
                p["steel"], verts=12)
    kit.box("Arm", (0, -0.75, 8.85), (0.09, 1.50, 0.09), p["metal"])
    kit.box("HeadHousing", (0, -1.55, 8.72), (0.34, 0.62, 0.16), p["metal"], bevel=0.02)
    kit.box("HeadLens", (0, -1.55, 8.62), (0.28, 0.54, 0.03), p["bulb_warm"])
    kit.cyl("Cap", (0, 0, 9.10), 0.10, 0.12, p["metal"], verts=10)
    # Banner + junction box: reads as a Lagos street pole, not a generic asset.
    kit.box("Banner", (0.30, -0.28, 6.10), (0.06, 0.52, 1.10), p["canvas_green"])
    kit.box("BannerRod", (0.30, -0.28, 6.70), (0.08, 0.64, 0.04), p["metal"])
    for x in (-0.30, 0.30):
        kit.box("BannerBracket", (x, 0, 6.10), (0.02, 0.02, 0.70), p["metal"])


# ---------------------------------------------------------------------------
# PRP_Bench_ML_v001 - 1.8 m concrete/timber bench
# ---------------------------------------------------------------------------
def bench(kit):
    p = kit.p
    for i, x in enumerate((-0.75, 0.75)):
        kit.box("Leg_%d" % (i + 1), (x, 0, 0.21), (0.14, 0.52, 0.42), p["concrete"],
                bevel=0.015)
    for i in range(4):
        kit.box("SeatSlat_%d" % (i + 1), (0, -0.18 + i * 0.12, 0.46),
                (1.80, 0.10, 0.05), p["wood_light"], bevel=0.008)
    for i in range(3):
        kit.box("BackSlat_%d" % (i + 1), (0, 0.26, 0.66 + i * 0.15),
                (1.80, 0.04, 0.11), p["wood_light"], rot=(0.12, 0, 0), bevel=0.008)
    for i, x in enumerate((-0.75, 0.75)):
        kit.box("BackPost_%d" % (i + 1), (x, 0.26, 0.72), (0.10, 0.06, 0.60),
                p["metal"], rot=(0.12, 0, 0))
    kit.box("CrossBrace", (0, 0, 0.12), (1.60, 0.06, 0.04), p["metal"])


# ---------------------------------------------------------------------------
# PRP_Bin_ML_v001 - 0.6 m metal + plastic street bin
# ---------------------------------------------------------------------------
def bin_prop(kit):
    p = kit.p
    kit.cyl("Body", (0, 0, 0.38), 0.28, 0.70, p["metal"], verts=16)
    for i in range(3):
        kit.cyl("Band_%d" % (i + 1), (0, 0, 0.14 + i * 0.22), 0.29, 0.04,
                p["plastic_blue"], verts=16)
    kit.cone("Lid", (0, 0, 0.77), 0.30, 0.26, 0.10, p["plastic_blue"], verts=16)
    kit.cyl("Opening", (0, 0, 0.83), 0.16, 0.03, p["black"], verts=12)
    kit.box("Latch", (0, -0.28, 0.74), (0.10, 0.04, 0.10), p["steel"])
    kit.cyl("FootRing", (0, 0, 0.03), 0.30, 0.06, p["black"], verts=16)


# ---------------------------------------------------------------------------
# PRP_Signage_ML_v001 - illuminated shop sign on a bracket + shopfront
# ---------------------------------------------------------------------------
def signage(kit):
    p = kit.p
    kit.box("Post", (0, 0, 1.6), (0.10, 0.10, 3.20), p["metal"])
    kit.box("PostFoot", (0, 0, 0.06), (0.36, 0.36, 0.12), p["concrete"])
    kit.box("ArmLow", (0, -0.45, 2.55), (0.06, 0.95, 0.06), p["metal"])
    kit.box("ArmHigh", (0, -0.45, 3.15), (0.06, 0.95, 0.06), p["metal"])
    kit.box("Panel", (0, -0.62, 2.85), (0.14, 1.90, 0.90), p["canvas_green"], bevel=0.01)
    kit.box("PanelTrim", (0, -0.62, 3.32), (0.16, 1.96, 0.05), p["canvas_red"])
    kit.box("PanelTrimLow", (0, -0.62, 2.38), (0.16, 1.96, 0.05), p["canvas_red"])
    # Block "letters" so the sign reads at distance.
    for i, x in enumerate((-0.72, -0.36, 0.0, 0.36, 0.72)):
        kit.box("Letter_%d" % (i + 1), (-0.08, x, 2.86), (0.02, 0.26, 0.44),
                p["neon_amber"])
    kit.box("ShopWindow", (0, 0.02, 1.30), (0.08, 1.80, 1.60), p["canvas_red"])
    kit.box("ShopGlass", (0, 0.02, 1.30), (0.06, 1.55, 1.35), p["glass_dark"])
    kit.box("ShopLightbox", (0, 0.02, 2.22), (0.10, 1.80, 0.24), p["neon_pink"])


# ---------------------------------------------------------------------------
# PRP_VendorCart_ML_v001 - wheeled stall cart with hot tray + produce
# ---------------------------------------------------------------------------
def vendor_cart(kit):
    p = kit.p
    kit.box("Deck", (0, 0, 0.72), (2.0, 1.0, 0.10), p["wood_mid"], bevel=0.01)
    kit.box("Body", (0, 0, 0.45), (1.90, 0.90, 0.44), p["wood_dark"], bevel=0.01)
    kit.box("BodyPanel", (0, -0.46, 0.45), (1.70, 0.03, 0.34), p["canvas_red"])
    for i, (x, y) in enumerate(((-0.78, -0.5), (0.78, -0.5), (-0.78, 0.5), (0.78, 0.5))):
        kit.torus("Wheel_%d" % (i + 1), (x, y, 0.26), 0.22, 0.06, p["black"],
                  rot=(math.pi / 2, 0, 0), major_seg=14, minor_seg=6)
        kit.cyl("Hub_%d" % (i + 1), (x, y, 0.26), 0.07, 0.16, p["steel"],
                rot=(math.pi / 2, 0, 0), verts=8)
    for i, y in enumerate((-0.5, 0.5)):
        kit.box("Axle_%d" % (i + 1), (0, y, 0.26), (1.70, 0.05, 0.05), p["steel"])
    for i, (x, y) in enumerate(((-0.94, -0.44), (0.94, -0.44), (-0.94, 0.44), (0.94, 0.44))):
        kit.cyl("CanopyPost_%d" % (i + 1), (x, y, 1.35), 0.025, 1.30, p["metal"],
                verts=8)
    kit.box("Canopy", (0, 0, 2.02), (2.10, 1.10, 0.05), p["canvas_green"], bevel=0.01)
    kit.box("CanopyValance", (0, -0.55, 1.92), (2.10, 0.03, 0.18), p["canvas_red"])
    kit.box("HotTray", (-0.45, -0.10, 0.82), (0.80, 0.60, 0.12), p["steel"], bevel=0.01)
    kit.box("HotTrayLid", (-0.45, 0.12, 0.90), (0.80, 0.16, 0.03), p["glass"])
    for i in range(3):
        x = 0.45 + (i % 2) * 0.30
        y = -0.22 + (i // 2) * 0.42
        kit.box("ProduceCrate_%d" % (i + 1), (x, y, 0.83), (0.28, 0.34, 0.12),
                p["plastic_red"])
        kit.sphere("Tomato_%d" % (i + 1), (x, y, 0.92), 0.06, p["canvas_red"],
                   segments=8, rings=4)
    kit.cyl("HangingBulb", (0, 0, 1.85), 0.06, 0.16, p["bulb_warm"], verts=10)
    kit.box("PriceBoard", (0.72, 0.48, 1.30), (0.38, 0.03, 0.30), p["fabric_cream"])


# ---------------------------------------------------------------------------
# PRP_TrafficLight_ML_v001 - 3-lamp signal head on mast arm, 4.4 m
# ---------------------------------------------------------------------------
def traffic_light(kit):
    p = kit.p
    kit.box("Base", (0, 0, 0.12), (0.50, 0.50, 0.24), p["concrete"], bevel=0.02)
    kit.cyl("Pole", (0, 0, 2.2), 0.10, 4.30, p["metal"], verts=12)
    kit.box("Arm", (-1.35, 0, 4.30), (2.70, 0.10, 0.10), p["metal"])
    kit.box("ArmBrace", (-0.70, 0, 3.95), (1.20, 0.06, 0.06), p["metal"], rot=(0, 0.5, 0))
    kit.box("HeadHousing", (-2.15, 0, 3.80), (0.40, 0.34, 1.10), p["black"], bevel=0.02)
    kit.box("VisorRed", (-2.15, -0.20, 4.24), (0.36, 0.12, 0.05), p["black"])
    kit.sphere("LampRed", (-2.15, -0.19, 4.15), 0.11, p["signal_red"], segments=10,
               rings=5)
    kit.sphere("LampAmber", (-2.15, -0.19, 3.82), 0.11, p["neon_amber"], segments=10,
               rings=5)
    kit.sphere("LampGreen", (-2.15, -0.19, 3.49), 0.11, p["signal_green"],
               segments=10, rings=5)
    kit.box("PedHead", (-0.55, -0.20, 2.70), (0.34, 0.14, 0.70), p["black"])
    kit.box("PedRed", (-0.55, -0.28, 2.92), (0.22, 0.02, 0.22), p["signal_red"])
    kit.box("PedGreen", (-0.55, -0.28, 2.52), (0.22, 0.02, 0.22), p["signal_green"])
    kit.box("Controller", (0.45, 0, 1.05), (0.36, 0.28, 0.80), p["alu"], bevel=0.01)


# ---------------------------------------------------------------------------
# PRP_Pothole_ML_v001 - road defect patch (1.5 x 1.2, sits on asphalt)
# ---------------------------------------------------------------------------
def pothole(kit):
    p = kit.p
    kit.cyl("Patch", (0, 0, 0.005), 0.75, 0.012, p["asphalt"], verts=16)
    kit.cyl("Crater", (0, 0, 0.007), 0.55, 0.014, p["charcoal"], verts=14)
    kit.cyl("Water", (0.05, -0.05, 0.016), 0.34, 0.005, p["glass_dark"], verts=12)
    for i, (x, y, r) in enumerate(((-0.55, 0.30, 0.18), (0.42, 0.42, 0.14),
                                   (-0.38, -0.48, 0.16), (0.58, -0.30, 0.12))):
        kit.sphere("Debris_%d" % (i + 1), (x, y, 0.02), r, p["concrete"],
                   segments=7, rings=4, scale=(1.0, 0.8, 0.35))
    for i, rot in enumerate((0.4, 1.9, 3.1, 4.4)):
        kit.box("Crack_%d" % (i + 1),
                (math.cos(rot) * 0.62, math.sin(rot) * 0.62, 0.012),
                (0.46, 0.035, 0.008), p["charcoal"], rot=(0, 0, rot))


# ---------------------------------------------------------------------------
# STR_CityStreet_ML_v001 - 24 x 10 m modular street tile
# ---------------------------------------------------------------------------
def city_street(kit):
    p = kit.p
    L, ROAD = 24.0, 7.0
    kit.box("Asphalt", (0, 0, 0.02), (L, ROAD, 0.06), p["asphalt"])
    kit.box("Underlay", (0, 0, -0.02), (L + 0.4, 10.4, 0.04), p["concrete"])
    for side in (-1, 1):
        tag = "L" if side < 0 else "R"
        y = side * (ROAD / 2 + 1.25)
        kit.box("Sidewalk_" + tag, (0, y, 0.13), (L, 2.5, 0.14), p["concrete_light"])
        kit.box("Curb_" + tag, (0, side * (ROAD / 2 + 0.06), 0.13), (L, 0.12, 0.16),
                p["concrete"])
        kit.box("KerbPaint_" + tag, (0, side * (ROAD / 2 + 0.13), 0.20),
                (L, 0.03, 0.03), p["plastic_yellow"])
        for i in range(5):
            x = -9.6 + i * 4.8
            kit.box("Drain_%s_%d" % (tag, i + 1), (x, side * (ROAD / 2 + 0.02), 0.08),
                    (0.7, 0.30, 0.06), p["black"])
        for i in range(3):
            x = -8.0 + i * 8.0
            kit.cyl("LampPole_%s_%d" % (tag, i + 1),
                    (x, side * (ROAD / 2 + 1.55), 3.0), 0.06, 6.0, p["metal"], verts=8)
            kit.box("LampHead_%s_%d" % (tag, i + 1),
                    (x - 0.5, side * (ROAD / 2 + 1.55), 6.0), (1.0, 0.16, 0.10),
                    p["metal"])
            kit.box("LampLens_%s_%d" % (tag, i + 1),
                    (x - 0.8, side * (ROAD / 2 + 1.55), 5.93), (0.32, 0.12, 0.03),
                    p["bulb_warm"])
        for i in range(2):
            x = -4.0 + i * 8.0
            kit.cyl("TreeTrunk_%s_%d" % (tag, i + 1),
                    (x, side * (ROAD / 2 + 1.9), 1.1), 0.11, 2.2, p["bark"], verts=8)
            kit.sphere("TreeCrown_%s_%d" % (tag, i + 1),
                       (x, side * (ROAD / 2 + 1.9), 2.7), 1.05,
                       p["leaf_dark"] if side < 0 else p["leaf"], segments=8, rings=5,
                       scale=(1.0, 1.0, 0.85))
    for i in range(10):
        kit.box("CenterDash_%d" % (i + 1), (-10.8 + i * 2.4, 0, 0.056),
                (1.2, 0.12, 0.01), p["road_paint"])
    for side in (-1, 1):
        kit.box("EdgeLine_%s" % ("L" if side < 0 else "R"),
                (0, side * (ROAD / 2 - 0.30), 0.056), (L, 0.10, 0.01), p["road_paint"])
    for i in range(7):
        kit.box("Crosswalk_%d" % (i + 1), (-11.0, -3.0 + i * 1.0, 0.058),
                (3.0, 0.50, 0.011), p["road_paint"])
    kit.box("StopLine", (-9.2, 3.0, 0.058), (0.25, 0.9, 0.011), p["road_paint"])
    kit.cyl("ManholeLid", (6.0, 1.6, 0.06), 0.36, 0.012, p["rust"], verts=16)
    # Street dressing so the tile is not bare: bench, bin, shelter.
    kit.box("BenchSeat", (4.0, ROAD / 2 + 1.0, 0.46), (1.8, 0.5, 0.08), p["wood_light"])
    kit.box("BenchBack", (4.0, ROAD / 2 + 1.24, 0.72), (1.8, 0.06, 0.50), p["wood_light"])
    for i, x in enumerate((3.2, 4.8)):
        kit.box("BenchLeg_%d" % (i + 1), (x, ROAD / 2 + 1.0, 0.22), (0.10, 0.44, 0.44),
                p["concrete"])
    kit.cyl("Bin", (6.0, ROAD / 2 + 1.0, 0.40), 0.26, 0.75, p["metal"], verts=12)
    kit.box("ShelterRoof", (-6.0, ROAD / 2 + 1.7, 2.6), (4.0, 1.6, 0.08), p["steel"])
    for i, x in enumerate((-7.8, -4.2)):
        kit.cyl("ShelterPost_%d" % (i + 1), (x, ROAD / 2 + 2.3, 1.3), 0.05, 2.6,
                p["steel"], verts=8)
    kit.box("ShelterGlass", (-6.0, ROAD / 2 + 2.3, 1.4), (3.6, 0.04, 1.8), p["glass"])
    kit.box("ShelterSign", (-6.0, ROAD / 2 + 2.2, 2.30), (3.4, 0.06, 0.30), p["neon_cyan"])


SPECS = [
    AssetSpec("PRP_LampPost_ML_v001", (0.42, 1.86, 9.16), lamp_post, note="9m street lamp"),
    AssetSpec("PRP_Bench_ML_v001", (1.80, 0.56, 1.06), bench, note="park bench"),
    AssetSpec("PRP_Bin_ML_v001", (0.60, 0.60, 0.82), bin_prop, note="street bin"),
    AssetSpec("PRP_Signage_ML_v001", (0.40, 1.96, 3.32), signage, note="lit shop sign"),
    AssetSpec("PRP_VendorCart_ML_v001", (2.10, 1.10, 2.05), vendor_cart, note="food cart"),
    AssetSpec("PRP_TrafficLight_ML_v001", (2.70, 0.50, 4.40), traffic_light, note="signal"),
    AssetSpec("PRP_Pothole_ML_v001", (1.50, 1.20, 0.04), pothole, note="road defect"),
    AssetSpec("STR_CityStreet_ML_v001", (24.4, 10.4, 6.05), city_street,
              note="24x10m modular tile"),
]

