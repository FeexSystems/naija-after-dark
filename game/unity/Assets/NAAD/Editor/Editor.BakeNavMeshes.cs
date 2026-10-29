#if NAAD_LEGACY_EDITOR_TOOLS
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AI;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace NAAD.Editor
{
    /// <summary>
    /// Bakes NavMeshes for all registered scenes in the build settings.
    /// Run from Unity Editor menu: NAAD > Scenes > Bake All NavMeshes
    /// </summary>
    public static class BakeNavMeshes
    {
        private const float AgentRadius = 0.5f;
        private const float AgentHeight = 2.0f;
        private const float AgentMaxSlope = 45f;
        private const float AgentStepHeight = 0.3f;
        private const float MinRegionArea = 2f;
        private const int VoxelSize = 128;
        private const int TileSize = 256;

        [MenuItem("NAAD/Scenes/Bake All NavMeshes")]
        public static void BakeAllNavMeshes()
        {
            Debug.Log("[NAAD][NavMesh] Starting NavMesh bake for all build scenes...");

            var scenes = GetEnabledScenePaths();
            if (scenes.Length == 0)
            {
                Debug.LogWarning("[NAAD][NavMesh] No enabled scenes in Build Settings.");
                return;
            }

            var originalScene = EditorSceneManager.GetActiveScene();
            var results = new List<NavMeshBakeResult>();

            foreach (var scenePath in scenes)
            {
                var result = BakeSceneNavMesh(scenePath);
                results.Add(result);
            }

            // Restore original scene
            if (originalScene.IsValid() && !originalScene.isDirty)
            {
                EditorSceneManager.OpenScene(originalScene.path);
            }

            // Report results
            ReportResults(results);
        }

        private static string[] GetEnabledScenePaths()
        {
            var scenes = EditorBuildSettings.scenes;
            var paths = new List<string>(scenes.Length);
            foreach (var scene in scenes)
            {
                if (scene.enabled)
                    paths.Add(scene.path);
            }
            return paths.ToArray();
        }

        private static NavMeshBakeResult BakeSceneNavMesh(string scenePath)
        {
            var result = new NavMeshBakeResult { ScenePath = scenePath };

            Debug.Log($"[NAAD][NavMesh] Processing: {scenePath}");

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            if (!scene.IsValid())
            {
                result.Success = false;
                result.Error = "Failed to open scene";
                return result;
            }

            try
            {
                // Mark walkable objects
                MarkWalkableObjects();

                // Bake NavMesh
                NavMeshBuilder.ClearAllNavMeshes();
                var bounds = CalculateWorldBounds();
                if (bounds.size == Vector3.zero)
                {
                    result.Success = false;
                    result.Error = "No walkable geometry found for bounds calculation";
                    return result;
                }

                var buildSettings = new NavMeshBuildSettings
                {
                    agentRadius = AgentRadius,
                    agentHeight = AgentHeight,
                    agentSlope = AgentMaxSlope,
                    agentClimb = AgentStepHeight,
                    minRegionArea = MinRegionArea,
                    voxelSize = VoxelSize,
                    tileSize = TileSize,
                    accuratePlacement = true,
                    debug = false
                };

                var sources = NavMeshBuilder.CollectSources(bounds, NavMeshCollectGeometry.PhysicsColliders, 0, ~0, NavMeshCollectGeometry.Default);
                NavMeshBuilder.BuildNavMesh(buildSettings, sources, bounds, Vector3.zero, Quaternion.identity);

                // Save scene
                EditorSceneManager.SaveScene(scene);
                AssetDatabase.SaveAssets();

                // Verify bake
                var triangulation = NavMesh.CalculateTriangulation();
                if (triangulation.vertices.Length > 0)
                {
                    result.Success = true;
                    result.NavMeshArea = CalculateNavMeshArea(triangulation);
                    result.NavMeshTiles = triangulation.indices.Length / 3;
                    Debug.Log($"[NAAD][NavMesh] ✓ Baked: {scenePath} (Area: {result.NavMeshArea:F1}m², Tiles: {result.NavMeshTiles})");
                }
                else
                {
                    result.Success = false;
                    result.Error = "NavMesh bake produced no data";
                    Debug.LogWarning($"[NAAD][NavMesh] ✗ No NavMesh data generated for: {scenePath}");
                }
            }
            catch (System.Exception ex)
            {
                result.Success = false;
                result.Error = ex.Message;
                Debug.LogError($"[NAAD][NavMesh] Error baking {scenePath}: {ex.Message}");
            }

            return result;
        }

        private static void MarkWalkableObjects()
        {
            var renderers = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            int marked = 0;
            foreach (var renderer in renderers)
            {
                if (renderer.gameObject.isStatic)
                {
                    GameObjectUtility.SetStaticEditorFlags(renderer.gameObject, StaticEditorFlags.NavigationStatic);
                    marked++;
                }
            }
            Debug.Log($"[NAAD][NavMesh] Marked {marked} objects as NavigationStatic");
        }

        private static Bounds CalculateWorldBounds()
        {
            var colliders = Object.FindObjectsByType<Collider>(FindObjectsSortMode.None);
            var bounds = new Bounds(Vector3.zero, Vector3.zero);
            bool first = true;

            foreach (var col in colliders)
            {
                if (!col.enabled || col.isTrigger) continue;
                if (first)
                {
                    bounds = col.bounds;
                    first = false;
                }
                else
                {
                    bounds.Encapsulate(col.bounds);
                }
            }

            // Expand bounds slightly for agent radius
            bounds.Expand(new Vector3(AgentRadius * 2, AgentHeight, AgentRadius * 2));
            return bounds;
        }

        private static float CalculateNavMeshArea(NavMeshTriangulation triangulation)
        {
            if (triangulation.vertices.Length == 0) return 0f;

            float area = 0f;
            var indices = triangulation.indices;
            var vertices = triangulation.vertices;

            for (int i = 0; i < indices.Length; i += 3)
            {
                var v0 = vertices[indices[i]];
                var v1 = vertices[indices[i + 1]];
                var v2 = vertices[indices[i + 2]];
                area += Vector3.Cross(v1 - v0, v2 - v0).magnitude * 0.5f;
            }

            return area;
        }

        private static void ReportResults(List<NavMeshBakeResult> results)
        {
            int success = 0, failed = 0;
            float totalArea = 0f;
            int totalTiles = 0;

            var report = new System.Text.StringBuilder();
            report.AppendLine("NAAD NavMesh Bake Report");
            report.AppendLine($"Generated: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine();

            foreach (var r in results)
            {
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(r.ScenePath);
                if (r.Success)
                {
                    success++;
                    totalArea += r.NavMeshArea;
                    totalTiles += r.NavMeshTiles;
                    report.AppendLine($"✓ {sceneName}: Area={r.NavMeshArea:F1}m², Tiles={r.NavMeshTiles}");
                }
                else
                {
                    failed++;
                    report.AppendLine($"✗ {sceneName}: {r.Error}");
                }
            }

            report.AppendLine();
            report.AppendLine($"Summary: {success} succeeded, {failed} failed");
            report.AppendLine($"Total NavMesh Area: {totalArea:F1}m²");
            report.AppendLine($"Total NavMesh Tiles: {totalTiles}");

            string reportPath = "Assets/NAAD/Content/NAVMESH_BAKE_REPORT.txt";
            System.IO.File.WriteAllText(reportPath, report.ToString());
            AssetDatabase.Refresh();

            Debug.Log($"[NAAD][NavMesh] Bake complete. Report saved to: {reportPath}");
            Debug.Log($"[NAAD][NavMesh] Summary: {success} succeeded, {failed} failed, Total Area: {totalArea:F1}m²");
        }

        private class NavMeshBakeResult
        {
            public string ScenePath;
            public bool Success;
            public string Error;
            public float NavMeshArea;
            public int NavMeshTiles;
        }
    }
}
#endif
