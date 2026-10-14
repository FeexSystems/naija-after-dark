using System;
using System.Collections.Generic;
using System.Linq;
using NAAD.Core.Bootstrap;
using NAAD.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NAAD.Editor
{
    /// <summary>
    /// Gate B / Prompt 1 — build NAAD Apartment + CityStreet scenes and validate registry.
    /// </summary>
    public static class NaadScenePipeline
    {
        private const string StructurePrefab =
            "Assets/Brick Project Studio/Apartment Kit/_Prefabs/Structures/Structure 01.prefab";
        private const string DemoCityPrefab =
            "Assets/Versatile Studio Assets/Demo City By Versatile Studio/Prefabs/demo_city_by_versatile_studio.prefab";
        private const string MidHousePrefab =
            "Assets/Versatile Studio Assets/Demo City By Versatile Studio/Prefabs/mid_house_1.prefab";
        private const string OfficePrefab =
            "Assets/Versatile Studio Assets/Demo City By Versatile Studio/Prefabs/office_building_1.prefab";
        private const string LampPrefab =
            "Assets/Versatile Studio Assets/Demo City By Versatile Studio/Prefabs/lamp_pole_single_orange.prefab";

        // Gate B Phase 1 — NAAD-authored Environment placeholders.
        private const string RdSurfaceAsphaltPrefab = "Assets/NAAD/Prefabs/Environment/RdSurfaceAsphalt.prefab";
        private const string StrSidewalkStraightPrefab = "Assets/NAAD/Prefabs/Environment/StrSidewalkStraight.prefab";
        private const string SyuStandStructurePrefab = "Assets/NAAD/Prefabs/Environment/SyuStandStructure.prefab";
        private const string SyuGrillOpenPrefab = "Assets/NAAD/Prefabs/Environment/SyuGrillOpen.prefab";
        private const string NclEntranceDoorLargePrefab = "Assets/NAAD/Prefabs/Environment/NclEntranceDoorLarge.prefab";
        private const string NclStagePlatformPrefab = "Assets/NAAD/Prefabs/Environment/NclStagePlatform.prefab";

        [MenuItem("NAAD/Scenes/Build Apartment And CityStreet")]
        public static void BuildApartmentAndCityStreet()
        {
            EnsureFolders();
            BuildApartmentScene();
            BuildCityStreetScene();
            BuildSuyaSpotScene();
            BuildNightclubScene();
            BuildBeachScene();
            EnsureBuildSettings();
            ValidateScenes();
            Debug.Log("[NAAD][Scenes] Apartment + CityStreet + SuyaSpot + Nightclub + Beach built and validated.");
        }

        [MenuItem("NAAD/Scenes/Validate Registered Scenes")]
        public static void ValidateScenesMenu() => ValidateScenes();

        public static void ValidateScenes()
        {
            var errors = new List<string>();
            ValidateSceneFile(NaadSceneNames.BootstrapPath, errors);
            ValidateSceneFile(NaadSceneNames.ApartmentPath, errors);
            ValidateSceneFile(NaadSceneNames.CityStreetPath, errors);
            ValidateSceneFile(NaadSceneNames.SuyaSpotPath, errors);
            ValidateSceneFile(NaadSceneNames.NightclubPath, errors);
            ValidateSceneFile(NaadSceneNames.BeachPath, errors);

            ValidateBuildEntry(0, NaadSceneNames.BootstrapPath, errors);
            ValidateBuildContains(NaadSceneNames.ApartmentPath, errors);
            ValidateBuildContains(NaadSceneNames.CityStreetPath, errors);
            ValidateBuildContains(NaadSceneNames.SuyaSpotPath, errors);
            ValidateBuildContains(NaadSceneNames.NightclubPath, errors);
            ValidateBuildContains(NaadSceneNames.BeachPath, errors);

            ValidateAnchors(NaadSceneNames.ApartmentPath, requireSpawn: true, requireExit: true, errors);
            ValidateAnchors(NaadSceneNames.CityStreetPath, requireSpawn: true, requireExit: true, errors);
            ValidateAnchors(NaadSceneNames.SuyaSpotPath, requireSpawn: true, requireExit: true, errors);
            ValidateAnchors(NaadSceneNames.NightclubPath, requireSpawn: true, requireExit: true, errors);
            ValidateAnchors(NaadSceneNames.BeachPath, requireSpawn: true, requireExit: true, errors);

            if (errors.Count > 0)
                throw new Exception("NAAD scene validation failed:\n - " + string.Join("\n - ", errors));

            Debug.Log("[NAAD][Scenes] Validation passed for Bootstrap, Apartment, CityStreet, SuyaSpot, Nightclub, Beach.");
        }

        [MenuItem("NAAD/Scenes/Validate Vendor Intake")]
        public static void ValidateVendorIntakeMenu() => ValidateVendorIntake();

        /// <summary>
        /// Non-fatal vendor intake check (see Content/ASSET_INTAKE_MAPPING.md).
        /// Reports the six gitignored Asset Store pack roots and the five scene-critical
        /// prefabs already referenced by Bootstrap/Apartment/CityStreet.
        /// Never throws on absence: a fresh clone without vendor packs must still pass
        /// batchmode verify. It is intentionally NOT wired into ValidateScenes, which
        /// keeps failing only on genuine scene/build errors.
        /// </summary>
        public static void ValidateVendorIntake()
        {
            var packRoots = new[]
            {
                "Assets/Brick Project Studio",
                "Assets/Versatile Studio Assets",
                "Assets/Kevin Iglesias",
                "Assets/Awbmecreations",
                "Assets/Marcos Schultz",
                "Assets/PalmTreePack"
            };

            var missingPacks = packRoots.Where(p => !AssetDatabase.IsValidFolder(p)).ToList();
            var scenePrefabs = new[] { StructurePrefab, DemoCityPrefab, MidHousePrefab, OfficePrefab, LampPrefab };
            var missingPrefabs = scenePrefabs.Where(p => AssetDatabase.LoadAssetAtPath<GameObject>(p) == null).ToList();

            if (missingPacks.Count == 0 && missingPrefabs.Count == 0)
            {
                Debug.Log("[NAAD][Intake] Vendor intake OK: 6/6 packs and 5/5 scene prefabs present.");
                return;
            }

            foreach (var pack in missingPacks)
                Debug.LogWarning($"[NAAD][Intake] Missing vendor pack: {pack}");
            foreach (var prefab in missingPrefabs)
                Debug.LogWarning($"[NAAD][Intake] Missing scene prefab: {prefab}");

            Debug.LogWarning(
                $"[NAAD][Intake] Vendor intake incomplete ({packRoots.Length - missingPacks.Count}/{packRoots.Length} packs, " +
                $"{scenePrefabs.Length - missingPrefabs.Count}/{scenePrefabs.Length} scene prefabs). " +
                "Non-fatal by design - re-import per ASSET_INTAKE_MAPPING.md (section 2).");
        }

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/NAAD/Scenes"))
                AssetDatabase.CreateFolder("Assets/NAAD", "Scenes");
            if (!AssetDatabase.IsValidFolder("Assets/NAAD/Prefabs"))
                AssetDatabase.CreateFolder("Assets/NAAD", "Prefabs");
            if (!AssetDatabase.IsValidFolder("Assets/NAAD/Prefabs/Environment"))
                AssetDatabase.CreateFolder("Assets/NAAD/Prefabs", "Environment");
        }

        private static void BuildApartmentScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("NAAD_Apartment");

            CreateDirectionalLight(new Vector3(50f, -30f, 0f), new Color(1f, 0.85f, 0.7f), 1.1f);
            CreateGround("ApartmentFloor", new Vector3(0f, 0f, 0f), new Vector3(40f, 1f, 40f), new Color(0.22f, 0.2f, 0.18f));

            var structure = TryInstantiate(StructurePrefab, root.transform, Vector3.zero, "ApartmentStructure");
            if (structure == null)
            {
                // Fallback blockout if Structure 01 is unavailable.
                CreateBox("AptShell", root.transform, new Vector3(0f, 1.5f, 0f), new Vector3(10f, 3f, 8f), new Color(0.45f, 0.4f, 0.35f));
            }

            CreateMainCamera(new Vector3(0f, 1.6f, -4f), Quaternion.Euler(8f, 0f, 0f));

            CreateAnchor("PlayerSpawn", SceneAnchor.AnchorKind.PlayerSpawn,
                NightDistrictIdsSafe("apartment-spawn"), "Apartment Spawn",
                new Vector3(0f, 0.1f, -2f), "");

            CreateAnchor("ExitToStreet", SceneAnchor.AnchorKind.ExitTravel,
                NightDistrictIdsSafe("apartment-exit-street"), "Exit to City Street",
                new Vector3(0f, 0.1f, 4.5f), NaadSceneNames.CityStreet);

            CreateAnchor("PhoneInteract", SceneAnchor.AnchorKind.Interaction,
                NightDistrictIdsSafe("apartment-phone"), "Phone",
                new Vector3(1.5f, 0.1f, -1f), "");

            CreateAnchor("WardrobeInteract", SceneAnchor.AnchorKind.Interaction,
                NightDistrictIdsSafe("apartment-wardrobe"), "Wardrobe",
                new Vector3(-1.8f, 0.1f, 0.5f), "");

            EditorSceneManager.SaveScene(scene, NaadSceneNames.ApartmentPath);
        }

        private static void BuildCityStreetScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("NAAD_CityStreet");

            CreateDirectionalLight(new Vector3(35f, -40f, 10f), new Color(0.55f, 0.65f, 1f), 0.85f);
            // Warm night fill
            var fill = new GameObject("Fill Light");
            var fillLight = fill.AddComponent<Light>();
            fillLight.type = LightType.Directional;
            fillLight.color = new Color(1f, 0.55f, 0.25f);
            fillLight.intensity = 0.35f;
            fill.transform.rotation = Quaternion.Euler(15f, 140f, 0f);

            CreateGround("StreetAsphalt", Vector3.zero, new Vector3(80f, 1f, 24f), new Color(0.12f, 0.12f, 0.13f));
            CreateGround("SidewalkNorth", new Vector3(0f, 0.05f, 10f), new Vector3(80f, 1f, 6f), new Color(0.25f, 0.24f, 0.22f));
            CreateGround("SidewalkSouth", new Vector3(0f, 0.05f, -10f), new Vector3(80f, 1f, 6f), new Color(0.25f, 0.24f, 0.22f));

            // Gate B Phase 1 — NAAD-authored Environment placeholders, with
            // blockout fallback so fresh clones still build the scene.
            // Road tiles (8m modules) along the street axis; sidewalks north.
            var roadStartX = -9f;
            for (var i = 0; i < 3; i++)
            {
                var p = new Vector3(roadStartX + i * 8f, 0f, 0f);
                if (TryInstantiate(RdSurfaceAsphaltPrefab, root.transform, p, $"RoadTile_{i}") == null)
                    CreateBox($"RoadTile_{i}_Fallback", root.transform, p + Vector3.up * 0.25f,
                        new Vector3(8f, 0.5f, 6f), new Color(0.12f, 0.12f, 0.13f));
                var sp = new Vector3(roadStartX + i * 8f, 0f, 10f);
                if (TryInstantiate(StrSidewalkStraightPrefab, root.transform, sp, $"SidewalkTile_{i}") == null)
                    CreateBox($"SidewalkTile_{i}_Fallback", root.transform, sp + Vector3.up * 0.1f,
                        new Vector3(8f, 0.2f, 3f), new Color(0.25f, 0.24f, 0.22f));
            }

            // Suya cluster near the suya landmark (8, 0, -4).
            var suyaPos = new Vector3(8f, 0f, -4f);
            if (TryInstantiate(SyuStandStructurePrefab, root.transform, suyaPos, "SuyaStand") == null)
                CreateBox("SuyaStand_Fallback", root.transform, suyaPos + Vector3.up * 1.4f,
                    new Vector3(4f, 2.8f, 4f), new Color(0.55f, 0.35f, 0.12f));
            if (TryInstantiate(SyuGrillOpenPrefab, root.transform, suyaPos + new Vector3(1.2f, 0f, 0.5f), "SuyaGrill") == null)
                CreateBox("SuyaGrill_Fallback", root.transform, suyaPos + new Vector3(1.2f, 0.65f, 0.5f),
                    new Vector3(1f, 0.5f, 0.5f), new Color(0.18f, 0.18f, 0.19f));

            // Nightclub cluster near the club landmark (14, 0, -4).
            var clubPos = new Vector3(14f, 0f, -4f);
            if (TryInstantiate(NclEntranceDoorLargePrefab, root.transform, clubPos + new Vector3(0f, 0f, 3f), "ClubDoors") == null)
                CreateBox("ClubDoors_Fallback", root.transform, clubPos + new Vector3(0f, 1.25f, 3f),
                    new Vector3(3f, 2.5f, 0.3f), new Color(0.35f, 0.55f, 0.75f));
            if (TryInstantiate(NclStagePlatformPrefab, root.transform, clubPos + new Vector3(0f, 0f, -6f), "ClubStage") == null)
                CreateBox("ClubStage_Fallback", root.transform, clubPos + new Vector3(0f, 0.3f, -6f),
                    new Vector3(8f, 0.6f, 4f), new Color(0.30f, 0.22f, 0.16f));

            // Compact street block — prefer modular houses over the full city mega-prefab.
            TryInstantiate(MidHousePrefab, root.transform, new Vector3(-18f, 0f, 14f), "House_A");
            TryInstantiate(MidHousePrefab, root.transform, new Vector3(0f, 0f, 14f), "House_B");
            TryInstantiate(OfficePrefab, root.transform, new Vector3(20f, 0f, 16f), "Office_A");
            TryInstantiate(LampPrefab, root.transform, new Vector3(-10f, 0f, -8f), "Lamp_A");
            TryInstantiate(LampPrefab, root.transform, new Vector3(10f, 0f, -8f), "Lamp_B");

            // Optional distant city silhouette if available (kept inactive for mobile budget).
            var city = TryInstantiate(DemoCityPrefab, root.transform, new Vector3(0f, 0f, 60f), "CityBackdrop");
            if (city != null)
            {
                city.SetActive(false);
                city.transform.localScale = Vector3.one * 0.35f;
            }

            CreateMainCamera(new Vector3(0f, 2.2f, -12f), Quaternion.Euler(12f, 0f, 0f));

            CreateAnchor("PlayerSpawn", SceneAnchor.AnchorKind.PlayerSpawn,
                NightDistrictIdsSafe("street-spawn"), "Street Spawn",
                new Vector3(0f, 0.1f, -6f), "");

            CreateAnchor("TravelApartment", SceneAnchor.AnchorKind.ExitTravel,
                NightDistrictIdsSafe("street-to-apartment"), "Enter Apartment",
                new Vector3(-6f, 0.1f, -4f), NaadSceneNames.Apartment);

            CreateAnchor("TravelSuyaMarker", SceneAnchor.AnchorKind.Landmark,
                NightDistrictIdsSafe("street-to-suya"), "Suya Spot (planned)",
                new Vector3(8f, 0.1f, -4f), "");

            CreateAnchor("TravelClubMarker", SceneAnchor.AnchorKind.Landmark,
                NightDistrictIdsSafe("street-to-club"), "Nightclub (planned)",
                new Vector3(14f, 0.1f, -4f), "");

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

            var scenes = desired
                .Where(p => AssetDatabase.LoadAssetAtPath<SceneAsset>(p) != null)
                .Select(p => new EditorBuildSettingsScene(p, true))
                .ToArray();

            EditorBuildSettings.scenes = scenes;
            AssetDatabase.SaveAssets();
        }

        private static void BuildSuyaSpotScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("SuyaSpot");
            var content = new GameObject("Content");
            content.transform.SetParent(root.transform, false);
            CreateGround("Ground", new Vector3(0f, 0.52f, 0f), new Vector3(14f, 0.1f, 12f), new Color(0.23f, 0.18f, 0.13f));

            var stand = TryInstantiate(SyuStandStructurePrefab, content.transform, new Vector3(0f, 0f, 0f), "SYU_Stand_Structure");
            if (stand == null)
                CreateBox("SYU_Stand_Structure", content.transform, new Vector3(0f, 1.1f, 0f), new Vector3(4f, 2.2f, 3f), new Color(0.55f, 0.32f, 0.15f));

            var grill = TryInstantiate(SyuGrillOpenPrefab, content.transform, new Vector3(2.6f, 0f, 0.6f), "SYU_Grill_Open");
            if (grill == null)
                CreateBox("SYU_Grill_Open", content.transform, new Vector3(2.6f, 0.55f, 0.6f), new Vector3(1.2f, 0.9f, 0.8f), Color.black);

            CreateBox("SYU_Counter", content.transform, new Vector3(-2.6f, 0.55f, 0.4f), new Vector3(1.6f, 1.1f, 0.9f), new Color(0.4f, 0.25f, 0.12f));
            CreateBox("SYU_Seating_Bench_A", content.transform, new Vector3(0f, 0.3f, 3.4f), new Vector3(2.4f, 0.5f, 0.6f), new Color(0.45f, 0.3f, 0.16f));
            CreateBox("SYU_Seating_Bench_B", content.transform, new Vector3(0f, 0.3f, 4.4f), new Vector3(2.4f, 0.5f, 0.6f), new Color(0.45f, 0.3f, 0.16f));
            CreateBox("SYU_Lighting_Warm", content.transform, new Vector3(0f, 2.6f, 1.2f), new Vector3(0.5f, 0.3f, 0.5f), new Color(1f, 0.75f, 0.4f));

            CreateAnchor("Spawn_SuyaSpot", SceneAnchor.AnchorKind.PlayerSpawn, NightDistrictIdsSafe("suya-spot"),
                "Suya Spot Spawn", new Vector3(0f, 0f, -4f), NaadSceneNames.CityStreet);
            CreateAnchor("Exit_SuyaSpot_To_CityStreet", SceneAnchor.AnchorKind.ExitTravel, NightDistrictIdsSafe("suya-spot-exit"),
                "Exit to City Street", new Vector3(0f, 0f, -5f), NaadSceneNames.CityStreet);
            CreateAnchor("Vendor_MamaSeyi", SceneAnchor.AnchorKind.Interaction, "npc.crew.mama-seyi",
                "Mama Seyi", new Vector3(1.2f, 0f, 1.6f), null);
            CreateAnchor("Interact_Grill", SceneAnchor.AnchorKind.Interaction, "syu.grill.open",
                "Charcoal Grill", new Vector3(2.6f, 0f, 0.6f), null);
            CreateAnchor("Interact_Counter", SceneAnchor.AnchorKind.Interaction, "syu.counter.order",
                "Order Counter", new Vector3(-2.6f, 0f, 0.4f), null);

            CreateMainCamera(new Vector3(0f, 2.2f, -7.5f), Quaternion.Euler(12f, 0f, 0f));
            CreateDirectionalLight(new Vector3(50f, -30f, 0f), new Color(1f, 0.85f, 0.6f), 0.9f);
            EditorSceneManager.SaveScene(scene, NaadSceneNames.SuyaSpotPath);
        }

        private static void BuildNightclubScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("Nightclub");
            var content = new GameObject("Content");
            content.transform.SetParent(root.transform, false);
            CreateGround("Ground", new Vector3(0f, 0.46f, 0f), new Vector3(20f, 0.1f, 16f), new Color(0.1f, 0.08f, 0.14f));

            var entrance = TryInstantiate(NclEntranceDoorLargePrefab, content.transform, new Vector3(0f, 0f, 6f), "NCL_Entrance_Door_Large");
            if (entrance == null)
                CreateBox("NCL_Entrance_Door_Large", content.transform, new Vector3(0f, 1.25f, 6f), new Vector3(3f, 2.5f, 0.4f), new Color(0.6f, 0.1f, 0.4f));

            var stage = TryInstantiate(NclStagePlatformPrefab, content.transform, new Vector3(0f, 0f, -5f), "NCL_Stage_Platform");
            if (stage == null)
                CreateBox("NCL_Stage_Platform", content.transform, new Vector3(0f, 0.3f, -5f), new Vector3(8f, 0.6f, 4f), new Color(0.15f, 0.1f, 0.25f));

            CreateBox("NCL_Bar_Counter", content.transform, new Vector3(-6.5f, 0.55f, 0f), new Vector3(3.5f, 1.1f, 1.2f), new Color(0.25f, 0.08f, 0.2f));
            CreateBox("NCL_Booth_VIP_A", content.transform, new Vector3(6.5f, 0.5f, -1f), new Vector3(2.5f, 1f, 2.5f), new Color(0.4f, 0.1f, 0.35f));
            CreateBox("NCL_Booth_VIP_B", content.transform, new Vector3(6.5f, 0.5f, 2.5f), new Vector3(2.5f, 1f, 2.5f), new Color(0.4f, 0.1f, 0.35f));
            CreateBox("NCL_Lighting_Neon", content.transform, new Vector3(0f, 3.4f, 0f), new Vector3(6f, 0.3f, 0.6f), new Color(1f, 0f, 0.55f));
            CreateBox("NCL_DanceFloor", content.transform, new Vector3(0f, 0.06f, 0.5f), new Vector3(6f, 0.12f, 5f), new Color(0.12f, 0.05f, 0.2f));

            CreateAnchor("Spawn_Nightclub", SceneAnchor.AnchorKind.PlayerSpawn, NightDistrictIdsSafe("nightclub"),
                "Nightclub Spawn", new Vector3(0f, 0f, 5f), NaadSceneNames.CityStreet);
            CreateAnchor("Exit_Nightclub_To_CityStreet", SceneAnchor.AnchorKind.ExitTravel, NightDistrictIdsSafe("nightclub-exit"),
                "Exit to City Street", new Vector3(0f, 0f, 7f), NaadSceneNames.CityStreet);
            CreateAnchor("Vendor_Emeka", SceneAnchor.AnchorKind.Interaction, "npc.crew.emeka",
                "Emeka", new Vector3(-6.5f, 0f, 1.2f), null);
            CreateAnchor("Interact_Bar", SceneAnchor.AnchorKind.Interaction, "ncl.bar.order",
                "Bar Counter", new Vector3(-6.5f, 0f, 0f), null);
            CreateAnchor("Interact_Stage", SceneAnchor.AnchorKind.Interaction, "ncl.stage.perform",
                "Stage", new Vector3(0f, 0f, -5f), null);

            CreateMainCamera(new Vector3(0f, 2.6f, 10f), Quaternion.Euler(10f, 180f, 0f));
            CreateDirectionalLight(new Vector3(50f, -30f, 0f), new Color(0.7f, 0.4f, 1f), 0.7f);
            EditorSceneManager.SaveScene(scene, NaadSceneNames.NightclubPath);
        }

        private static void BuildBeachScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("Beach");
            var content = new GameObject("Content");
            content.transform.SetParent(root.transform, false);

            // Sand ground
            CreateGround("SandGround", new Vector3(0f, 0f, 0f), new Vector3(30f, 0.5f, 20f), new Color(0.76f, 0.7f, 0.5f));

            // Water/ocean
            CreateBox("OceanWater", content.transform, new Vector3(0f, -0.5f, 12f), new Vector3(30f, 1f, 10f), new Color(0.1f, 0.4f, 0.7f));

            // Beach elements
            CreateBox("BeachBar", content.transform, new Vector3(-5f, 0.75f, -2f), new Vector3(4f, 1.5f, 2f), new Color(0.6f, 0.45f, 0.3f));
            CreateBox("PalmTree_A", content.transform, new Vector3(8f, 2.5f, -5f), new Vector3(0.5f, 5f, 0.5f), new Color(0.4f, 0.3f, 0.15f));
            CreateBox("PalmTree_B", content.transform, new Vector3(-8f, 2.5f, -4f), new Vector3(0.5f, 5f, 0.5f), new Color(0.4f, 0.3f, 0.15f));
            CreateBox("BeachChair_A", content.transform, new Vector3(2f, 0.3f, 3f), new Vector3(1.5f, 0.6f, 2f), new Color(0.8f, 0.2f, 0.2f));
            CreateBox("BeachChair_B", content.transform, new Vector3(4f, 0.3f, 3f), new Vector3(1.5f, 0.6f, 2f), new Color(0.2f, 0.6f, 0.2f));
            CreateBox("BonfirePit", content.transform, new Vector3(0f, 0.4f, 6f), new Vector3(2f, 0.8f, 2f), new Color(0.3f, 0.25f, 0.2f));

            // Lighting - warm beach night
            CreateDirectionalLight(new Vector3(60f, -20f, 0f), new Color(1f, 0.9f, 0.7f), 0.8f);

            // Bonfire light
            var bonfireLight = new GameObject("BonfireLight");
            var fireLight = bonfireLight.AddComponent<Light>();
            fireLight.type = LightType.Point;
            fireLight.color = new Color(1f, 0.5f, 0.1f);
            fireLight.intensity = 1.5f;
            fireLight.range = 8f;
            bonfireLight.transform.position = new Vector3(0f, 1f, 6f);

            CreateAnchor("Spawn_Beach", SceneAnchor.AnchorKind.PlayerSpawn, NightDistrictIdsSafe("beach"),
                "Beach Spawn", new Vector3(0f, 0f, -6f), NaadSceneNames.CityStreet);
            CreateAnchor("Exit_Beach_To_CityStreet", SceneAnchor.AnchorKind.ExitTravel, NightDistrictIdsSafe("beach-exit"),
                "Exit to City Street", new Vector3(0f, 0f, -8f), NaadSceneNames.CityStreet);
            CreateAnchor("Interact_BeachBar", SceneAnchor.AnchorKind.Interaction, "beach.bar.drinks",
                "Beach Bar", new Vector3(-5f, 0f, -2f), null);
            CreateAnchor("Interact_Bonfire", SceneAnchor.AnchorKind.Interaction, "beach.bonfire.gather",
                "Bonfire", new Vector3(0f, 0f, 6f), null);

            CreateMainCamera(new Vector3(0f, 3f, -10f), Quaternion.Euler(15f, 0f, 0f));
            EditorSceneManager.SaveScene(scene, NaadSceneNames.BeachPath);
        }

        private static void ValidateSceneFile(string path, List<string> errors)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null)
                errors.Add($"Missing scene asset: {path}");
        }

        private static void ValidateBuildEntry(int index, string path, List<string> errors)
        {
            var scenes = EditorBuildSettings.scenes;
            if (scenes == null || scenes.Length <= index || !scenes[index].enabled || scenes[index].path != path)
                errors.Add($"Build index {index} must be enabled and point to {path}");
        }

        private static void ValidateBuildContains(string path, List<string> errors)
        {
            if (EditorBuildSettings.scenes == null || EditorBuildSettings.scenes.All(s => s.path != path || !s.enabled))
                errors.Add($"Build Settings missing enabled scene: {path}");
        }

        private static void ValidateAnchors(string path, bool requireSpawn, bool requireExit, List<string> errors)
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                errors.Add($"Could not open {path}");
                return;
            }

            var anchors = UnityEngine.Object.FindObjectsByType<SceneAnchor>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (requireSpawn && anchors.All(a => a.Kind != SceneAnchor.AnchorKind.PlayerSpawn))
                errors.Add($"{path}: missing PlayerSpawn SceneAnchor");
            if (requireExit && anchors.All(a => a.Kind != SceneAnchor.AnchorKind.ExitTravel))
                errors.Add($"{path}: missing ExitTravel SceneAnchor");

            var cam = UnityEngine.Object.FindFirstObjectByType<Camera>();
            if (cam == null || !cam.CompareTag("MainCamera"))
                errors.Add($"{path}: missing Camera tagged MainCamera");
        }

        private static GameObject TryInstantiate(string path, Transform parent, Vector3 pos, string name)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogWarning($"[NAAD][Scenes] Prefab not found: {path}");
                return null;
            }

            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            go.name = name;
            go.transform.SetParent(parent, true);
            go.transform.position = pos;
            return go;
        }

        private static void CreateAnchor(string objectName, SceneAnchor.AnchorKind kind, string contentId,
            string displayName, Vector3 position, string travelScene)
        {
            var go = new GameObject(objectName);
            go.transform.position = position;
            var anchor = go.AddComponent<SceneAnchor>();
            anchor.Configure(contentId, kind, displayName, travelScene);

            var col = go.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = 0.75f;
        }

        private static void CreateMainCamera(Vector3 pos, Quaternion rot)
        {
            var go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            go.transform.SetPositionAndRotation(pos, rot);
            go.AddComponent<Camera>();
            go.AddComponent<AudioListener>();
        }

        private static void CreateDirectionalLight(Vector3 euler, Color color, float intensity)
        {
            var go = new GameObject("Directional Light");
            var light = go.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = color;
            light.intensity = intensity;
            light.shadows = LightShadows.Soft;
            go.transform.rotation = Quaternion.Euler(euler);
        }

        private static void CreateGround(string name, Vector3 pos, Vector3 scale, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.position = pos + Vector3.up * (-0.5f * scale.y);
            go.transform.localScale = scale;
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                mat.color = color;
                renderer.sharedMaterial = mat;
            }
        }

        private static void CreateBox(string name, Transform parent, Vector3 pos, Vector3 scale, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, true);
            go.transform.position = pos;
            go.transform.localScale = scale;
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
                mat.color = color;
                renderer.sharedMaterial = mat;
            }
        }

        private static string NightDistrictIdsSafe(string suffix) => "naad.scene." + suffix;
    }
}
