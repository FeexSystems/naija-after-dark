using UnityEditor;
using UnityEngine;

namespace NAAD.Editor
{
    public static class ExtractPlayerAvatar
    {
        [MenuItem("NAAD/Extract Player Avatar")]
        public static void Run()
        {
            var rigPath = "Assets/NAAD/Animation/Characters/Player/PlayerRig.fbx";
            var avatarPath = "Assets/NAAD/Animation/Avatars/PlayerAvatar.asset";

            // Load all assets from the FBX
            var allAssets = AssetDatabase.LoadAllAssetsAtPath(rigPath);
            Avatar avatar = null;

            foreach (var asset in allAssets)
            {
                if (asset is Avatar a && a.isValid && a.isHuman)
                {
                    avatar = a;
                    break;
                }
            }

            if (avatar == null)
            {
                Debug.LogError($"No valid humanoid Avatar found in {rigPath}");
                EditorApplication.Exit(1);
                return;
            }

            // Ensure directory exists
            var directory = System.IO.Path.GetDirectoryName(avatarPath);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }

            // Create the Avatar asset
            AssetDatabase.CreateAsset(avatar, avatarPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[NAAD][GateC] Extracted Avatar to {avatarPath}");
            EditorApplication.Exit(0);
        }
    }
}
