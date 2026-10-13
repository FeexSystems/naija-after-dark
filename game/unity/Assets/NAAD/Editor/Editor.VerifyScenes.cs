using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NAAD.Editor
{
    public static partial class Editor
    {
        /// <summary>
        /// `-executeMethod NAAD.Editor.VerifyScenes`
        ///
        /// Loads every enabled scene from Build Settings and checks it opens without throwing and
        /// contains at least one root object. Scenes declared in Build Settings are authoritative:
        /// anything not listed is reported as absent, not as a failure.
        /// </summary>
        public static void VerifyScenes()
        {
            Debug.Log("[NAAD][Verify] Verifying scenes...");
            var report = new VerificationReport();

            var scenes = EditorBuildSettings.scenes;
            if (scenes == null || scenes.Length == 0)
            {
                Fail(report, "No scenes in Build Settings — nothing to verify.");
                Finish("VerifyScenes", report);
                return;
            }

            var loadedNames = new List<string>();
            for (var i = 0; i < scenes.Length; i++)
            {
                var scene = scenes[i];
                if (!scene.enabled)
                {
                    Debug.Log($"[NAAD][Verify] Scene index {i} disabled, skipped: {scene.path}");
                    continue;
                }

                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path) == null)
                {
                    Fail(report, $"Scene asset missing or unloadable: {scene.path}");
                    continue;
                }

                Scene opened = default;
                try
                {
                    opened = EditorSceneManager.OpenScene(scene.path, OpenSceneMode.Single);
                }
                catch (System.Exception ex)
                {
                    Fail(report, $"Scene failed to open: {scene.path} ({ex.Message})");
                    continue;
                }

                var rootCount = opened.GetRootGameObjects().Length;
                if (rootCount == 0)
                {
                    Fail(report, $"Scene has no root GameObjects: {scene.path}");
                }
                else
                {
                    Pass(report, $"Scene opens with {rootCount} root object(s): {scene.path}");
                }

                loadedNames.Add(opened.name);
            }

            Pass(report, $"Scenes verified: {string.Join(", ", loadedNames)}");
            Finish("VerifyScenes", report);
        }

        /// <summary>
        /// `-executeMethod NAAD.Editor.VerifyEnvironmentScenes`
        ///
        /// Delegates to the existing environment verifier (menu entry: NAAD > Verify Environment
        /// Scenes) so CLI and menu share one implementation. Exits the batch editor when finished.
        /// </summary>
        public static void VerifyEnvironmentScenes()
        {
            Debug.Log("[NAAD][Verify] Verifying environment scenes...");
            VerifyEnvironmentScenes.RunVerification();
            ExitBatch(0);
        }
    }
}
