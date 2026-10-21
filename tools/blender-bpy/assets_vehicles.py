"""Priority 2 - Vehicles (Lagos transport), Island/Mainland.

Lagos vehicles are the city's pulse: Keke Napep (tricycle taxi), Danfo (minibus),
and private cars. All are sized for the player character (1.62 m) to enter/exit.
Every asset faces -Y (forward).
"""

import math

from naad_spec import AssetSpec


# ---------------------------------------------------------------------------
# VHC_KekeNapep_STR_v001 - tricycle taxi, 2.2 x 1.2 m, yellow
# ---------------------------------------------------------------------------
def keke_napep(kit):
    p = kit.p
    # Main body (fiberglass shell)
    kit.box("Body", (0, 0, 0.70), (1.40, 1.20, 0.90), p["plastic_yellow"], bevel=0.03)
    # Windshield
    kit.box("Windshield", (0, 0.45, 1.05), (1.20, 0.06, 0.40), p["glass"], bevel=0.01)
    # Roof
    kit.box("Roof", (0, 0, 1.30), (1.35, 1.15, 0.08), p["plastic_yellow"], bevel=0.02)
    # Rear cargo box
    kit.box("Cargo", (0, -0.65, 0.55), (0.80, 0.40, 0.50), p["plastic_yellow"], bevel=0.02)
    # Front wheel (single)
    kit.cyl("WheelFront", (0, 0.55, 0.25), 0.18, 0.08, p["black"],
            rot=(math.pi / 2, 0, 0), verts=12)
    kit.cyl("WheelFrontRim", (0, 0.55, 0.25), 0.12, 0.08, p["steel"],
            rot=(math.pi / 2, 0, 0), verts=8)
    # Rear wheels (dual)
    for x in (-0.25, 0.25):
        kit.cyl("WheelRear_%s" % ("L" if x < 0 else "R"), (x, -0.55, 0.25), 0.18, 0.08,
                p["black"], rot=(math.pi / 2, 0, 0), verts=12)
        kit.cyl("WheelRearRim_%s" % ("L" if x < 0 else "R"), (x, -0.55, 0.25), 0.12, 0.08,
                p["steel"], rot=(math.pi / 2, 0, 0), verts=8)
    # Handlebars
    kit.box("Handlebar", (0, 0.55, 1.10), (0.60, 0.04, 0.04), p["steel"])
    # Headlight
    kit.cyl("Headlight", (0, 0.55, 0.85), 0.08, 0.04, p["bulb_warm"],
            rot=(math.pi / 2, 0, 0), verts=8)
    # Taillight
    kit.cyl("Taillight", (0, -0.85, 0.75), 0.06, 0.03, p["signal_red"],
            rot=(math.pi / 2, 0, 0), verts=8)
    # Seat
    kit.box("Seat", (0, 0.10, 0.95), (0.50, 0.35, 0.08), p["fabric_dark"], bevel=0.015)


# ---------------------------------------------------------------------------
# VHC_Danfo_STR_v001 - minibus, 4.5 x 1.8 m, yellow/black
# ---------------------------------------------------------------------------
def danfo(kit):
    p = kit.p
    # Main body
    kit.box("Body", (0, 0, 1.10), (4.20, 1.70, 1.80), p["plastic_yellow"], bevel=0.03)
    # Black stripe (Danfo livery)
    kit.box("Stripe", (0, 0, 1.10), (4.22, 1.72, 0.30), p["black"], bevel=0.01)
    # Windshield
    kit.box("Windshield", (1.50, 0.80, 1.80), (1.40, 0.08, 0.70), p["glass"], bevel=0.01)
    # Side windows
    for i, x in enumerate((-0.80, 0.20, 1.20)):
        kit.box("WindowSide_%d" % (i + 1), (x, 0.86, 1.70), (0.80, 0.04, 0.55),
                p["glass"], bevel=0.008)
    # Rear window
    kit.box("WindowRear", (-1.70, 0.86, 1.70), (0.80, 0.04, 0.55), p["glass"], bevel=0.008)
    # Roof
    kit.box("Roof", (0, 0, 2.05), (4.15, 1.65, 0.08), p["plastic_yellow"], bevel=0.015)
    # Roof rack
    kit.box("Rack", (0, 0, 2.12), (3.80, 1.40, 0.06), p["steel"], bevel=0.01)
    # Wheels
    for x in (-1.50, 1.50):
        for side, y in (("L", 0.90), ("R", -0.90)):
            kit.cyl("Wheel_%s_%s" % ("F" if x > 0 else "R", side), (x, y, 0.40), 0.30, 0.12,
                    p["black"], rot=(math.pi / 2, 0, 0), verts=16)
            kit.cyl("WheelRim_%s_%s" % ("F" if x > 0 else "R", side), (x, y, 0.40), 0.20, 0.12,
                    p["alu"], rot=(math.pi / 2, 0, 0), verts=10)
    # Headlights
    for x in (-0.60, 0.60):
        kit.box("Headlight_%s" % ("L" if x < 0 else "R"), (x, 0.88, 1.10),
                (0.20, 0.06, 0.15), p["bulb_warm"], bevel=0.01)
    # Taillights
    for x in (-0.60, 0.60):
        kit.box("Taillight_%s" % ("L" if x < 0 else "R"), (x, -0.88, 1.10),
                (0.20, 0.06, 0.15), p["signal_red"], bevel=0.01)
    # Side mirrors
    for x in (-0.70, 0.70):
        kit.box("Mirror_%s" % ("L" if x < 0 else "R"), (x, 0.92, 1.60),
                (0.12, 0.04, 0.12), p["black"], bevel=0.008)
    # Destination board
    kit.box("DestBoard", (0, 0.88, 1.95), (1.20, 0.02, 0.25), p["plastic_white"])
    kit.box("DestText", (0, 0.88, 1.95), (1.00, 0.01, 0.20), p["black"])


# ---------------------------------------------------------------------------
# VHC_Sedan_STR_v001 - private sedan, 4.0 x 1.6 m, silver
# ---------------------------------------------------------------------------
def sedan(kit):
    p = kit.p
    # Main body
    kit.box("Body", (0, 0, 0.60), (3.80, 1.60, 1.30), p["alu"], bevel=0.025)
    # Hood
    kit.box("Hood", (1.10, 0, 0.85), (1.40, 1.55, 0.12), p["alu"], bevel=0.02)
    # Trunk
    kit.box("Trunk", (-1.10, 0, 0.80), (1.20, 1.55, 0.10), p["alu"], bevel=0.015)
    # Roof
    kit.box("Roof", (0, 0, 1.30), (2.80, 1.45, 0.10), p["alu"], bevel=0.015)
    # Windshield
    kit.box("Windshield", (0.90, 0, 1.05), (1.30, 1.45, 0.35),
                p["glass"], bevel=0.01, rot=(-0.35, 0, 0))
    # Rear window
    kit.box("RearWindow", (-0.90, 0, 1.05), (1.20, 1.45, 0.30),
                p["glass"], bevel=0.01, rot=(0.35, 0, 0))
    # Side windows
    for x in (-0.30, 0.30):
        kit.box("WindowSide_%s" % ("L" if x < 0 else "R"), (x, 0.81, 1.20),
                (0.80, 0.04, 0.45), p["glass"], bevel=0.008)
    # Wheels
    for x in (-1.30, 1.30):
        for side, y in (("L", 0.82), ("R", -0.82)):
            kit.cyl("Wheel_%s_%s" % ("F" if x > 0 else "R", side), (x, y, 0.35), 0.28, 0.10,
                    p["black"], rot=(math.pi / 2, 0, 0), verts=16)
            kit.cyl("WheelRim_%s_%s" % ("F" if x > 0 else "R", side), (x, y, 0.35), 0.18, 0.10,
                    p["steel"], rot=(math.pi / 2, 0, 0), verts=10)
    # Headlights
    for x in (-0.50, 0.50):
        kit.box("Headlight_%s" % ("L" if x < 0 else "R"), (x, 0.80, 0.75),
                (0.18, 0.06, 0.12), p["bulb_cool"], bevel=0.01)
    # Taillights
    for x in (-0.50, 0.50):
        kit.box("Taillight_%s" % ("L" if x < 0 else "R"), (x, -0.80, 0.70),
                (0.18, 0.06, 0.10), p["signal_red"], bevel=0.01)
    # Grille
    kit.box("Grille", (0, 0.80, 0.55), (0.80, 0.04, 0.20), p["black"], bevel=0.005)
    # Side mirrors
    for x in (-0.60, 0.60):
        kit.box("Mirror_%s" % ("L" if x < 0 else "R"), (x, 0.82, 1.05),
                (0.10, 0.04, 0.10), p["black"], bevel=0.008)
    # Door handles
    for x in (-0.30, 0.30):
        kit.box("Handle_%s" % ("L" if x < 0 else "R"), (x, 0.81, 0.85),
                (0.12, 0.02, 0.04), p["black"], bevel=0.005)


SPECS = [
    AssetSpec("VHC_KekeNapep_STR_v001", (2.20, 1.20, 1.35), keke_napep,
              note="tricycle taxi, yellow"),
    AssetSpec("VHC_Danfo_STR_v001", (4.50, 1.80, 2.15), danfo,
              note="minibus, yellow/black"),
    AssetSpec("VHC_Sedan_STR_v001", (4.00, 1.60, 1.45), sedan,
              note="private sedan, silver"),
]

