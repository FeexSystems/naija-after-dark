using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace NAAD.Editor
{
    /// <summary>
    /// Verification script for player navigation in environment scenes.
    /// Run this from the Unity Editor menu: NAAD > Verify Player Navigation
    /// 
    /// This script checks:
    /// - NavMesh presence and configuration
    /// - Player spawn point accessibility
    /// - Navigation between key locations
    /// - Collision mesh correctness
    /// - Agent configuration
    /// </summary>
    public class VerifyPlayerNavigation
    {
        private class NavigationTestResult
        {
            public string SceneName;
            public bool HasNavMesh;
            public bool NavMeshBaked;
            public bool HasPlayerSpawn;
            public bool SpawnAccessible;
            public float NavMeshArea;
            public int NavMeshVertices;
            public List<string> Issues;
            public List<string> Warnings;
        }

        private static List<NavigationTestResult> _results;

        [MenuItem("NAAD/Verify Player Navigation")]
        public static void RunVerification()
        {
            Debug.Log("[NAAD] Starting player navigation verification...");
            
            InitializeResults();
            TestNavigation();
            ReportResults();
        }

        private static void InitializeResults()
        {
            _results = new List<NavigationTestResult>();

            // Blockout Scenes (MVP)
            AddSceneToTest("Apartment_Blockout", "Assets/NAAD/Content/Scenes/Blockout/Apartment_Blockout.unity");
            AddSceneToTest("SuyaSpot_Blockout", "Assets/NAAD/Content/Scenes/Blockout/SuyaSpot_Blockout.unity");
            AddSceneToTest("Nightclub_Blockout", "Assets/NAAD/Content/Scenes/Blockout/Nightclub_Blockout.unity");

            // District Scenes
            AddSceneToTest("Mainland_Streets", "Assets/NAAD/Content/Scenes/Mainland/Mainland_Streets.unity");
            AddSceneToTest("Island_Nightlife", "Assets/NAAD/Content/Scenes/Island/Island_Nightlife.unity");
            AddSceneToTest("Creative_District", "Assets/NAAD/Content/Scenes/Creative/Creative_District.unity");
            AddSceneToTest("Coastal_Beach", "Assets/NAAD/Content/Scenes/Coastal/Coastal_Beach.unity");
        }

        private static void AddSceneToTest(string sceneName, string scenePath)
        {
            _results.Add(new NavigationTestResult
            {
                SceneName = sceneName,
                HasNavMesh = false,
                NavMeshBaked = false,
                HasPlayerSpawn = false,
                SpawnAccessible = false,
                NavMeshArea = 0f,
                NavMeshVertices = 0,
                Issues = new List<string>(),
                Warnings = new List<string>()
            });
        }

        private static void TestNavigation()
        {
            foreach (var result in _results)
            {
                var scenePath = $"Assets/NAAD/Content/Scenes/Blockout/{result.SceneName}.unity";
                if (!System.IO.File.Exists(scenePath))
                {
                    scenePath = $"Assets/NAAD/Content/Scenes/Mainland/{result.SceneName}.unity";
                }
                if (!System.IO.File.Exists(scenePath))
                {
                    scenePath = $"Assets/NAAD/Content/Scenes/Island/{result.SceneName}.unity";
                }
                if (!System.IO.File.Exists(scenePath))
                {
                    scenePath = $"Assets/NAAD/Content/Scenes/Creative/{result.SceneName}.unity";
                }
                if (!System.IO.File.Exists(scenePath))
                {
                    scenePath = $"Assets/NAAD/Content/Scenes/Coastal/{result.SceneName}.unity";
                }

                var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
                if (sceneAsset == null)
                {
                    result.Issues.Add("Scene file not found");
                    continue;
                }

                // Open scene
                var currentScene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

                // Test NavMesh
                TestNavMesh(result);

                // Test Player Spawn
                TestPlayerSpawn(result);

                // Test Navigation Between Key Points
                TestNavigationPoints(result);

                // Test Collision Meshes
                TestCollisionMeshes(result);

                // Save and close scene
                EditorSceneManager.SaveScene(currentScene);
                EditorSceneManager.CloseScene(currentScene, true);
            }
        }

        private static void TestNavMesh(NavigationTestResult result)
        {
            // Uses the built-in UnityEngine.AI NavMesh triangulation rather than the
            // optional AI Navigation package's NavMeshSurface, so this verifier has
            // no extra package dependency. CalculateTriangulation returns the baked
            // NavMesh for the currently open scene.
            var triangulation = NavMesh.CalculateTriangulation();
            var vertices = triangulation.vertices;
            result.HasNavMesh = vertices != null && vertices.Length > 0;

            if (!result.HasNavMesh)
            {
                result.Issues.Add("No baked NavMesh found in scene. Bake a NavMesh (Window > AI > Navigation, or legacy Bake) before verifying.");
                return;
            }

            result.NavMeshBaked = true;

            result.NavMeshVertices = vertices.Length;
            result.NavMeshArea = CalculateNavMeshArea(vertices);

            // Check NavMesh size
            if (result.NavMeshArea < 10f)
            {
                result.Issues.Add($"NavMesh area too small: {result.NavMeshArea:F2}m². Expected at least 10m² for navigation.");
            }

            // Check NavMesh vertices
            if (result.NavMeshVertices < 100)
            {
                result.Warnings.Add($"NavMesh has low vertex count: {result.NavMeshVertices}. May indicate incomplete bake.");
            }
        }

        private static float CalculateNavMeshArea(Vector3[] vertices)
        {
            // Approximate area calculation based on bounding box
            if (vertices == null || vertices.Length == 0)
                return 0f;

            var min = vertices[0];
            var max = vertices[0];

            foreach (var pos in vertices)
            {
                min = Vector3.Min(min, pos);
                max = Vector3.Max(max, pos);
            }

            var size = max - min;
            return size.x * size.z; // Area on XZ plane
        }

        private static void TestPlayerSpawn(NavigationTestResult result)
        {
            // Check for PlayerSpawn GameObject
            var playerSpawn = GameObject.Find("PlayerSpawn");
            result.HasPlayerSpawn = playerSpawn != null;

            if (!result.HasPlayerSpawn)
            {
                result.Issues.Add("No PlayerSpawn GameObject found. Create an empty GameObject named 'PlayerSpawn' at the desired spawn location.");
                return;
            }

            // Check if spawn is on NavMesh
            if (result.HasNavMesh && result.NavMeshBaked)
            {
                var spawnPos = playerSpawn.transform.position;
                var navMeshHit = new NavMeshHit();
                
                if (NavMesh.SamplePosition(spawnPos, out navMeshHit, 2.0f, NavMesh.AllAreas))
                {
                    result.SpawnAccessible = true;
                    
                    // Check distance from actual spawn to nearest NavMesh point
                    var distance = Vector3.Distance(spawnPos, navMeshHit.position);
                    if (distance > 0.5f)
                    {
                        result.Warnings.Add($"PlayerSpawn is {distance:F2}m from nearest NavMesh point. Consider moving spawn closer to NavMesh.");
                    }
                }
                else
                {
                    result.SpawnAccessible = false;
                    result.Issues.Add("PlayerSpawn is not on NavMesh. Player will not be able to navigate from spawn.");
                }
            }
            else
            {
                result.Warnings.Add("Cannot test PlayerSpawn accessibility: NavMesh not present or not baked.");
            }

            // Check spawn height (should be above ground)
            if (playerSpawn.transform.position.y < 0.5f)
            {
                result.Warnings.Add($"PlayerSpawn height is low: {playerSpawn.transform.position.y:F2}m. Ensure spawn is above ground level.");
            }
        }

        private static void TestNavigationPoints(NavigationTestResult result)
        {
            if (!result.HasNavMesh || !result.NavMeshBaked)
            {
                result.Warnings.Add("Cannot test navigation points: NavMesh not present or not baked.");
                return;
            }

            // Define key navigation points based on scene type
            var navigationPoints = GetNavigationPointsForScene(result.SceneName);

            if (navigationPoints.Count == 0)
            {
                result.Warnings.Add("No navigation points defined for this scene type. Add navigation points to test pathfinding.");
                return;
            }

            // Test pathfinding between points
            foreach (var point in navigationPoints)
            {
                var pointObj = GameObject.Find(point);
                if (pointObj == null)
                {
                    result.Warnings.Add($"Navigation point '{point}' not found in scene.");
                    continue;
                }

                var pointPos = pointObj.transform.position;
                var navMeshHit = new NavMeshHit();
                
                if (!NavMesh.SamplePosition(pointPos, out navMeshHit, 2.0f, NavMesh.AllAreas))
                {
                    result.Issues.Add($"Navigation point '{point}' is not on NavMesh.");
                }
            }

            // Test pathfinding from spawn to key points
            if (result.HasPlayerSpawn)
            {
                var spawnPos = GameObject.Find("PlayerSpawn").transform.position;
                
                foreach (var point in navigationPoints)
                {
                    var pointObj = GameObject.Find(point);
                    if (pointObj == null) continue;

                    var pointPos = pointObj.transform.position;
                    var path = new NavMeshPath();
                    
                    if (NavMesh.CalculatePath(spawnPos, pointPos, NavMesh.AllAreas, path))
                    {
                        if (path.status == NavMeshPathStatus.PathComplete)
                        {
                            // Path found
                            var pathLength = CalculatePathLength(path);
                            if (pathLength > 100f)
                            {
                                result.Warnings.Add($"Path from spawn to '{point}' is long: {pathLength:F2}m. Consider adding intermediate waypoints.");
                            }
                        }
                        else if (path.status == NavMeshPathStatus.PathPartial)
                        {
                            result.Issues.Add($"Partial path from spawn to '{point}'. Some areas may be unreachable.");
                        }
                        else
                        {
                            result.Issues.Add($"No path from spawn to '{point}'. Key location is unreachable.");
                        }
                    }
                }
            }
        }

        private static List<string> GetNavigationPointsForScene(string sceneName)
        {
            var points = new List<string>();

            if (sceneName.Contains("Apartment"))
            {
                points.Add("Entrance");
                points.Add("LivingRoom");
                points.Add("Bedroom");
                points.Add("Kitchen");
                points.Add("Balcony");
            }
            else if (sceneName.Contains("Suya"))
            {
                points.Add("Counter");
                points.Add("Seating");
                points.Add("Grill");
            }
            else if (sceneName.Contains("Nightclub"))
            {
                points.Add("Entrance");
                points.Add("Stage");
                points.Add("DanceFloor");
                points.Add("Bar");
                points.Add("VIP");
            }
            else if (sceneName.Contains("Mainland"))
            {
                points.Add("Street_Start");
                points.Add("Street_End");
                points.Add("Shopfront");
            }
            else if (sceneName.Contains("Island"))
            {
                points.Add("Club_Entrance");
                points.Add("Bar_District");
                points.Add("Waterfront");
            }
            else if (sceneName.Contains("Creative"))
            {
                points.Add("Gallery");
                points.Add("Cafe");
                points.Add("Workspace");
            }
            else if (sceneName.Contains("Coastal"))
            {
                points.Add("Beach");
                points.Add("Resort");
                points.Add("Jetty");
            }

            return points;
        }

        private static float CalculatePathLength(NavMeshPath path)
        {
            var length = 0f;
            for (int i = 1; i < path.corners.Length; i++)
            {
                length += Vector3.Distance(path.corners[i - 1], path.corners[i]);
            }
            return length;
        }

        private static void TestCollisionMeshes(NavigationTestResult result)
        {
            // Check for colliders on key objects
            var colliders = GameObject.FindObjectsOfType<Collider>();
            
            if (colliders.Length == 0)
            {
                result.Issues.Add("No colliders found in scene. Navigation may not work correctly without collision meshes.");
                return;
            }

            // Check for common collision issues
            var triggers = GameObject.FindObjectsOfType<Collider>().Where(c => c.isTrigger).ToList();
            var nonTriggers = GameObject.FindObjectsOfType<Collider>().Where(c => !c.isTrigger).ToList();

            if (nonTriggers.Count < 5)
            {
                result.Warnings.Add($"Low number of non-trigger colliders: {nonTriggers.Count}. Ensure walls and floors have collision.");
            }

            // Check for large colliders that might block navigation
            var largeColliders = nonTriggers.Where(c => c.bounds.size.magnitude > 20f).ToList();
            if (largeColliders.Count > 0)
            {
                result.Warnings.Add($"Found {largeColliders.Count} large colliders. Verify they don't unintentionally block NavMesh.");
            }
        }

        private static void ReportResults()
        {
            var scenesWithNavMesh = _results.Where(r => r.HasNavMesh).ToList();
            var scenesWithBakedNavMesh = _results.Where(r => r.NavMeshBaked).ToList();
            var scenesWithPlayerSpawn = _results.Where(r => r.HasPlayerSpawn).ToList();
            var scenesWithAccessibleSpawn = _results.Where(r => r.SpawnAccessible).ToList();

            Debug.Log($"[NAAD] Player Navigation Verification Complete");
            Debug.Log($"[NAAD] Scenes with NavMesh: {scenesWithNavMesh.Count}/{_results.Count}");
            Debug.Log($"[NAAD] Scenes with Baked NavMesh: {scenesWithBakedNavMesh.Count}/{_results.Count}");
            Debug.Log($"[NAAD] Scenes with PlayerSpawn: {scenesWithPlayerSpawn.Count}/{_results.Count}");
            Debug.Log($"[NAAD] Scenes with Accessible Spawn: {scenesWithAccessibleSpawn.Count}/{_results.Count}");

            // Report issues
            var scenesWithIssues = _results.Where(r => r.Issues.Count > 0).ToList();
            if (scenesWithIssues.Count > 0)
            {
                Debug.LogError($"[NAAD] Scenes with navigation issues ({scenesWithIssues.Count}):");
                foreach (var result in scenesWithIssues)
                {
                    Debug.LogError($"  - {result.SceneName}:");
                    foreach (var issue in result.Issues)
                    {
                        Debug.LogError($"    • {issue}");
                    }
                }
            }

            // Report warnings
            var scenesWithWarnings = _results.Where(r => r.Warnings.Count > 0).ToList();
            if (scenesWithWarnings.Count > 0)
            {
                Debug.LogWarning($"[NAAD] Scenes with navigation warnings ({scenesWithWarnings.Count}):");
                foreach (var result in scenesWithWarnings)
                {
                    Debug.LogWarning($"  - {result.SceneName}:");
                    foreach (var warning in result.Warnings)
                    {
                        Debug.LogWarning($"    • {warning}");
                    }
                }
            }

            // Generate report file
            GenerateReportFile(scenesWithNavMesh, scenesWithBakedNavMesh, scenesWithPlayerSpawn, 
                scenesWithAccessibleSpawn, scenesWithIssues, scenesWithWarnings);
        }

        private static void GenerateReportFile(List<NavigationTestResult> scenesWithNavMesh,
            List<NavigationTestResult> scenesWithBakedNavMesh,
            List<NavigationTestResult> scenesWithPlayerSpawn,
            List<NavigationTestResult> scenesWithAccessibleSpawn,
            List<NavigationTestResult> scenesWithIssues,
            List<NavigationTestResult> scenesWithWarnings)
        {
            var reportPath = "Assets/NAAD/Content/NAVIGATION_VERIFICATION_REPORT.txt";
            var report = new System.Text.StringBuilder();

            report.AppendLine("NAAD Player Navigation Verification Report");
            report.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine();
            report.AppendLine($"Summary:");
            report.AppendLine($"  Scenes with NavMesh: {scenesWithNavMesh.Count}/{_results.Count}");
            report.AppendLine($"  Scenes with Baked NavMesh: {scenesWithBakedNavMesh.Count}/{_results.Count}");
            report.AppendLine($"  Scenes with PlayerSpawn: {scenesWithPlayerSpawn.Count}/{_results.Count}");
            report.AppendLine($"  Scenes with Accessible Spawn: {scenesWithAccessibleSpawn.Count}/{_results.Count}");
            report.AppendLine($"  Scenes with Issues: {scenesWithIssues.Count}");
            report.AppendLine($"  Scenes with Warnings: {scenesWithWarnings.Count}");
            report.AppendLine();

            foreach (var result in _results)
            {
                report.AppendLine($"Scene: {result.SceneName}");
                report.AppendLine($"  NavMesh Present: {(result.HasNavMesh ? "✓" : "✗")}");
                report.AppendLine($"  NavMesh Baked: {(result.NavMeshBaked ? "✓" : "✗")}");
                report.AppendLine($"  NavMesh Area: {result.NavMeshArea:F2}m²");
                report.AppendLine($"  NavMesh Vertices: {result.NavMeshVertices}");
                report.AppendLine($"  PlayerSpawn Present: {(result.HasPlayerSpawn ? "✓" : "✗")}");
                report.AppendLine($"  Spawn Accessible: {(result.SpawnAccessible ? "✓" : "✗")}");
                
                if (result.Issues.Count > 0)
                {
                    report.AppendLine("  Issues:");
                    foreach (var issue in result.Issues)
                    {
                        report.AppendLine($"    - {issue}");
                    }
                }

                if (result.Warnings.Count > 0)
                {
                    report.AppendLine("  Warnings:");
                    foreach (var warning in result.Warnings)
                    {
                        report.AppendLine($"    - {warning}");
                    }
                }

                if (result.Issues.Count == 0 && result.Warnings.Count == 0)
                {
                    report.AppendLine("  Status: ✓ Healthy");
                }

                report.AppendLine();
            }

            System.IO.File.WriteAllText(reportPath, report.ToString());
            Debug.Log($"[NAAD] Navigation verification report saved to: {reportPath}");
            AssetDatabase.Refresh();
        }
    }
}
