using System.Collections.Generic;
using System.IO;
using System.Linq;
using NAAD.UI;
using NAAD.UI.Scripts;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NAAD.Editor
{
    /// <summary>
    /// Mount the generated UI shell into the Bootstrap scene.
    ///
    /// <para>
    /// The screen prefabs under <c>Assets/NAAD/UI/Views/</c> existed but nothing instantiated
    /// them, and nothing ever called <c>ScreenNavigationManager.RegisterView</c> — so the only
    /// UI at runtime was the IMGUI <c>FirstNightHud</c>. This builds the canvas, instantiates
    /// every view and the matching controller, and leaves navigation to
    /// <c>RegisterChildViews()</c> + <c>RegisterControllersAuto()</c> at runtime.
    /// </para>
    ///
    /// <para>
    /// Idempotent: an existing <c>UIRoot</c> is reused and missing views are added, so this can
    /// be re-run after the factory regenerates a prefab. It never deletes anything.
    /// </para>
    ///
    /// Invoke: <b>NAAD &gt; UI &gt; Mount UI Shell In Bootstrap</b>, or headless
    /// <c>-executeMethod NAAD.Editor.WireUIShellIntoBootstrap.Run</c>
    /// </summary>
    public static class WireUIShellIntoBootstrap
    {
        private const string BootstrapScene = "Assets/Scenes/Bootstrap.unity";
        private const string ViewsFolder = "Assets/NAAD/UI/Views";
        private const string RootName = "UIRoot";

        [MenuItem("NAAD/UI/Mount UI Shell In Bootstrap", priority = 320)]
        public static void Run()
        {
            if (!File.Exists(BootstrapScene))
            {
                Debug.LogError($"[NAAD][UI] Missing scene: {BootstrapScene}");
                return;
            }

            var prefabs = Directory.GetFiles(ViewsFolder, "*.prefab")
                .Select(Path.GetFileNameWithoutExtension)
                .OrderBy(n => n)
                .ToArray();

            if (prefabs.Length == 0)
            {
                Debug.LogError($"[NAAD][UI] No view prefabs in {ViewsFolder}. Run NAAD > UI > Create All Screen Prefabs first.");
                return;
            }

            var scene = EditorSceneManager.OpenScene(BootstrapScene, OpenSceneMode.Single);

            var root = scene.GetRootGameObjects().FirstOrDefault(go => go.name == RootName);
            if (root == null)
            {
                root = new GameObject(RootName);
            }

            // GetComponent<T>() ?? AddComponent<T>() is a trap: a missing component is
            // Unity-null but not C#-null, so ?? never fires and the next line throws
            // MissingComponentException. Always compare with == null.
            var canvas = root.GetComponent<Canvas>();
            if (canvas == null) canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = root.GetComponent<CanvasScaler>();
            if (scaler == null) scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            if (root.GetComponent<GraphicRaycaster>() == null)
                root.AddComponent<GraphicRaycaster>();

            if (root.GetComponent<ScreenNavigationManager>() == null)
                root.AddComponent<ScreenNavigationManager>();

            var viewsParent = EnsureChild(root, "Views");
            var added = 0;

            foreach (var name in prefabs)
            {
                if (viewsParent.transform.Find(name) != null) continue;

                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{ViewsFolder}/{name}.prefab");
                if (prefab == null)
                {
                    Debug.LogWarning($"[NAAD][UI] Could not load view prefab: {name}");
                    continue;
                }

                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, viewsParent.transform);
                instance.name = name;
                instance.SetActive(false);
                added++;
            }

            var controllers = EnsureChild(root, "Controllers");
            EnsureController<AuthController>(controllers);
            EnsureController<BootstrapScreenController>(controllers);
            EnsureController<BottomNavigationController>(controllers);
            EnsureController<ConfigurationUIController>(controllers);
            EnsureController<MainHudController>(controllers);
            EnsureController<NightSummaryController>(controllers);
            EnsureController<PhoneHomeController>(controllers);
            EnsureController<PingMessagingController>(controllers);

            EnsureEventSystem(scene);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();

            Debug.Log($"[NAAD][UI] UI shell mounted in Bootstrap: {prefabs.Length} view type(s), {added} instantiated.");
        }

        /// <summary>Fails if the Bootstrap scene has no UI shell, so it cannot silently regress.</summary>
        public static void Validate()
        {
            var errors = new List<string>();
            var scene = EditorSceneManager.OpenScene(BootstrapScene, OpenSceneMode.Single);
            var root = scene.GetRootGameObjects().FirstOrDefault(go => go.name == RootName);

            if (root == null) errors.Add("no UIRoot in Bootstrap");
            else
            {
                if (root.GetComponent<Canvas>() == null) errors.Add("UIRoot has no Canvas");
                if (root.GetComponent<ScreenNavigationManager>() == null) errors.Add("UIRoot has no ScreenNavigationManager");
                var views = root.GetComponentsInChildren<ScreenViewBase>(true);
                if (views.Length == 0) errors.Add("UIRoot has no ScreenViewBase children");
                var controllers = root.GetComponentsInChildren<BaseScreenController>(true);
                if (controllers.Length == 0) errors.Add("UIRoot has no BaseScreenController children");
            }

            if (scene.GetRootGameObjects().Any(go => go.GetComponentInChildren<EventSystem>(true) != null) == false)
                errors.Add("no EventSystem in Bootstrap");

            if (errors.Count > 0)
                throw new System.Exception("NAAD UI shell validation failed:\n - " + string.Join("\n - ", errors));

            Debug.Log("[NAAD][UI] UI shell validated in Bootstrap.");
        }

        private static GameObject EnsureChild(GameObject parent, string name)
        {
            var existing = parent.transform.Find(name);
            if (existing != null) return existing.gameObject;

            var child = new GameObject(name);
            child.transform.SetParent(parent.transform, false);
            return child;
        }

        private static void EnsureController<T>(GameObject host) where T : Component
        {
            if (host.GetComponent<T>() == null)
                host.AddComponent<T>();
        }

        private static void EnsureEventSystem(Scene scene)
        {
            var existing = scene.GetRootGameObjects()
                .Any(go => go.GetComponentInChildren<EventSystem>(true) != null);
            if (existing) return;

            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }
    }
}
