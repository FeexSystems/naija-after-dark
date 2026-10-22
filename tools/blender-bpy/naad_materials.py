"""NAAD night-time Lagos palette.

Every material is a flat Principled BSDF - no textures - so the exported GLB/FBX
stay tiny and mobile-friendly. `MAT_<Name>` matches the material naming rule in
`game/unity/Assets/NAAD/Content/ASSET_NAMING_CONVENTION.md` so the Unity import
pipeline can find and tint them later.
"""

from naad_kit import mat


def palette():
    """Return a dict of shared materials (created once per Blender session)."""
    return {
        # structure
        "concrete": mat("MAT_Concrete_Grey_STD", (0.055, 0.056, 0.060), 0.0, 0.9),
        "concrete_light": mat("MAT_Concrete_Light_ML", (0.105, 0.103, 0.098), 0.0, 0.88),
        "asphalt": mat("MAT_Asphalt_Dark_STD", (0.012, 0.012, 0.014), 0.0, 0.95),
        "road_paint": mat("MAT_Paint_RoadWhite_STD", (0.62, 0.60, 0.54), 0.0, 0.85),
        "wood_dark": mat("MAT_Wood_Dark_STD", (0.075, 0.042, 0.024), 0.0, 0.8),
        "wood_mid": mat("MAT_Wood_Mid_ML", (0.150, 0.085, 0.045), 0.0, 0.78),
        "wood_light": mat("MAT_Wood_Light_STD", (0.290, 0.190, 0.105), 0.0, 0.75),
        "bamboo": mat("MAT_Wood_Bamboo_ML", (0.330, 0.245, 0.120), 0.0, 0.7),
        "metal": mat("MAT_Metal_Raw_STD", (0.075, 0.078, 0.085), 0.85, 0.45),
        "steel": mat("MAT_Metal_Steel_STD", (0.330, 0.340, 0.360), 0.9, 0.32),
        "alu": mat("MAT_Metal_Aluminium_STD", (0.480, 0.500, 0.520), 0.75, 0.35),
        "rust": mat("MAT_Metal_Rust_ML", (0.180, 0.070, 0.030), 0.35, 0.85),
        "brass": mat("MAT_Metal_Brass_IS", (0.520, 0.360, 0.110), 1.0, 0.28),
        "black": mat("MAT_Rubber_Black_STD", (0.010, 0.010, 0.012), 0.0, 0.75),
        "glass": mat("MAT_Glass_Clear_STD", (0.470, 0.560, 0.580), 0.25, 0.06, alpha=0.35),
        "glass_dark": mat("MAT_Glass_Smoked_IS", (0.030, 0.040, 0.055), 0.4, 0.08),
        # fabrics
        "fabric_dark": mat("MAT_Fabric_Charcoal_STD", (0.032, 0.032, 0.038), 0.0, 0.95),
        "fabric_plum": mat("MAT_Fabric_Plum_IS", (0.110, 0.030, 0.095), 0.0, 0.92),
        "fabric_teal": mat("MAT_Fabric_Teal_CS", (0.020, 0.115, 0.115), 0.0, 0.92),
        "fabric_orange": mat("MAT_Fabric_BurntOrange_ML", (0.400, 0.110, 0.020), 0.0, 0.88),
        "fabric_cream": mat("MAT_Fabric_Cream_STD", (0.520, 0.480, 0.395), 0.0, 0.9),
        "linen": mat("MAT_Fabric_Linen_APT", (0.560, 0.545, 0.505), 0.0, 0.92),
        "canvas_green": mat("MAT_Canvas_AnkaraGreen_ML", (0.020, 0.150, 0.055), 0.0, 0.9),
        "canvas_red": mat("MAT_Canvas_Red_ML", (0.320, 0.020, 0.020), 0.0, 0.9),
        "tarp_blue": mat("MAT_Tarp_Blue_ML", (0.012, 0.045, 0.190), 0.0, 0.85),
        "plastic_white": mat("MAT_Plastic_White_STD", (0.620, 0.620, 0.600), 0.0, 0.45),
        "plastic_blue": mat("MAT_Plastic_CrateBlue_ML", (0.015, 0.055, 0.330), 0.0, 0.55),
        "plastic_red": mat("MAT_Plastic_Terracotta_ML", (0.330, 0.040, 0.012), 0.0, 0.6),
        "plastic_yellow": mat("MAT_Plastic_SafetyYellow_STD", (0.600, 0.420, 0.010), 0.0, 0.5),
        "screen": mat("MAT_Screen_Emissive_STD", (0.010, 0.012, 0.020), 0.0, 0.2,
                      emission=(0.10, 0.35, 0.60), emission_strength=2.5),
        # vegetation
        "sand": mat("MAT_Sand_Wet_CS", (0.330, 0.245, 0.130), 0.0, 0.95),
        "sand_dry": mat("MAT_Sand_Dry_CS", (0.520, 0.430, 0.290), 0.0, 0.95),
        "foam": mat("MAT_Water_Foam_CS", (0.560, 0.620, 0.640), 0.0, 0.6),
        "leaf": mat("MAT_Leaf_Palm_CS", (0.020, 0.095, 0.030), 0.0, 0.85),
        "leaf_dark": mat("MAT_Leaf_PalmDark_CS", (0.012, 0.055, 0.022), 0.0, 0.85),
        "bark": mat("MAT_Bark_Palm_CS", (0.075, 0.060, 0.040), 0.0, 0.9),
        "charcoal": mat("MAT_Charcoal_Burn_STD", (0.018, 0.016, 0.015), 0.0, 1.0),
        # light / neon (emissive)
        "bulb_warm": mat("MAT_Emissive_Warm_STD", (1.0, 0.62, 0.22), 0.0, 0.3,
                         emission=(1.0, 0.62, 0.22), emission_strength=6.0),
        "bulb_cool": mat("MAT_Emissive_White_STD", (0.85, 0.92, 1.0), 0.0, 0.3,
                         emission=(0.85, 0.92, 1.0), emission_strength=5.0),
        "neon_pink": mat("MAT_Neon_Pink_IS", (0.85, 0.05, 0.45), 0.0, 0.3,
                         emission=(1.0, 0.05, 0.45), emission_strength=8.0),
        "neon_cyan": mat("MAT_Neon_Cyan_IS", (0.02, 0.70, 0.85), 0.0, 0.3,
                         emission=(0.05, 0.85, 1.0), emission_strength=8.0),
        "neon_amber": mat("MAT_Neon_Amber_ML", (1.0, 0.42, 0.03), 0.0, 0.3,
                          emission=(1.0, 0.42, 0.03), emission_strength=7.0),
        "signal_red": mat("MAT_Emissive_Red_STD", (0.80, 0.03, 0.02), 0.0, 0.35,
                          emission=(1.0, 0.04, 0.02), emission_strength=4.0),
        "signal_green": mat("MAT_Emissive_Green_STD", (0.05, 0.75, 0.25), 0.0, 0.35,
                            emission=(0.05, 0.9, 0.3), emission_strength=4.0),
        "fire": mat("MAT_Emissive_Fire_STD", (1.0, 0.35, 0.05), 0.0, 0.5,
                    emission=(1.0, 0.42, 0.06), emission_strength=9.0),
        # skin / cloth for NPC characters
        "skin_1": mat("MAT_Skin_Deep_STD", (0.105, 0.058, 0.040), 0.0, 0.62),
        "skin_2": mat("MAT_Skin_DarkBrown_STD", (0.155, 0.088, 0.060), 0.0, 0.62),
        "skin_3": mat("MAT_Skin_MidBrown_STD", (0.215, 0.125, 0.082), 0.0, 0.62),
        "skin_4": mat("MAT_Skin_LightBrown_STD", (0.330, 0.200, 0.135), 0.0, 0.62),
        "hair_black": mat("MAT_Hair_Black_STD", (0.012, 0.010, 0.010), 0.0, 0.55),
        "cloth_white": mat("MAT_Cloth_White_STD", (0.600, 0.580, 0.545), 0.0, 0.85),
        "cloth_navy": mat("MAT_Cloth_Navy_STD", (0.014, 0.022, 0.055), 0.0, 0.85),
        "cloth_khaki": mat("MAT_Cloth_Khaki_ML", (0.190, 0.165, 0.105), 0.0, 0.88),
        "cloth_gold": mat("MAT_Cloth_Gold_ML", (0.430, 0.260, 0.020), 0.2, 0.45),
        "cloth_teal": mat("MAT_Cloth_Teal_CT", (0.015, 0.100, 0.100), 0.0, 0.85),
        "cloth_red": mat("MAT_Cloth_Red_STD", (0.230, 0.015, 0.020), 0.0, 0.85),
        "cloth_olive": mat("MAT_Cloth_Olive_STD", (0.075, 0.080, 0.040), 0.0, 0.9),
    }
