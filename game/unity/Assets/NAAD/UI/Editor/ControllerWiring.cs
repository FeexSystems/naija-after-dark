using System;
using System.Collections.Generic;
using UnityEditor;
using NAAD.Networking;
using NAAD.Networking.Supabase;
using NAAD.UI.Scripts;
using UnityEngine;

namespace NAAD.Editor
{
    /// <summary>
    /// Editor utility that wires UI controllers to mock services for automated testing.
    /// <para>
    /// Creates a prefab-less test environment: instantiates the required views,
    /// registers mock services, and wires controller-view bindings via
    /// <see cref="ScreenNavigationManager"/>.
    /// </para>
    /// <para>
    /// Invoke via the menu: <b>NAAD > UI > Wire Mock Services</b>.
    /// </para>
    /// </summary>
    public static class ControllerWiring
    {
        private const string TestCanvasPath = "Assets/NAAD/UI/TestEnvironment/";

        [MenuItem("NAAD/UI/Wire Mock Services", priority = 350)]
        public static void WireMockServices()
        {
            // Clear any existing registrations
            ServiceContainer.Clear();

            // Register mock services
            ServiceContainer.RegisterMockServices();

            // Create test environment canvas as a prefab
            var canvas = CreateTestCanvas();
            var manager = canvas.AddComponent<ScreenNavigationManager>();

            Debug.Log("[ControllerWiring] Mock services registered.");
            Debug.Log("[ControllerWiring] Test environment canvas created.");
        }

        private static GameObject CreateTestCanvas()
        {
            var go = new GameObject("TestEnvironment");
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);

            var raycaster = go.AddComponent<GraphicRaycaster>();
            var safeArea = go.AddComponent<SafeAreaFitter>();

            var manager = go.AddComponent<ScreenNavigationManager>();

            // Create a navigation root
            var navRoot = new GameObject("NavigationRoot");
            navRoot.transform.SetParent(go.transform, false);

            return go;
        }

        [MenuItem("NAAD/UI/Wire View Controllers", priority = 351)]
        public static void WireViewControllers()
        {
            Debug.Log("[ControllerWiring] View-controller wiring not yet implemented.");
            Debug.Log("[ControllerWiring] See NAAD.UI.Scripts for ControllerBase and view scripts.");
        }
    }
}