using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using NAAD.Core.Bootstrap;

namespace NAAD.Editor
{
    /// <summary>
    /// Editor utility that registers all NAAD MVP scenes in Build Settings in
    /// canonical order (Bootstrap at index 0, then Apartment, CityStreet,
    /// SuyaSpot, Nightclub, Beach).
    /// <para>
    /// Invoke via the menu: <b>NAAD &gt; Scenes &gt; Register Build Settings</b>.
    /// </para>
    /// </summary>
    public static class BuildSettingsRegister
    {
        [MenuItem("NAAD/Scenes/Register Build Settings", priority = 101)]
        public static void RegisterScenes()
        {
            var scenes = NaadSceneNames.AllScenes
                .Select(s => new EditorBuildSettingsScene(s.path, enabled: true))
                .ToArray();

            EditorBuildSettings.scenes = scenes;

            Debug.Log($"[BuildSettingsRegister] Registered {scenes.Length} scene(s) in Build Settings:");
            for (var i = 0; i < scenes.Length; i++)
            {
                Debug.Log($"  Index {i}: {scenes[i].path} (enabled={scenes[i].enabled})");
            }

            AssetDatabase.SaveAssets();
        }

        [MenuItem("NAAD/Scenes/Validate Build Settings", priority = 102)]
        public static void ValidateBuildSettings()
        {
            var scenes = EditorBuildSettings.scenes;
            if (scenes == null || scenes.Length == 0)
            {
                Debug.LogError("[BuildSettingsRegister] No scenes configured in Build Settings.");
                return;
            }

            var expected = NaadSceneNames.AllScenes.ToArray();
            var allValid = true;

            if (scenes.Length < expected.Length)
            {
                Debug.LogError($"[BuildSettingsRegister] Expected {expected.Length} scenes, found {scenes.Length}.");
                allValid = false;
            }

            for (var i = 0; i < expected.Length && i < scenes.Length; i++)
            {
                var expectedPath = expected[i].path;
                var actualPath = scenes[i].path;

                if (!actualPath.Equals(expectedPath, System.StringComparison.OrdinalIgnoreCase))
                {
                    Debug.LogError($"[BuildSettingsRegister] Index {i} mismatch: expected '{expectedPath}', found '{actualPath}'.");
                    allValid = false;
                }

                if (!scenes[i].enabled)
                {
                    Debug.LogError($"[BuildSettingsRegister] Index {i} is disabled: {actualPath}.");
                    allValid = false;
                }

                var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(actualPath);
                if (sceneAsset == null)
                {
                    Debug.LogError($"[BuildSettingsRegister] Scene asset missing or unloadable: {actualPath}.");
                    allValid = false;
                }
            }

            if (allValid)
            {
                Debug.Log("[BuildSettingsRegister] Build Settings validation PASSED — all scenes registered and enabled.");
            }
        }
    }
}