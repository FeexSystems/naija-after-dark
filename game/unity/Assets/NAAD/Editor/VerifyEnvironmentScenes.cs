using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NAAD.Editor
{
    /// <summary>
    /// Verification script for environment scene loading.
    /// Run this from the Unity Editor menu: NAAD > Verify Environment Scenes
    /// </summary>
    public class VerifyEnvironmentScenes
    {
        private struct SceneRequirement
        {
            public string SceneName;
            public string ScenePath;
            public bool IsRequired;
            public bool IsFound;
            public bool HasNavMesh;
            public bool HasLighting;
            public int GameObjectCount;
            public List<string> Issues;
        }

        private static List<SceneRequirement> _requirements;

        [MenuItem("NAAD/Verify Environment Scenes")]
        public static void RunVerification()
        {
            Debug.Log("[NAAD] Starting environment scene verification...");
            
            InitializeRequirements();
            CheckScenes();
            ReportResults();
        }

        private static void InitializeRequirements()
        {
            _requirements = new List<SceneRequirement>();

            // Blockout Scenes (MVP)
            AddSceneRequirement("Apartment_Blockout", "Assets/NAAD/Content/Scenes/Blockout/Apartment_Blockout.unity", true);
            AddSceneRequirement("SuyaSpot_Blockout", "Assets/NAAD/Content/Scenes/Blockout/SuyaSpot_Blockout.unity", true);
            AddSceneRequirement("Nightclub_Blockout", "Assets/NAAD/Content/Scenes/Blockout/Nightclub_Blockout.unity", true);

            // District Scenes
            AddSceneRequirement("Mainland_Streets", "Assets/NAAD/Content/Scenes/Mainland/Mainland_Streets.unity", false);
            AddSceneRequirement("Island_Nightlife", "Assets/NAAD/Content/Scenes/Island/Island_Nightlife.unity", false);
            AddSceneRequirement("Creative_District", "Assets/NAAD/Content/Scenes/Creative/Creative_District.unity", false);
            AddSceneRequirement("Coastal_Beach", "Assets/NAAD/Content/Scenes/Coastal/Coastal_Beach.unity", false);
        }

        private static void AddSceneRequirement(string sceneName, string scenePath, bool isRequired)
        {
            _requirements.Add(new SceneRequirement
            {
                SceneName = sceneName,
                ScenePath = scenePath,
                IsRequired = isRequired,
                IsFound = false,
                HasNavMesh = false,
                HasLighting = false,
                GameObjectCount = 0,
                Issues = new List<string>()
            });
        }

        private static void CheckScenes()
        {
            foreach (var req in _requirements)
            {
                // Check if scene exists
                var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(req.ScenePath);
                req.IsFound = sceneAsset != null;

                if (req.IsFound)
                {
                    // Open scene and check contents
                    var currentScene = EditorSceneManager.OpenScene(req.ScenePath, OpenSceneMode.Single);
                    
                    // Count game objects
                    req.GameObjectCount = GameObject.FindObjectsOfType<GameObject>().Length;

                    // Check for NavMesh
                    var navMeshSurface = GameObject.FindObjectOfType<UnityEngine.AI.NavMeshSurface>();
                    req.HasNavMesh = navMeshSurface != null;
                    if (!req.HasNavMesh)
                    {
                        req.Issues.Add("No NavMeshSurface found in scene");
                    }

                    // Check for lighting
                    var lights = GameObject.FindObjectsOfType<Light>();
                    req.HasLighting = lights.Length > 0;
                    if (!req.HasLighting)
                    {
                        req.Issues.Add("No Light components found in scene");
                    }

                    // Check for essential components in blockout scenes
                    if (req.SceneName.Contains("Blockout"))
                    {
                        CheckBlockoutRequirements(req);
                    }

                    // Save and close scene
                    EditorSceneManager.SaveScene(currentScene);
                    EditorSceneManager.CloseScene(currentScene, true);
                }
                else
                {
                    req.Issues.Add("Scene file not found");
                }
            }
        }

        private static void CheckBlockoutRequirements(SceneRequirement req)
        {
            // Check for player spawn point
            var playerSpawn = GameObject.Find("PlayerSpawn");
            if (playerSpawn == null)
            {
                req.Issues.Add("No PlayerSpawn GameObject found");
            }

            // Check for scene bounds or volume
            var sceneBounds = GameObject.Find("SceneBounds");
            if (sceneBounds == null)
            {
                req.Issues.Add("No SceneBounds GameObject found (recommended for performance)");
            }

            // Check for audio listener
            var audioListener = GameObject.FindObjectOfType<AudioListener>();
            if (audioListener == null)
            {
                req.Issues.Add("No AudioListener found in scene");
            }

            // Check specific blockout elements
            switch (req.SceneName)
            {
                case "Apartment_Blockout":
                    CheckApartmentBlockout(req);
                    break;
                case "SuyaSpot_Blockout":
                    CheckSuyaSpotBlockout(req);
                    break;
                case "Nightclub_Blockout":
                    CheckNightclubBlockout(req);
                    break;
            }
        }

        private static void CheckApartmentBlockout(SceneRequirement req)
        {
            // Check for building structure
            var building = GameObject.Find("Apartment_Building");
            if (building == null)
            {
                req.Issues.Add("No Apartment_Building GameObject found");
            }

            // Check for interior
            var interior = GameObject.Find("Interior");
            if (interior == null)
            {
                req.Issues.Add("No Interior GameObject found");
            }
        }

        private static void CheckSuyaSpotBlockout(SceneRequirement req)
        {
            // Check for stand structure
            var stand = GameObject.Find("Suya_Stand");
            if (stand == null)
            {
                req.Issues.Add("No Suya_Stand GameObject found");
            }

            // Check for grill
            var grill = GameObject.Find("Grill");
            if (grill == null)
            {
                req.Issues.Add("No Grill GameObject found");
            }

            // Check for seating
            var seating = GameObject.Find("Seating");
            if (seating == null)
            {
                req.Issues.Add("No Seating GameObject found");
            }
        }

        private static void CheckNightclubBlockout(SceneRequirement req)
        {
            // Check for entrance
            var entrance = GameObject.Find("Entrance");
            if (entrance == null)
            {
                req.Issues.Add("No Entrance GameObject found");
            }

            // Check for stage
            var stage = GameObject.Find("Stage");
            if (stage == null)
            {
                req.Issues.Add("No Stage GameObject found");
            }

            // Check for dance floor
            var danceFloor = GameObject.Find("DanceFloor");
            if (danceFloor == null)
            {
                req.Issues.Add("No DanceFloor GameObject found");
            }

            // Check for bar
            var bar = GameObject.Find("Bar");
            if (bar == null)
            {
                req.Issues.Add("No Bar GameObject found");
            }
        }

        private static void ReportResults()
        {
            var missingRequired = _requirements.Where(r => r.IsRequired && !r.IsFound).ToList();
            var missingOptional = _requirements.Where(r => !r.IsRequired && !r.IsFound).ToList();
            var foundCount = _requirements.Count(r => r.IsFound);
            var totalCount = _requirements.Count;

            Debug.Log($"[NAAD] Environment Scene Verification Complete");
            Debug.Log($"[NAAD] Found: {foundCount}/{totalCount} scenes");

            if (missingRequired.Count > 0)
            {
                Debug.LogError($"[NAAD] MISSING REQUIRED SCENES ({missingRequired.Count}):");
                foreach (var req in missingRequired)
                {
                    Debug.LogError($"  - {req.SceneName}: {req.ScenePath}");
                }
            }
            else
            {
                Debug.Log("[NAAD] ✓ All required scenes are present");
            }

            if (missingOptional.Count > 0)
            {
                Debug.LogWarning($"[NAAD] Missing optional scenes ({missingOptional.Count}):");
                foreach (var req in missingOptional)
                {
                    Debug.LogWarning($"  - {req.SceneName}: {req.ScenePath}");
                }
            }

            // Report issues in found scenes
            var scenesWithIssues = _requirements.Where(r => r.IsFound && r.Issues.Count > 0).ToList();
            if (scenesWithIssues.Count > 0)
            {
                Debug.LogWarning($"[NAAD] Scenes with issues ({scenesWithIssues.Count}):");
                foreach (var req in scenesWithIssues)
                {
                    Debug.LogWarning($"  - {req.SceneName}:");
                    foreach (var issue in req.Issues)
                    {
                        Debug.LogWarning($"    • {issue}");
                    }
                }
            }

            // Generate report file
            GenerateReportFile(missingRequired, missingOptional, scenesWithIssues, foundCount, totalCount);
        }

        private static void GenerateReportFile(List<SceneRequirement> missingRequired,
            List<SceneRequirement> missingOptional,
            List<SceneRequirement> scenesWithIssues,
            int foundCount, int totalCount)
        {
            var reportPath = "Assets/NAAD/Content/SCENE_VERIFICATION_REPORT.txt";
            var report = new System.Text.StringBuilder();

            report.AppendLine("NAAD Environment Scene Verification Report");
            report.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine();
            report.AppendLine($"Summary: {foundCount}/{totalCount} scenes found");
            report.AppendLine($"Missing Required: {missingRequired.Count}");
            report.AppendLine($"Missing Optional: {missingOptional.Count}");
            report.AppendLine($"Scenes with Issues: {scenesWithIssues.Count}");
            report.AppendLine();

            if (missingRequired.Count > 0)
            {
                report.AppendLine("MISSING REQUIRED SCENES:");
                foreach (var req in missingRequired)
                {
                    report.AppendLine($"  [X] {req.SceneName}");
                    report.AppendLine($"      Path: {req.ScenePath}");
                    report.AppendLine($"      Action: Create scene following blockout documentation");
                    report.AppendLine();
                }
            }

            if (missingOptional.Count > 0)
            {
                report.AppendLine("MISSING OPTIONAL SCENES:");
                foreach (var req in missingOptional)
                {
                    report.AppendLine($"  [ ] {req.SceneName}");
                    report.AppendLine($"      Path: {req.ScenePath}");
                    report.AppendLine();
                }
            }

            if (scenesWithIssues.Count > 0)
            {
                report.AppendLine("SCENES WITH ISSUES:");
                foreach (var req in scenesWithIssues)
                {
                    report.AppendLine($"  [!] {req.SceneName}");
                    report.AppendLine($"      GameObjects: {req.GameObjectCount}");
                    report.AppendLine($"      NavMesh: {(req.HasNavMesh ? "✓" : "✗")}");
                    report.AppendLine($"      Lighting: {(req.HasLighting ? "✓" : "✗")}");
                    report.AppendLine("      Issues:");
                    foreach (var issue in req.Issues)
                    {
                        report.AppendLine($"        - {issue}");
                    }
                    report.AppendLine();
                }
            }

            // Report healthy scenes
            var healthyScenes = _requirements.Where(r => r.IsFound && r.Issues.Count == 0).ToList();
            if (healthyScenes.Count > 0)
            {
                report.AppendLine("HEALTHY SCENES:");
                foreach (var req in healthyScenes)
                {
                    report.AppendLine($"  [✓] {req.SceneName}");
                    report.AppendLine($"      GameObjects: {req.GameObjectCount}");
                    report.AppendLine($"      NavMesh: {(req.HasNavMesh ? "✓" : "✗")}");
                    report.AppendLine($"      Lighting: {(req.HasLighting ? "✓" : "✗")}");
                    report.AppendLine();
                }
            }

            if (missingRequired.Count == 0 && missingOptional.Count == 0 && scenesWithIssues.Count == 0)
            {
                report.AppendLine("✓ All environment scenes are present and properly configured.");
            }

            System.IO.File.WriteAllText(reportPath, report.ToString());
            Debug.Log($"[NAAD] Scene verification report saved to: {reportPath}");
            AssetDatabase.Refresh();
        }
    }
}
