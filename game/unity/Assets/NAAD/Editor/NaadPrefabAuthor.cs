using System;
using NAAD.World;
using UnityEditor;
using UnityEngine;

namespace NAAD.Editor
{
    /// <summary>
    /// Gate B Phase 1 — authors the six NAAD-owned Environment placeholder prefabs
    /// (ENVIRONMENT_MANIFEST road/sidewalk/suya/nightclub Phase-1 blockers) from
    /// built-in primitives with URP-safe materials.
    /// Batchmode: -executeMethod NAAD.Editor.NaadPrefabAuthor.AuthorPlaceholders
    /// </summary>
    public static class NaadPrefabAuthor
    {
        public const string EnvDir = "Assets/NAAD/Prefabs/Environment";

        [MenuItem("NAAD/Prefabs/Author Phase-1 Placeholders")]
        public static void AuthorPlaceholdersMenu() => AuthorPlaceholders();

        public static void AuthorPlaceholders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/NAAD/Prefabs"))
                AssetDatabase.CreateFolder("Assets/NAAD", "Prefabs");
            if (!AssetDatabase.IsValidFolder(EnvDir))
                AssetDatabase.CreateFolder("Assets/NAAD/Prefabs", "Environment");

            AuthorRoads();
            AuthorSuya();
            AuthorNightclub();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[NAAD][Prefabs] Phase-1 placeholders authored (6/6).");
        }

        private static void AuthorRoads()
        {
            BuildPrefab("RdSurfaceAsphalt", "RD_Surface_Asphalt",
                "Standard asphalt road surface tile (8m x 0.2m x 4m).",
                root => AddBox(root, "Surface", new Vector3(0f, -0.1f, 0f),
                    new Vector3(8f, 0.2f, 4f), new Color(0.12f, 0.12f, 0.13f)));
            BuildPrefab("StrSidewalkStraight", "STR_Sidewalk_Straight",
                "Straight sidewalk module (4m x 0.15m x 2m).",
                root => AddBox(root, "Walk", new Vector3(0f, 0.075f, 0f),
                    new Vector3(4f, 0.15f, 2f), new Color(0.42f, 0.40f, 0.38f)));
        }

        private static void AuthorSuya()
        {
            // BLOCKOUT_SUYA_SPOT: 4m x 6m footprint; grill 1.0x0.5 @0.9m.
            BuildPrefab("SyuStandStructure", "SYU_Stand_Structure",
                "Suya stand frame: 4 posts + canopy (4m x 2.8m x 4m).",
                root =>
                {
                    var post = new Color(0.25f, 0.25f, 0.27f);
                    AddBox(root, "Post_FL", new Vector3(-1.9f, 1.4f, -1.9f),
                        new Vector3(0.12f, 2.8f, 0.12f), post);
                    AddBox(root, "Post_FR", new Vector3(1.9f, 1.4f, -1.9f),
                        new Vector3(0.12f, 2.8f, 0.12f), post);
                    AddBox(root, "Post_BL", new Vector3(-1.9f, 1.4f, 1.9f),
                        new Vector3(0.12f, 2.8f, 0.12f), post);
                    AddBox(root, "Post_BR", new Vector3(1.9f, 1.4f, 1.9f),
                        new Vector3(0.12f, 2.8f, 0.12f), post);
                    AddBox(root, "Canopy", new Vector3(0f, 2.9f, 0f),
                        new Vector3(4.2f, 0.12f, 4.2f), new Color(0.85f, 0.45f, 0.12f));
                    AddBox(root, "Counter", new Vector3(0f, 0.45f, -1.2f),
                        new Vector3(3.0f, 0.9f, 0.6f), new Color(0.45f, 0.32f, 0.20f));
                });
            BuildPrefab("SyuGrillOpen", "SYU_Grill_Open",
                "Open charcoal grill (1.0m x 0.5m @ 0.9m height).",
                root =>
                {
                    AddBox(root, "Body", new Vector3(0f, 0.65f, 0f),
                        new Vector3(1.0f, 0.5f, 0.5f), new Color(0.18f, 0.18f, 0.19f));
                    AddBox(root, "Coals", new Vector3(0f, 0.88f, 0f),
                        new Vector3(0.9f, 0.08f, 0.4f), new Color(0.9f, 0.35f, 0.08f),
                        emissive: true);
                    var leg = new Color(0.15f, 0.15f, 0.16f);
                    AddBox(root, "Leg_FL", new Vector3(-0.45f, 0.2f, -0.2f),
                        new Vector3(0.06f, 0.4f, 0.06f), leg);
                    AddBox(root, "Leg_FR", new Vector3(0.45f, 0.2f, -0.2f),
                        new Vector3(0.06f, 0.4f, 0.06f), leg);
                    AddBox(root, "Leg_BL", new Vector3(-0.45f, 0.2f, 0.2f),
                        new Vector3(0.06f, 0.4f, 0.06f), leg);
                    AddBox(root, "Leg_BR", new Vector3(0.45f, 0.2f, 0.2f),
                        new Vector3(0.06f, 0.4f, 0.06f), leg);
                });
        }

        private static void AuthorNightclub()
        {
            // BLOCKOUT_NIGHTCLUB: double doors 3m x 2.5m; stage 8x4 @0.6m.
            BuildPrefab("NclEntranceDoorLarge", "NCL_Entrance_Door_Large",
                "Double glass entrance doors (3m wide x 2.5m high).",
                root =>
                {
                    var frame = new Color(0.20f, 0.20f, 0.22f);
                    var glass = new Color(0.35f, 0.55f, 0.75f);
                    AddBox(root, "Frame_L", new Vector3(-1.6f, 1.25f, 0f),
                        new Vector3(0.2f, 2.5f, 0.3f), frame);
                    AddBox(root, "Frame_R", new Vector3(1.6f, 1.25f, 0f),
                        new Vector3(0.2f, 2.5f, 0.3f), frame);
                    AddBox(root, "Frame_Top", new Vector3(0f, 2.55f, 0f),
                        new Vector3(3.4f, 0.2f, 0.3f), frame);
                    AddBox(root, "Door_L", new Vector3(-0.75f, 1.25f, 0f),
                        new Vector3(1.4f, 2.4f, 0.08f), glass, glassy: true);
                    AddBox(root, "Door_R", new Vector3(0.75f, 1.25f, 0f),
                        new Vector3(1.4f, 2.4f, 0.08f), glass, glassy: true);
                });
            BuildPrefab("NclStagePlatform", "NCL_Stage_Platform",
                "Raised stage floor (8m x 4m @ 0.6m height).",
                root => AddBox(root, "Platform", new Vector3(0f, 0.3f, 0f),
                    new Vector3(8f, 0.6f, 4f), new Color(0.30f, 0.22f, 0.16f)));
        }

        private static void BuildPrefab(string fileName, string assetId,
            string description, Action<GameObject> build)
        {
            var root = new GameObject(fileName);
            try
            {
                var tag = root.AddComponent<NaadPlaceholderTag>();
                tag.AssetId = assetId;
                tag.Description = description;
                build(root);
                var path = EnvDir + "/" + fileName + ".prefab";
                PrefabUtility.SaveAsPrefabAsset(root, path);
                Debug.Log("[NAAD][Prefabs] Saved " + path + " (" + assetId + ").");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static void AddBox(GameObject parent, string name, Vector3 pos,
            Vector3 scale, Color color, bool emissive = false, bool glassy = false)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent.transform, false);
            go.transform.localPosition = pos;
            go.transform.localScale = scale;
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit")
                    ?? Shader.Find("Standard");
                var mat = new Material(shader);
                mat.color = color;
                if (glassy)
                {
                    var c = mat.color;
                    c.a = 0.45f;
                    mat.color = c;
                }
                if (emissive)
                    mat.SetColor("_EmissionColor", color * 1.5f);
                renderer.sharedMaterial = mat;
            }
        }
    }
}

