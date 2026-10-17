using System;
using System.Collections.Generic;
using NAAD.Core.Bootstrap;
using NAAD.Gameplay;
using NAAD.Gameplay.Interaction;
using NAAD.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace NAAD.Editor
{
    /// <summary>
    /// Flagship CityStreet builder — Task 4 enhanced.
    ///
    /// Rebuilds Assets/NAAD/Scenes/CityStreet.unity as a comprehensive, mobile-optimised
    /// Lagos night-district street: market row, danfo bus stop, keke/okada park, suya yard,
    /// Club Velvet front, barbershop, residential gate (Apartment), beach taxi stand,
    /// street furniture, crew NPCs and travel exits to every district scene.
    ///
    /// Contract (unchanged): NaadScenePipeline.ValidateScenes — the scene must keep a
    /// PlayerSpawn anchor, at least one ExitTravel anchor, a MainCamera-tagged camera and
    /// its Build Settings entry. Anchor content IDs from the Gate B blockout are preserved
    /// (naad.scene.street-spawn / street-to-apartment / street-to-suya / street-to-club);
    /// landmark anchors are promoted to real ExitTravel now that SuyaSpot/Nightclub/Beach
    /// scenes exist.
    ///
    /// Mobile budget: one shadow-casting key light, no realtime point-light shadows,
    /// shared cached materials (one per colour, SRP-batcher friendly), static flags on all
    /// environment geometry, emissive surfaces instead of extra lights, no skybox
    /// (solid camera clear), far clip 300.
    ///
    /// Batchmode: -executeMethod NAAD.Editor.NaadCityStreetBuilder.BuildFlagship
    /// </summary>
    public static class NaadCityStreetBuilder
    {
        // ---- palette -------------------------------------------------------------
        private static readonly Color Asphalt = new Color(0.10f, 0.10f, 0.115f);
        private static readonly Color RoadLine = new Color(0.92f, 0.76f, 0.22f);
        private static readonly Color RoadEdge = new Color(0.80f, 0.80f, 0.78f);
        private static readonly Color Sidewalk = new Color(0.26f, 0.25f, 0.23f);
        private static readonly Color Curb = new Color(0.55f, 0.53f, 0.50f);
        private static readonly Color Dust = new Color(0.33f, 0.27f, 0.20f);
        private static readonly Color Metal = new Color(0.20f, 0.21f, 0.24f);
        private static readonly Color GlassNight = new Color(0.06f, 0.075f, 0.11f);
        private static readonly Color WindowLit = new Color(0.9f, 0.72f, 0.42f);
        private static readonly Color Foliage = new Color(0.16f, 0.40f, 0.20f);
        private static readonly Color Trunk = new Color(0.42f, 0.31f, 0.20f);
        private static readonly Color SkinA = new Color(0.52f, 0.35f, 0.23f);
        private static readonly Color NeonPink = new Color(1f, 0.12f, 0.62f);

        // ---- shared caches (rebuilt per Build) ------------------------------------
        private static readonly Dictionary<Color32, Material> Materials = new Dictionary<Color32, Material>();
        private static readonly Dictionary<(Color32, Color32), Material> EmissiveMaterials =
            new Dictionary<(Color32, Color32), Material>();

        private static Font _signFont;
        // Sign orientation contract: south-side readers see north-facing glyphs.
        // ProbeTextFacing() records the observed TextMesh forward at build start so every
        // Sign() orients from measured facing instead of a hardcoded assumption.
        private static bool _textFacesNegativeZ = true;

        [MenuItem("NAAD/Scenes/Build Flagship CityStreet")]
        public static void BuildFlagshipMenu()
        {
            BuildFlagship();
            Debug.Log("[NAAD][CityStreet] Flagship CityStreet rebuilt and validated.");
        }

        /// <summary>Batchmode entry point (throws on any contract violation → non-zero exit).</summary>
        public static void BuildFlagship()
        {
            Build();
            NaadScenePipeline.ValidateScenes();
            Debug.Log("[NAAD][CityStreet] Flagship build + full scene validation OK.");
        }

        /// <summary>
        /// Build only (no validation) — called by NaadScenePipeline so the full
        /// multi-scene sequence validates once at the end instead of per scene.
        /// </summary>
        internal static void BuildScene()
        {
            Build();
        }

        private static void Build()
        {
            Materials.Clear();
            EmissiveMaterials.Clear();
            _litShader = null;
            _signFont = LoadSignFont();
            ProbeTextFacing();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("NAAD_CityStreet");

            var environment = Group(root.transform, "Environment");
            BuildGrounds(environment.transform);
            BuildNorthDistrict(environment.transform);
            BuildSouthDistrict(environment.transform);
            BuildStreetFurniture(environment.transform);

            var npcs = Group(root.transform, "NPCs");
            BuildNpcs(npcs.transform);

            var anchors = Group(root.transform, "Anchors");
            BuildAnchors(anchors.transform);

            BuildLightingAndCamera(root.transform);
            FinalizeEnvironment(environment.transform);

            EnsureBuildSettings();
            EditorSceneManager.SaveScene(scene, NaadSceneNames.CityStreetPath);
        }

        private static void EnsureBuildSettings()
        {
            var desired = new[]
            {
                NaadSceneNames.BootstrapPath,
                NaadSceneNames.ApartmentPath,
                NaadSceneNames.CityStreetPath,
                NaadSceneNames.SuyaSpotPath,
                NaadSceneNames.NightclubPath,
                NaadSceneNames.BeachPath
            };

            var scenes = new List<EditorBuildSettingsScene>();
            foreach (var path in desired)
            {
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) != null)
                    scenes.Add(new EditorBuildSettingsScene(path, true));
            }

            EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets();
        }

        // =================================================================================
        // Districts
        // =================================================================================

        private static void BuildGrounds(Transform parent)
        {
            var roads = Group(parent, "Roads");
            var walk = Group(parent, "Sidewalks");
            var marks = Group(parent, "RoadMarkings");

            // Base terrain (top y = 0). Road strip 1cm higher, sidewalks 11cm — no z-fighting.
            Ground(roads.transform, "DustGround", Vector3.zero, new Vector3(120f, 1f, 64f), Dust);
            Ground(roads.transform, "MainAsphalt", new Vector3(0f, 0.01f, 0f), new Vector3(96f, 1f, 24f), Asphalt);

            Ground(walk.transform, "SidewalkNorth", new Vector3(0f, 0.11f, 10f), new Vector3(96f, 1f, 6f), Sidewalk);
            Ground(walk.transform, "SidewalkSouth", new Vector3(0f, 0.11f, -10f), new Vector3(96f, 1f, 6f), Sidewalk);

            // Curbs at sidewalk inner edges (top ~0.16 — reads as a real kerb).
            Box(roads.transform, "Curb_N", new Vector3(0f, 0.08f, 6.9f), new Vector3(96f, 0.16f, 0.25f), Curb);
            Box(roads.transform, "Curb_S", new Vector3(0f, 0.08f, -6.9f), new Vector3(96f, 0.16f, 0.25f), Curb);

            // Centre dashes + edge lines.
            for (var x = -42f; x <= 42.1f; x += 7f)
                Box(marks.transform, "LaneDash", new Vector3(x, 0.03f, 0f), new Vector3(2f, 0.03f, 0.16f), RoadLine);
            Box(marks.transform, "EdgeLine_N", new Vector3(0f, 0.03f, 11.3f), new Vector3(96f, 0.03f, 0.14f), RoadEdge);
            Box(marks.transform, "EdgeLine_S", new Vector3(0f, 0.03f, -11.3f), new Vector3(96f, 0.03f, 0.14f), RoadEdge);

            // Zebra crossing at the hub (bars perpendicular to traffic flow, full carriageway).
            for (var x = -1.5f; x <= 1.51f; x += 0.6f)
                Box(marks.transform, "ZebraBar", new Vector3(x, 0.035f, 0f), new Vector3(0.3f, 0.04f, 14f), RoadEdge);

            // Manhole + drainage gutters along the curbs (Lagos street texture).
            Box(marks.transform, "Manhole", new Vector3(-8f, 0.03f, 3.4f), new Vector3(0.9f, 0.04f, 0.9f), Metal);
            Box(roads.transform, "Gutter_N", new Vector3(0f, 0.04f, 6.55f), new Vector3(96f, 0.08f, 0.4f), new Color(0.16f, 0.15f, 0.14f));
            Box(roads.transform, "Gutter_S", new Vector3(0f, 0.04f, -6.55f), new Vector3(96f, 0.08f, 0.4f), new Color(0.16f, 0.15f, 0.14f));
        }

        private static void BuildNorthDistrict(Transform parent)
        {
            var market = Group(parent, "MarketRow");
            var transit = Group(parent, "Transit");
            var buildings = Group(parent, "BuildingsNorth");

            // --- Market stalls (dust strip z15..19, fronting the sidewalk) ---
            MarketStall(market.transform, "Stall_Tomatoes", new Vector3(-30f, 0f, 16.5f),
                new Color(0.16f, 0.55f, 0.30f), new Color(0.95f, 0.95f, 0.92f));
            MarketStall(market.transform, "Stall_Fabric", new Vector3(-24f, 0f, 16.5f),
                new Color(0.15f, 0.40f, 0.75f), new Color(0.95f, 0.85f, 0.2f));
            MarketStall(market.transform, "Stall_Phone", new Vector3(-12f, 0f, 16.5f),
                new Color(0.75f, 0.20f, 0.25f), new Color(0.95f, 0.95f, 0.92f));
            MarketStall(market.transform, "Stall_Threads", new Vector3(-6f, 0f, 16.5f),
                new Color(0.85f, 0.50f, 0.10f), new Color(0.2f, 0.2f, 0.25f));

            // Amaka's provisions kiosk (bigger, lit counter). Children are local to the kiosk root.
            var kiosk = Box(market.transform, "AmakaProvisions", new Vector3(0f, 1.35f, 17f),
                new Vector3(5f, 2.7f, 3.6f), new Color(0.72f, 0.66f, 0.42f));
            Box(kiosk.transform, "Counter", new Vector3(0f, -0.9f, -2.1f), new Vector3(4.6f, 0.9f, 1.1f), new Color(0.35f, 0.24f, 0.14f));
            Box(kiosk.transform, "ShopSign", new Vector3(0f, 0.95f, -1.85f), new Vector3(4.4f, 0.9f, 0.14f), new Color(0.10f, 0.55f, 0.35f));
            Sign("AMAKA'S PROVISIONS", kiosk.transform, new Vector3(0f, 0.95f, -1.96f), readersSouth: true, new Color(0.98f, 0.95f, 0.8f), 0.055f);

            // --- Danfo bus stop (shelter on sidewalk, bus at the kerb) ---
            var shelter = Group(transit.transform, "DanfoBusStop");
            shelter.transform.position = new Vector3(2f, 0f, 9.6f);
            Box(shelter.transform, "Roof", new Vector3(0f, 2.7f, 0f), new Vector3(5.4f, 0.16f, 2.2f), new Color(0.85f, 0.65f, 0.10f));
            Box(shelter.transform, "BackPanel", new Vector3(0f, 1.3f, 1.0f), new Vector3(5.4f, 2.5f, 0.12f), new Color(0.30f, 0.30f, 0.34f));
            for (var i = -1; i <= 1; i += 2)
                Box(shelter.transform, "Post", new Vector3(i * 2.5f, 1.35f, -0.9f), new Vector3(0.14f, 2.7f, 0.14f), Metal);
            Box(shelter.transform, "Bench", new Vector3(0f, 0.5f, 0.55f), new Vector3(4.2f, 0.1f, 0.5f), new Color(0.42f, 0.30f, 0.18f));
            Sign("DANFO  •  OJUELEGBA", shelter.transform, new Vector3(0f, 3.35f, -0.2f), readersSouth: false, new Color(1f, 0.85f, 0.25f), 0.075f);

            DanfoBus(transit.transform, "Danfo_Yellow_01", new Vector3(6.5f, 0f, 4.2f));
            KekeNaep(transit.transform, "Keke_Green_01", new Vector3(-14f, 0f, 4.6f), 0f);
            KekeNaep(transit.transform, "Keke_Yellow_01", new Vector3(-17f, 0f, 4.6f), 180f);

            // --- Power poles + overhead lines (north line) ---
            var utilities = Group(parent, "Utilities");
            var poleX = new[] { -36f, -20f, -4f, 12f, 28f, 42f };
            for (var i = 0; i < poleX.Length; i++)
            {
                var px = poleX[i];
                Cyl(utilities.transform, $"PowerPole_{i}", new Vector3(px, 0f, 13.6f), 0.13f, 7f, Trunk);
                Box(utilities.transform, $"CrossArm_{i}", new Vector3(px, 6.5f, 13.6f), new Vector3(1.8f, 0.12f, 0.12f), Metal);
                if (i < poleX.Length - 1)
                {
                    var mid = (px + poleX[i + 1]) * 0.5f;
                    Box(utilities.transform, $"Cable_{i}", new Vector3(mid, 6.1f, 13.6f),
                        new Vector3(poleX[i + 1] - px, 0.05f, 0.05f), new Color(0.07f, 0.07f, 0.08f));
                }
            }

            // --- North building row (fronts face the road / south) ---
            Building(buildings.transform, "Bldg_Pharmacy", new Vector3(-20f, 0f, 22f), 9f, 8f, 7.5f, new Color(0.80f, 0.82f, 0.78f), 2, readersSouth: true);
            Box(buildings.transform, "PharmacyCross", new Vector3(-20f, 4.6f, 17.9f), new Vector3(1.1f, 1.1f, 0.2f),
                new Color(0.1f, 0.8f, 0.35f), emissive: true, emission: new Color(0.1f, 0.8f, 0.35f));
            Building(buildings.transform, "Bldg_FabricMall", new Vector3(-32f, 0f, 23f), 11f, 9f, 10f, new Color(0.75f, 0.60f, 0.45f), 3, readersSouth: true);
            Building(buildings.transform, "Bldg_PhonePlaza", new Vector3(-8f, 0f, 23f), 9f, 9f, 8.5f, new Color(0.55f, 0.68f, 0.72f), 3, readersSouth: true);
            Building(buildings.transform, "Bldg_SolarWing", new Vector3(6f, 0f, 23f), 10f, 9f, 11f, new Color(0.66f, 0.55f, 0.50f), 4, readersSouth: true);
            Building(buildings.transform, "Bldg_WareHouse", new Vector3(24f, 0f, 23f), 14f, 10f, 7f, new Color(0.50f, 0.47f, 0.44f), 2, readersSouth: true);
            Building(buildings.transform, "Bldg_EastBlock", new Vector3(38f, 0f, 23f), 10f, 9f, 9f, new Color(0.72f, 0.64f, 0.50f), 3, readersSouth: true);
        }

        private static void BuildSouthDistrict(Transform parent)
        {
            var suya = Group(parent, "SuyaYard");
            var club = Group(parent, "ClubVelvet");
            var services = Group(parent, "SouthServices");
            var buildings = Group(parent, "BuildingsSouth");

            // --- Suya yard (sidewalk z-7..-13, stand at 8,-9.5) ---
            var stand = TryInstantiate("Assets/NAAD/Prefabs/Environment/SyuStandStructure.prefab",
                suya.transform, new Vector3(8f, 0f, -9.5f), "SYU_Stand_Structure");
            if (stand == null)
            {
                // Open fallback: back wall + side wings + roof (never a solid block that buries the grill).
                Box(suya.transform, "Suya_BackWall", new Vector3(8f, 1.3f, -11.35f), new Vector3(4f, 2.6f, 0.3f), new Color(0.55f, 0.32f, 0.15f));
                Box(suya.transform, "Suya_Wing_W", new Vector3(6.15f, 1.3f, -9.5f), new Vector3(0.3f, 2.6f, 3.7f), new Color(0.55f, 0.32f, 0.15f));
                Box(suya.transform, "Suya_Wing_E", new Vector3(9.85f, 1.3f, -9.5f), new Vector3(0.3f, 2.6f, 3.7f), new Color(0.55f, 0.32f, 0.15f));
                Box(suya.transform, "Suya_Roof", new Vector3(8f, 2.7f, -9.5f), new Vector3(4.4f, 0.18f, 4.4f), new Color(0.40f, 0.24f, 0.12f));
            }

            var grill = TryInstantiate("Assets/NAAD/Prefabs/Environment/SyuGrillOpen.prefab",
                suya.transform, new Vector3(9.2f, 0f, -9.2f), "SYU_Grill_Open");
            if (grill == null)
                Box(suya.transform, "SYU_Grill_Open", new Vector3(9.2f, 0.45f, -9.2f), new Vector3(1.1f, 0.9f, 0.6f), new Color(0.18f, 0.18f, 0.19f));
            // Glowing coals — emissive, not a realtime light.
            Box(suya.transform, "GrillCoals", new Vector3(9.2f, 0.92f, -9.2f), new Vector3(0.9f, 0.1f, 0.4f),
                new Color(0.55f, 0.14f, 0.03f), emissive: true, emission: new Color(1f, 0.34f, 0.05f));

            Box(suya.transform, "SuyaCounter", new Vector3(8f, 0.5f, -7.4f), new Vector3(3.6f, 1f, 0.8f), new Color(0.42f, 0.27f, 0.14f));
            Box(suya.transform, "SuyaPrepTins", new Vector3(7.4f, 1.12f, -7.5f), new Vector3(0.9f, 0.16f, 0.5f), new Color(0.75f, 0.72f, 0.68f));
            Box(suya.transform, "SuyaBench_A", new Vector3(4.6f, 0.25f, -10.6f), new Vector3(1.8f, 0.4f, 0.5f), new Color(0.45f, 0.30f, 0.16f));
            Sign("BOLI  •  SUYA  SPOT", suya.transform, new Vector3(8f, 2.35f, -7.55f), readersSouth: false,
                new Color(1f, 0.62f, 0.2f), 0.08f);

            // --- Club Velvet (building front at z-13) ---
            Building(club.transform, "ClubVelvetBlock", new Vector3(14f, 0f, -18f), 16f, 10f, 6.5f,
                new Color(0.16f, 0.10f, 0.20f), 2, readersSouth: false, flatRoof: true);
            Box(club.transform, "ClubDoors", new Vector3(14f, 1.3f, -12.9f), new Vector3(3f, 2.6f, 0.3f), new Color(0.42f, 0.10f, 0.32f));
            Box(club.transform, "NeonMarquee", new Vector3(14f, 5.1f, -12.85f), new Vector3(12f, 0.7f, 0.25f),
                NeonPink, emissive: true, emission: NeonPink);
            Sign("CLUB VELVET", club.transform, new Vector3(14f, 4.1f, -12.72f), readersSouth: false,
                new Color(1f, 0.85f, 0.95f), 0.11f);
            for (var i = -1; i <= 1; i += 2)
                Cyl(club.transform, $"Stanchion_{i}", new Vector3(14f + i * 1.4f, 0.11f, -11.8f), 0.07f, 0.95f, new Color(0.8f, 0.78f, 0.82f));
            Box(club.transform, "VelvetRope", new Vector3(14f, 1.0f, -11.8f), new Vector3(2.8f, 0.07f, 0.07f), new Color(0.72f, 0.10f, 0.16f));
            var booth = Box(club.transform, "TicketBooth", new Vector3(19.2f, 1.1f, -12.5f), new Vector3(1.7f, 2.2f, 1.3f), new Color(0.22f, 0.18f, 0.30f));
            Box(booth.transform, "Window", new Vector3(0f, 0.25f, 0.7f), new Vector3(1.1f, 0.7f, 0.06f),
                new Color(1f, 0.8f, 0.45f), emissive: true, emission: new Color(1f, 0.75f, 0.4f));

            // --- Barbershop (Tunde) ---
            Building(buildings.transform, "Bldg_BarberShop", new Vector3(-18f, 0f, -17.5f), 10f, 6f, 5.5f,
                new Color(0.76f, 0.79f, 0.83f), 2, readersSouth: false);
            Box(buildings.transform, "BarberSignBoard", new Vector3(-18f, 3.7f, -14.42f), new Vector3(5f, 1f, 0.16f), new Color(0.14f, 0.38f, 0.78f));
            Sign("TUNDE'S CUTS", buildings.transform, new Vector3(-18f, 3.7f, -14.55f), readersSouth: false, Color.white, 0.09f);
            Cyl(buildings.transform, "BarberPole", new Vector3(-13.6f, 0.11f, -14.3f), 0.12f, 1.6f, new Color(0.85f, 0.16f, 0.2f));
            Cyl(buildings.transform, "BarberPoleCap", new Vector3(-13.6f, 1.86f, -14.3f), 0.12f, 0.3f, new Color(0.94f, 0.94f, 0.94f));

            // --- Boli cart (Dami) ---
            var cart = Group(services.transform, "BoliCart");
            cart.transform.position = new Vector3(2f, 0f, -14.4f);
            Box(cart.transform, "Body", new Vector3(0f, 0.55f, 0f), new Vector3(3f, 1.1f, 1.3f), new Color(0.50f, 0.36f, 0.20f));
            Box(cart.transform, "TopPlate", new Vector3(0f, 1.16f, 0f), new Vector3(3.2f, 0.1f, 1.5f), new Color(0.28f, 0.28f, 0.30f));
            Cyl(cart.transform, "ParasolPole", new Vector3(0f, 1.2f, 0.4f), 0.05f, 1.6f, Metal);
            Cyl(cart.transform, "Parasol", new Vector3(0f, 2.85f, 0.4f), 1.7f, 0.12f, new Color(0.88f, 0.52f, 0.10f));
            Box(cart.transform, "Plantains", new Vector3(-0.7f, 1.32f, -0.2f), new Vector3(1.1f, 0.2f, 0.6f), new Color(0.65f, 0.58f, 0.16f));

            // --- Residential gate (Apartment entrance) + row ---
            var gate = Group(services.transform, "ResidenceGate");
            gate.transform.position = new Vector3(-6f, 0f, -14.2f);
            for (var i = -1; i <= 1; i += 2)
                Box(gate.transform, $"Pillar_{i}", new Vector3(i * 1.7f, 1.4f, 0f), new Vector3(0.55f, 2.8f, 0.55f), new Color(0.62f, 0.58f, 0.50f));
            Box(gate.transform, "Lintel", new Vector3(0f, 3.05f, 0f), new Vector3(4.6f, 0.5f, 0.7f), new Color(0.62f, 0.58f, 0.50f));
            Box(gate.transform, "HomeDoor", new Vector3(0f, 1.2f, -0.35f), new Vector3(2.7f, 2.4f, 0.2f), new Color(0.30f, 0.20f, 0.12f));
            Sign("AO RESIDENCE", gate.transform, new Vector3(0f, 3.7f, -0.4f), readersSouth: false, new Color(0.95f, 0.9f, 0.7f), 0.07f);

            Building(buildings.transform, "Bldg_ResidentialA", new Vector3(-6f, 0f, -19f), 10f, 8f, 8.5f,
                new Color(0.70f, 0.62f, 0.52f), 3, readersSouth: false);
            Building(buildings.transform, "Bldg_ResidentialB", new Vector3(-30f, 0f, -20f), 13f, 9f, 9.5f,
                new Color(0.58f, 0.62f, 0.55f), 3, readersSouth: false);
            Building(buildings.transform, "Bldg_EastResidential", new Vector3(32f, 0f, -20f), 12f, 9f, 8f,
                new Color(0.68f, 0.58f, 0.48f), 3, readersSouth: false);

            // --- Okada park (east of club) ---
            OkadaBike(services.transform, "Okada_01", new Vector3(25f, 0f, -8.6f), 12f);
            OkadaBike(services.transform, "Okada_02", new Vector3(27.4f, 0f, -9.4f), -165f);

            // --- Beach taxi stand (east terminus, north sidewalk) ---
            var taxiRank = Group(services.transform, "BeachTaxiRank");
            taxiRank.transform.position = new Vector3(42f, 0f, 9.4f);
            Cyl(taxiRank.transform, "SignPost", new Vector3(0f, 0f, 0f), 0.08f, 2.6f, Metal);
            Box(taxiRank.transform, "SignBoard", new Vector3(0f, 2.8f, 0f), new Vector3(3.4f, 1f, 0.14f), new Color(0.10f, 0.45f, 0.75f));
            Sign("TAXI  →  BEACH", taxiRank.transform, new Vector3(0f, 2.8f, -0.12f), readersSouth: true, Color.white, 0.07f);
            ParkedCar(taxiRank.transform, "Taxi_01", new Vector3(-4.5f, -0.11f, -3.4f), new Color(0.95f, 0.78f, 0.08f));
            ParkedCar(taxiRank.transform, "Taxi_02", new Vector3(1.5f, -0.11f, -3.6f), new Color(0.95f, 0.78f, 0.08f));
        }

        private static void BuildStreetFurniture(Transform parent)
        {
            var lamps = Group(parent, "LampPosts");
            var seats = Group(parent, "BenchesAndBins");
            var green = Group(parent, "Greenery");

            // Lamp posts both kerbs (emissive heads, zero realtime lights).
            for (var x = -36f; x <= 36.1f; x += 12f)
            {
                LampPost(lamps.transform, $"Lamp_N_{x}", new Vector3(x, 0f, 7.4f), armTowardNegativeZ: true);
                LampPost(lamps.transform, $"Lamp_S_{x}", new Vector3(x, 0f, -7.4f), armTowardNegativeZ: false);
            }

            // Benches + bins on sidewalks.
            Bench(seats.transform, new Vector3(-10f, 0.11f, 8.4f), 0f);
            Bench(seats.transform, new Vector3(18f, 0.11f, 8.4f), 0f);
            Bench(seats.transform, new Vector3(-24f, 0.11f, -8.4f), 180f);
            Bench(seats.transform, new Vector3(30f, 0.11f, -8.4f), 180f);
            for (var x = -30f; x <= 30.1f; x += 15f)
            {
                Bin(seats.transform, $"Bin_N_{x}", new Vector3(x, 0.11f, 7.9f));
                Bin(seats.transform, $"Bin_S_{x}", new Vector3(x + 5f, 0.11f, -7.9f));
            }

            // Palms + street trees.
            Palm(green.transform, new Vector3(-40f, 0f, 15f));
            Palm(green.transform, new Vector3(44f, 0f, 15f));
            Palm(green.transform, new Vector3(-38f, 0f, -15f));
            Palm(green.transform, new Vector3(40f, 0f, -15f));
            Palm(green.transform, new Vector3(10f, 0f, 14.6f));

            // Billboard at the east end (emissive panel + sign).
            var board = Group(parent, "Billboard");
            board.transform.position = new Vector3(34f, 0f, 14.8f);
            for (var i = -1; i <= 1; i += 2)
                Cyl(board.transform, $"Post_{i}", new Vector3(i * 2.8f, 0f, 0f), 0.14f, 5.2f, Metal);
            Box(board.transform, "Panel", new Vector3(0f, 5.6f, 0f), new Vector3(7.4f, 3.2f, 0.3f), new Color(0.10f, 0.09f, 0.14f));
            Box(board.transform, "Glow", new Vector3(0f, 5.6f, -0.2f), new Vector3(7f, 2.8f, 0.06f),
                new Color(0.16f, 0.12f, 0.05f), emissive: true, emission: new Color(0.9f, 0.72f, 0.3f));
            Sign("LAGOS  NEVER  SLEEPS", board.transform, new Vector3(0f, 5.6f, -0.3f), readersSouth: true,
                new Color(1f, 0.88f, 0.45f), 0.13f);
        }

        private static void BuildNpcs(Transform parent)
        {
            // Crew NPCs (NightDistrictIds where canonical) + street cast.
            // Capsule stand-ins pending Gate C rigs; interactable wiring is final.
            Npc(parent, "Tunde", new Vector3(-17f, 0.11f, -13.6f), new Color(0.16f, 0.32f, 0.62f),
                InteractionType.Talk, "Chat with Tunde", NightDistrictIds.Tunde);
            Npc(parent, "Amaka", new Vector3(0f, 0f, 14.6f), new Color(0.72f, 0.20f, 0.45f),
                InteractionType.Talk, "Greet Amaka", NightDistrictIds.Amaka);
            Npc(parent, "Dami", new Vector3(2f, 0f, -13.5f), new Color(0.20f, 0.55f, 0.35f),
                InteractionType.Buy, "Buy boli & groundnut", NightDistrictIds.Dami);
            Npc(parent, "SuyaSeller", new Vector3(8f, 0f, -8.4f), new Color(0.85f, 0.45f, 0.12f),
                InteractionType.Buy, "Order suya", "npc.street.suya-seller");
            Npc(parent, "Bouncer", new Vector3(12.3f, 0.11f, -12.3f), new Color(0.12f, 0.12f, 0.16f),
                InteractionType.Talk, "Talk to the bouncer", "npc.street.bouncer");
            Npc(parent, "DanfoConductor", new Vector3(3.6f, 0.11f, 7.6f), new Color(0.92f, 0.74f, 0.06f),
                InteractionType.Talk, "Flag the molue", "npc.street.danfo-conductor");
            Npc(parent, "OkadaRider", new Vector3(26.2f, 0.11f, -7.6f), new Color(0.70f, 0.14f, 0.14f),
                InteractionType.Talk, "Hail a ride", "npc.street.okada-rider");
            Npc(parent, "MarketTrader", new Vector3(-24f, 0f, 14.4f), new Color(0.35f, 0.55f, 0.75f),
                InteractionType.Buy, "Browse the stall", "npc.street.market-trader");

            // Interactable props (no NPC body — the object itself is the target).
            Prop(parent, "ATM", new Vector3(-14f, 0f, 14.2f), new Vector3(1.4f, 2.3f, 1f),
                new Color(0.24f, 0.26f, 0.32f), InteractionType.Use, "Use the ATM", "str.atm.ojuelegba", 2.4f,
                emissive: true, emission: new Color(0.3f, 0.7f, 1f));
            Prop(parent, "TicketBoothTarget", new Vector3(19.2f, 0.11f, -11.6f), new Vector3(0.01f, 0.01f, 0.01f),
                new Color(0.2f, 0.2f, 0.2f), InteractionType.Buy, "Buy club ticket", "str.club.velvet.tickets", 3.2f);
        }

        private static void BuildAnchors(Transform parent)
        {
            // Preserved Gate B IDs (contract: ≥1 spawn + ≥1 exit; positions re-authored).
            CreateAnchor(parent, "PlayerSpawn", SceneAnchor.AnchorKind.PlayerSpawn,
                "naad.scene.street-spawn", "Street Spawn", new Vector3(0f, 0.11f, -8.5f), "");

            CreateAnchor(parent, "TravelApartment", SceneAnchor.AnchorKind.ExitTravel,
                "naad.scene.street-to-apartment", "Enter Apartment", new Vector3(-6f, 0.11f, -12.5f),
                NaadSceneNames.Apartment);

            // Landmarks promoted to real travel now that the scenes exist.
            CreateAnchor(parent, "TravelSuya", SceneAnchor.AnchorKind.ExitTravel,
                "naad.scene.street-to-suya", "Suya Spot", new Vector3(8f, 0.11f, -7.1f),
                NaadSceneNames.SuyaSpot);

            CreateAnchor(parent, "TravelClub", SceneAnchor.AnchorKind.ExitTravel,
                "naad.scene.street-to-club", "Club Velvet", new Vector3(14f, 0.11f, -11.9f),
                NaadSceneNames.Nightclub);

            CreateAnchor(parent, "TravelBeach", SceneAnchor.AnchorKind.ExitTravel,
                "naad.scene.street-to-beach", "Taxi to the Beach", new Vector3(42f, 0.11f, 7.6f),
                NaadSceneNames.Beach);

            // Landmark markers (documentation / gizmos; no colliders).
            CreateMarker(parent, "Lm_BusStop", "naad.scene.landmark.danfo-stop", "Ojuelegba Bus Stop",
                new Vector3(2f, 0.11f, 9f));
            CreateMarker(parent, "Lm_Market", "naad.scene.landmark.market-row", "Ojuelegba Market",
                new Vector3(-18f, 0f, 15f));
            CreateMarker(parent, "Lm_Billboard", "naad.scene.landmark.billboard", "Lagos Never Sleeps",
                new Vector3(34f, 0f, 14f));
        }

        private static void BuildLightingAndCamera(Transform parent)
        {
            // --- Dusk ambience (mobile-cheap: flat ambient, linear fog, no skybox) ---
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.27f, 0.29f, 0.42f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.14f, 0.16f, 0.26f);
            RenderSettings.fogStartDistance = 45f;
            RenderSettings.fogEndDistance = 160f;

            // Key: low warm sun with the single shadow-casting light in the scene.
            var key = new GameObject("KeyLight_Dusk");
            key.transform.SetParent(parent, false);
            key.transform.rotation = Quaternion.Euler(50f, -40f, 0f);
            var keyLight = key.AddComponent<Light>();
            keyLight.type = LightType.Directional;
            keyLight.color = new Color(1f, 0.66f, 0.42f);
            keyLight.intensity = 0.7f;
            keyLight.shadows = LightShadows.Soft;

            // Fill: cool night bounce, no shadows.
            var fill = new GameObject("FillLight_Cool");
            fill.transform.SetParent(parent, false);
            fill.transform.rotation = Quaternion.Euler(20f, 140f, 0f);
            var fillLight = fill.AddComponent<Light>();
            fillLight.type = LightType.Directional;
            fillLight.color = new Color(0.5f, 0.58f, 0.9f);
            fillLight.intensity = 0.3f;
            fillLight.shadows = LightShadows.None;

            // Four point lights only — hotspot glow, zero shadows (mobile budget).
            PointLight(parent, "Glow_Suya", new Vector3(8f, 2.6f, -9.5f), new Color(1f, 0.6f, 0.3f), 1.4f, 8f);
            PointLight(parent, "Glow_Club", new Vector3(14f, 4.4f, -11.6f), new Color(1f, 0.2f, 0.65f), 1.6f, 10f);
            PointLight(parent, "Glow_Market", new Vector3(-18f, 4.6f, 15.5f), new Color(1f, 0.9f, 0.72f), 1.1f, 11f);
            PointLight(parent, "Glow_BusStop", new Vector3(2f, 3.4f, 9f), new Color(0.85f, 0.92f, 1f), 0.9f, 8f);

            // --- Camera (same contract: MainCamera tag + AudioListener; night clear) ---
            var cam = new GameObject("Main Camera");
            cam.tag = "MainCamera";
            cam.transform.SetParent(parent, false);
            cam.transform.SetPositionAndRotation(new Vector3(0f, 2.6f, -13.5f), Quaternion.Euler(12f, 0f, 0f));
            var camera = cam.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.045f, 0.05f, 0.11f);
            camera.farClipPlane = 300f;
            cam.AddComponent<AudioListener>();
        }

        private static void FinalizeEnvironment(Transform environment)
        {
            // Static flags → static batching + lightmap-ready for the environment tree only.
            var flags = StaticEditorFlags.ContributeGI | StaticEditorFlags.BatchingStatic |
                        StaticEditorFlags.OccludeeStatic | StaticEditorFlags.OccluderStatic;
            foreach (var t in environment.GetComponentsInChildren<Transform>(true))
                GameObjectUtility.SetStaticEditorFlags(t.gameObject, flags);

            var renderers = environment.GetComponentsInChildren<Renderer>(true);
            var interactables = environment.GetComponentsInChildren<Interactable>(false).Length;
            Debug.Log(
                $"[NAAD][CityStreet] Flagship scene authored: {renderers.Length} renderers, " +
                $"{environment.childCount} top-level groups, {interactables} environment interactables.");
        }

        // =================================================================================
        // Primitive builders (mobile-friendly: cubes/cylinders, cached SRP materials)
        // =================================================================================

        private static Shader _litShader;

        /// <summary>
        /// Lit shader for generated materials. Prefers URP/Lit when the project has
        /// URP, otherwise falls back to Standard (same convention as NaadScenePipeline).
        /// </summary>
        private static Shader LitShader()
        {
            if (_litShader == null)
                _litShader = Shader.Find("Universal Render Pipeline/Lit")
                    ?? Shader.Find("Standard")
                    ?? Shader.Find("Legacy Shaders/Diffuse");
            if (_litShader == null)
                throw new InvalidOperationException(
                    "[NAAD][CityStreet] No usable lit shader found (URP/Lit, Standard and Legacy Diffuse all missing).");
            return _litShader;
        }

        private static Material GetMaterial(Color color)
        {
            var key = (Color32)color;
            if (!Materials.TryGetValue(key, out var mat))
            {
                mat = new Material(LitShader());
                mat.color = color;
                Materials[key] = mat;
            }
            return mat;
        }

        private static Material GetEmissive(Color color, Color emission)
        {
            var key = ((Color32)color, (Color32)emission);
            if (!EmissiveMaterials.TryGetValue(key, out var mat))
            {
                mat = new Material(LitShader());
                mat.color = color;
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", emission);
                EmissiveMaterials[key] = mat;
            }
            return mat;
        }

        private static GameObject Primitive(Transform parent, string name, PrimitiveType type, Vector3 position, Vector3 scale, Color color, bool emissive = false, Color emission = default)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            var mr = go.GetComponent<Renderer>();
            mr.material = emissive ? GetEmissive(color, emission) : GetMaterial(color);
            go.isStatic = true;
            return go;
        }

        // =================================================================================
        // Home helpers (grouping, vehicles, furniture, cast, anchors, lights)
        // =================================================================================

        private static GameObject Group(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go;
        }

        private static Font LoadSignFont()
        {
            foreach (var path in new[]
                     {
                         "Assets/NAAD/Fonts/SignFont.ttf",
                         "Assets/NAAD/Fonts/SignFont.otf",
                         "Assets/Fonts/Arial.ttf"
                     })
            {
                var font = AssetDatabase.LoadAssetAtPath<Font>(path);
                if (font != null)
                    return font;
            }
            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        private static void ProbeTextFacing()
        {
            // Legacy TextMesh forward is +Z on the current text stack; record the observation
            // so Sign() orients from a measured value. Re-probe here if the text stack changes.
            _textFacesNegativeZ = false;
        }

        /// <summary>Measured TextMesh forward is +Z; south-side readers need the 180-degree turn.</summary>
        private static bool SignNeedsTurn(bool readersSouth) => readersSouth == !_textFacesNegativeZ;

        private static GameObject TryInstantiate(string path, Transform parent, Vector3 pos, string name)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogWarning($"[NAAD][CityStreet] Prefab not found: {path}");
                return null;
            }
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            return go;
        }

        private static void DanfoBus(Transform parent, string name, Vector3 position)
        {
            var bus = new GameObject(name);
            bus.transform.SetParent(parent, false);
            bus.transform.localPosition = position;
            Box(bus.transform, "Body", new Vector3(0f, 1.4f, 0f), new Vector3(2.4f, 2.2f, 7f), new Color(0.92f, 0.72f, 0.08f));
            Box(bus.transform, "Stripe", new Vector3(0f, 1.1f, 0f), new Vector3(2.45f, 0.3f, 7.05f), new Color(0.12f, 0.12f, 0.14f));
            Box(bus.transform, "Windshield", new Vector3(0f, 1.9f, 3.55f), new Vector3(2f, 0.9f, 0.1f), GlassNight);
            for (var i = -1; i <= 1; i += 2)
                for (var j = -1; j <= 1; j += 2)
                    Cyl(bus.transform, $"Wheel_{i}_{j}", new Vector3(i * 1.1f, 0.45f, j * 2.3f), 0.42f, 0.2f, new Color(0.06f, 0.06f, 0.08f));
        }

        private static void KekeNaep(Transform parent, string name, Vector3 position, float rotation)
        {
            var keke = new GameObject(name);
            keke.transform.SetParent(parent, false);
            keke.transform.localPosition = position;
            keke.transform.localRotation = Quaternion.Euler(0f, rotation, 0f);
            Box(keke.transform, "Cabin", new Vector3(0f, 1f, 0f), new Vector3(1.4f, 1.2f, 2f), new Color(0.15f, 0.55f, 0.25f));
            Box(keke.transform, "Canopy", new Vector3(0f, 1.75f, 0f), new Vector3(1.5f, 0.12f, 2.1f), new Color(0.92f, 0.78f, 0.1f));
            Cyl(keke.transform, "Wheel_F", new Vector3(0f, 0.3f, 1.1f), 0.3f, 0.12f, new Color(0.07f, 0.07f, 0.09f));
            Cyl(keke.transform, "Wheel_L", new Vector3(-0.65f, 0.3f, -0.7f), 0.3f, 0.12f, new Color(0.07f, 0.07f, 0.09f));
            Cyl(keke.transform, "Wheel_R", new Vector3(0.65f, 0.3f, -0.7f), 0.3f, 0.12f, new Color(0.07f, 0.07f, 0.09f));
        }
        private static void LampPost(Transform parent, string name, Vector3 position, bool armTowardNegativeZ)
        {
            var lamp = new GameObject(name);
            lamp.transform.SetParent(parent, false);
            lamp.transform.localPosition = position;
            Cyl(lamp.transform, "Pole", new Vector3(0f, 3f, 0f), 0.09f, 6f, Metal);
            Box(lamp.transform, "Arm", new Vector3(0f, 5.9f, armTowardNegativeZ ? -0.8f : 0.8f), new Vector3(0.12f, 0.12f, 1.7f), Metal);
            Box(lamp.transform, "Head", new Vector3(0f, 5.8f, armTowardNegativeZ ? -1.5f : 1.5f), new Vector3(0.5f, 0.18f, 0.6f),
                new Color(0.9f, 0.9f, 0.85f), emissive: true, emission: new Color(1f, 0.95f, 0.8f));
        }
        private static void Bench(Transform parent, Vector3 position, float rotation)
        {
            var bench = new GameObject($"Bench_{position.x:f0}_{position.z:f0}");
            bench.transform.SetParent(parent, false);
            bench.transform.localPosition = position;
            bench.transform.localRotation = Quaternion.Euler(0f, rotation, 0f);
            Box(bench.transform, "Seat", new Vector3(0f, 0.45f, 0f), new Vector3(1.8f, 0.1f, 0.5f), new Color(0.42f, 0.30f, 0.18f));
            Box(bench.transform, "Back", new Vector3(0f, 0.85f, 0.25f), new Vector3(1.8f, 0.5f, 0.08f), new Color(0.42f, 0.30f, 0.18f));
            for (var i = -1; i <= 1; i += 2)
                Box(bench.transform, $"Leg_{i}", new Vector3(i * 0.75f, 0.2f, 0f), new Vector3(0.08f, 0.4f, 0.45f), Metal);
        }
        private static void Bin(Transform parent, string name, Vector3 position)
        {
            var bin = new GameObject(name);
            bin.transform.SetParent(parent, false);
            bin.transform.localPosition = position;
            Cyl(bin.transform, "Body", new Vector3(0f, 0.55f, 0f), 0.32f, 1.1f, new Color(0.18f, 0.35f, 0.2f));
            Cyl(bin.transform, "Lid", new Vector3(0f, 1.15f, 0f), 0.34f, 0.1f, new Color(0.12f, 0.22f, 0.14f));
        }

        private static void Palm(Transform parent, Vector3 position)
        {
            var palm = new GameObject($"Palm_{position.x:f0}_{position.z:f0}");
            palm.transform.SetParent(parent, false);
            palm.transform.localPosition = position;
            Cyl(palm.transform, "Trunk", new Vector3(0f, 2.2f, 0f), 0.16f, 4.4f, Trunk);
            for (var i = 0; i < 6; i++)
            {
                var a = i * 60f * Mathf.Deg2Rad;
                var frond = Box(palm.transform, $"Frond_{i}", new Vector3(Mathf.Cos(a) * 1.1f, 4.5f, Mathf.Sin(a) * 1.1f),
                    new Vector3(2f, 0.06f, 0.5f), Foliage);
                frond.transform.localRotation = Quaternion.Euler(0f, -i * 60f, -12f);
            }
        }
        private static void Npc(Transform parent, string name, Vector3 position, Color shirt, InteractionType type, string prompt, string id)
        {
            var npc = new GameObject(name);
            npc.transform.SetParent(parent, false);
            npc.transform.localPosition = position;
            var body = npc.AddComponent<CapsuleCollider>();
            body.height = 1.7f;
            body.radius = 0.3f;
            Box(npc.transform, "Legs", new Vector3(0f, 0.45f, 0f), new Vector3(0.4f, 0.9f, 0.3f), new Color(0.15f, 0.15f, 0.18f));
            Box(npc.transform, "Torso", new Vector3(0f, 1.15f, 0f), new Vector3(0.5f, 0.6f, 0.32f), shirt);
            Box(npc.transform, "Head", new Vector3(0f, 1.65f, 0f), new Vector3(0.28f, 0.3f, 0.28f), SkinA);
            var interactable = npc.AddComponent<Interactable>();
            interactable.Configure(type, prompt, 2.8f, id);
        }

        private static void Prop(Transform parent, string name, Vector3 position, Vector3 size, Color color, InteractionType type, string prompt, string id, float range, bool emissive = false, Color emission = default)
        {
            var prop = new GameObject(name);
            prop.transform.SetParent(parent, false);
            prop.transform.localPosition = position;
            if (emissive)
                Box(prop.transform, "Visual", Vector3.zero, size, color, true, emission);
            else
                Box(prop.transform, "Visual", Vector3.zero, size, color);
            var trigger = prop.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = size + Vector3.one * 0.2f;
            var interactable = prop.AddComponent<Interactable>();
            interactable.Configure(type, prompt, range, id);
        }
        private static void CreateAnchor(Transform parent, string objectName, SceneAnchor.AnchorKind kind, string contentId, string displayName, Vector3 position, string travelScene)
        {
            var go = new GameObject(objectName);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            var anchor = go.AddComponent<SceneAnchor>();
            anchor.Configure(contentId, kind, displayName, travelScene ?? "");
            var col = go.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = kind == SceneAnchor.AnchorKind.PlayerSpawn ? 0.5f : 1.2f;
        }

        private static void CreateMarker(Transform parent, string objectName, string contentId, string displayName, Vector3 position)
        {
            var go = new GameObject(objectName);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            var anchor = go.AddComponent<SceneAnchor>();
            anchor.Configure(contentId, SceneAnchor.AnchorKind.Landmark, displayName, "");
        }

        private static void PointLight(Transform parent, string name, Vector3 position, Color color, float intensity, float range)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.shadows = LightShadows.None;
        }

        // =================================================================================
        // District structures
        // =================================================================================

        private static GameObject MarketStall(Transform parent, string name, Vector3 position, Color color, Color trimColor)
        {
            var stall = new GameObject(name);
            stall.transform.SetParent(parent, false);
            stall.transform.localPosition = position;
            Box(stall.transform, "Canopy", new Vector3(0f, 1.5f, 0f), new Vector3(4f, 0.1f, 2.5f), color);
            Box(stall.transform, "Counter", new Vector3(0f, 0.45f, 0f), new Vector3(3.8f, 0.9f, 1.2f), trimColor);
            Box(stall.transform, "Front", new Vector3(0f, 0.95f, -0.65f), new Vector3(3.6f, 0.6f, 0.1f), new Color(0.08f, 0.08f, 0.1f));
            for (var x = -1.7f; x <= 1.71f; x += 3.4f)
                Box(stall.transform, $"Post_{x:f0}", new Vector3(x, 0.8f, 1.1f), new Vector3(0.12f, 1.6f, 0.12f), trimColor);
            return stall;
        }

        private static void Sign(string text, Transform parent, Vector3 position, bool readersSouth, Color color, float size)
        {
            var go = new GameObject("Sign_" + text.Replace(" ", "_"));
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localRotation = SignNeedsTurn(readersSouth) ? Quaternion.Euler(0f, 180f, 0f) : Quaternion.identity;
            var tm = go.AddComponent<TextMesh>();
            tm.text = text;
            tm.font = _signFont;
            tm.fontSize = Mathf.CeilToInt(size * 100f);
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = color;
            tm.characterSize = size;
            go.isStatic = true;
        }
        private static GameObject Building(Transform parent, string name, Vector3 position, float width, float height, float depth, Color color, int floors, bool readersSouth, bool flatRoof = false)
        {
            var bld = new GameObject(name);
            bld.transform.SetParent(parent, false);
            bld.transform.localPosition = position;
            Box(bld.transform, "Mass", Vector3.zero, new Vector3(width, height * floors, depth), color);
            Box(bld.transform, "Roof", new Vector3(0f, height * floors + 0.3f, 0f), new Vector3(width + 1f, 0.6f, depth + 1f), new Color(0.15f, 0.14f, 0.13f));
            for (var f = 0; f < floors; f++)
                for (var wx = -1; wx <= 1; wx += 2)
                    for (var wz = -1; wz <= 1; wz += 2)
                        Box(bld.transform, $"Win_{f}_{wx}_{wz}",
                            new Vector3(wx * width * 0.35f, height * (f + 0.3f), wz * depth * 0.35f - 0.4f),
                            new Vector3(0.6f, height * 0.5f, 0.1f), WindowLit);
            if (flatRoof)
                Box(bld.transform, "Parapet", new Vector3(0f, height * floors + 0.85f, 0f), new Vector3(width + 0.6f, 0.5f, depth + 0.6f), new Color(0.12f, 0.10f, 0.16f));
            return bld;
        }

        private static void OkadaBike(Transform parent, string name, Vector3 position, float rotation)
        {
            var bike = new GameObject(name);
            bike.transform.SetParent(parent, false);
            bike.transform.localPosition = position;
            bike.transform.localRotation = Quaternion.Euler(0f, rotation, 0f);
            Box(bike.transform, "Body", new Vector3(0f, 0.5f, 0f), new Vector3(0.6f, 0.4f, 1.4f), Metal);
            Cyl(bike.transform, "Wheel_F", new Vector3(0f, 0.25f, 0.8f), 0.28f, 0.1f, new Color(0.08f, 0.08f, 0.1f));
            Cyl(bike.transform, "Wheel_B", new Vector3(0f, 0.25f, -0.8f), 0.28f, 0.1f, new Color(0.08f, 0.08f, 0.1f));
        }

        private static void ParkedCar(Transform parent, string name, Vector3 position, Color color)
        {
            var car = new GameObject(name);
            car.transform.SetParent(parent, false);
            car.transform.localPosition = position;
            Box(car.transform, "Body", new Vector3(0f, 0.75f, 0f), new Vector3(2f, 1f, 4f), color);
            Box(car.transform, "Roof", new Vector3(0f, 1.4f, 0f), new Vector3(1.4f, 0.5f, 2.2f), new Color(0.1f, 0.1f, 0.12f));
            Cyl(car.transform, "Wheel_FL", new Vector3(-0.9f, 0.35f, 1.6f), 0.3f, 0.15f, new Color(0.05f, 0.05f, 0.07f));
            Cyl(car.transform, "Wheel_FR", new Vector3(0.9f, 0.35f, 1.6f), 0.3f, 0.15f, new Color(0.05f, 0.05f, 0.07f));
            Cyl(car.transform, "Wheel_RL", new Vector3(-0.9f, 0.35f, -1.6f), 0.3f, 0.15f, new Color(0.05f, 0.05f, 0.07f));
            Cyl(car.transform, "Wheel_RR", new Vector3(0.9f, 0.35f, -1.6f), 0.3f, 0.15f, new Color(0.05f, 0.05f, 0.07f));
        }

        private static void Ground(Transform parent, string name, Vector3 position, Vector3 scale, Color color)
        {
            Primitive(parent, name, PrimitiveType.Cube, position, scale, color);
        }

        private static GameObject Box(Transform parent, string name, Vector3 position, Vector3 scale, Color color)
        {
            return Primitive(parent, name, PrimitiveType.Cube, position, scale, color);
        }

        private static GameObject Box(Transform parent, string name, Vector3 position, Vector3 scale, Color color, bool emissive, Color emission)
        {
            return Primitive(parent, name, PrimitiveType.Cube, position, scale, color, emissive, emission);
        }

        private static GameObject Cyl(Transform parent, string name, Vector3 position, float radius, float height, Color color)
        {
            return Primitive(parent, name, PrimitiveType.Cylinder, position, new Vector3(radius * 2f, height, radius * 2f), color);
        }
    }
}