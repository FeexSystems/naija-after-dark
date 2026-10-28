using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using NAAD.Gameplay.World;

namespace NAAD.Editor
{
    public class SetupWorldClockLighting
    {
        [MenuItem("NAAD/Setup World Clock Lighting/All Scenes")]
        public static void SetupAllScenes()
        {
            SetupApartmentLighting();
            SetupCityStreetLighting();
            SetupSuyaSpotLighting();
            SetupNightclubLighting();
            SetupBeachLighting();
            
            Debug.Log("All scenes have been set up with world clock lighting.");
        }
        
        [MenuItem("NAAD/Setup World Clock Lighting/Apartment")]
        public static void SetupApartmentLighting()
        {
            var scene = EditorSceneManager.OpenScene("Assets/NAAD/Scenes/Apartment.unity", OpenSceneMode.Single);
            
            // Find WorldClockService
            var worldClockService = GameObject.Find("WorldClockService");
            if (worldClockService == null)
            {
                Debug.LogError("WorldClockService not found. Run SetupScenes first.");
                return;
            }
            
            var clockService = worldClockService.GetComponent<WorldClockService>();
            
            // Find WorldClockPresenter or create it
            var presenter = GameObject.Find("WorldClockPresenter");
            if (presenter == null)
            {
                presenter = new GameObject("WorldClockPresenter");
                var presenterComponent = presenter.AddComponent<WorldClockPresenter>();
                presenterComponent.worldClockService = clockService;
            }
            
            // Create or find NightLightingProfile
            var lightingProfile = GameObject.Find("NightLightingProfile");
            if (lightingProfile == null)
            {
                lightingProfile = new GameObject("NightLightingProfile");
                var profile = lightingProfile.AddComponent<NightLightingProfile>();
                
                // Configure basic lighting settings
                RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
                RenderSettings.ambientLight = new Color(0.4f, 0.4f, 0.5f); // Cool ambient
                
                // Setup directional light (sun/moon)
                var directionalLight = GameObject.Find("Directional Light");
                if (directionalLight == null)
                {
                    directionalLight = new GameObject("Directional Light");
                    var light = directionalLight.AddComponent<Light>();
                    light.type = LightType.Directional;
                    light.intensity = 1f;
                    light.color = new Color(1f, 0.95f, 0.8f); // Warm sunlight
                }
            }
            
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Setup world clock lighting for Apartment scene");
        }
        
        [MenuItem("NAAD/Setup World Clock Lighting/CityStreet")]
        public static void SetupCityStreetLighting()
        {
            var scene = EditorSceneManager.OpenScene("Assets/NAAD/Scenes/CityStreet.unity", OpenSceneMode.Single);
            
            var worldClockService = GameObject.Find("WorldClockService");
            if (worldClockService == null)
            {
                Debug.LogError("WorldClockService not found. Run SetupScenes first.");
                return;
            }
            
            var clockService = worldClockService.GetComponent<WorldClockService>();
            
            var presenter = GameObject.Find("WorldClockPresenter");
            if (presenter == null)
            {
                presenter = new GameObject("WorldClockPresenter");
                var presenterComponent = presenter.AddComponent<WorldClockPresenter>();
                presenterComponent.worldClockService = clockService;
            }
            
            // City street lighting - add street lights
            var streetLights = new GameObject("StreetLights");
            streetLights.transform.position = new Vector3(0, 0, 0);
            
            // Add a few street light points
            for (int i = 0; i < 5; i++)
            {
                var light = new GameObject($"StreetLight_{i}");
                light.transform.SetParent(streetLights.transform);
                light.transform.position = new Vector3(i * 10 - 20, 5, 0);
                var lightComponent = light.AddComponent<Light>();
                lightComponent.type = LightType.Point;
                lightComponent.intensity = 2f;
                lightComponent.range = 15f;
                lightComponent.color = new Color(1f, 0.9f, 0.7f); // Warm street light
            }
            
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Setup world clock lighting for CityStreet scene");
        }
        
        [MenuItem("NAAD/Setup World Clock Lighting/SuyaSpot")]
        public static void SetupSuyaSpotLighting()
        {
            var scene = EditorSceneManager.OpenScene("Assets/NAAD/Scenes/SuyaSpot.unity", OpenSceneMode.Single);
            
            var worldClockService = GameObject.Find("WorldClockService");
            if (worldClockService == null)
            {
                Debug.LogError("WorldClockService not found. Run SetupScenes first.");
                return;
            }
            
            var clockService = worldClockService.GetComponent<WorldClockService>();
            
            var presenter = GameObject.Find("WorldClockPresenter");
            if (presenter == null)
            {
                presenter = new GameObject("WorldClockPresenter");
                var presenterComponent = presenter.AddComponent<WorldClockPresenter>();
                presenterComponent.worldClockService = clockService;
            }
            
            // Suya spot lighting - warm, inviting
            var suyaLights = new GameObject("SuyaLights");
            suyaLights.transform.position = new Vector3(0, 0, 0);
            
            var mainLight = new GameObject("MainSuyaLight");
            mainLight.transform.SetParent(suyaLights.transform);
            mainLight.transform.position = new Vector3(0, 4, 0);
            var lightComponent = mainLight.AddComponent<Light>();
            lightComponent.type = LightType.Point;
            lightComponent.intensity = 3f;
            lightComponent.range = 20f;
            lightComponent.color = new Color(1f, 0.7f, 0.3f); // Warm orange
            
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Setup world clock lighting for SuyaSpot scene");
        }
        
        [MenuItem("NAAD/Setup World Clock Lighting/Nightclub")]
        public static void SetupNightclubLighting()
        {
            var scene = EditorSceneManager.OpenScene("Assets/NAAD/Scenes/Nightclub.unity", OpenSceneMode.Single);
            
            var worldClockService = GameObject.Find("WorldClockService");
            if (worldClockService == null)
            {
                Debug.LogError("WorldClockService not found. Run SetupScenes first.");
                return;
            }
            
            var clockService = worldClockService.GetComponent<WorldClockService>();
            
            var presenter = GameObject.Find("WorldClockPresenter");
            if (presenter == null)
            {
                presenter = new GameObject("WorldClockPresenter");
                var presenterComponent = presenter.AddComponent<WorldClockPresenter>();
                presenterComponent.worldClockService = clockService;
            }
            
            // Nightclub lighting - dynamic, colorful
            var clubLights = new GameObject("ClubLights");
            clubLights.transform.position = new Vector3(0, 0, 0);
            
            // Add colored lights
            var colors = new Color[] { Color.red, Color.blue, Color.magenta, Color.cyan };
            for (int i = 0; i < 4; i++)
            {
                var light = new GameObject($"ClubLight_{i}");
                light.transform.SetParent(clubLights.transform);
                light.transform.position = new Vector3((i - 1.5f) * 5, 6, 0);
                var lightComponent = light.AddComponent<Light>();
                lightComponent.type = LightType.Point;
                lightComponent.intensity = 2f;
                lightComponent.range = 15f;
                lightComponent.color = colors[i];
            }
            
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Setup world clock lighting for Nightclub scene");
        }
        
        [MenuItem("NAAD/Setup World Clock Lighting/Beach")]
        public static void SetupBeachLighting()
        {
            var scene = EditorSceneManager.OpenScene("Assets/NAAD/Scenes/Beach.unity", OpenSceneMode.Single);
            
            var worldClockService = GameObject.Find("WorldClockService");
            if (worldClockService == null)
            {
                Debug.LogError("WorldClockService not found. Run SetupScenes first.");
                return;
            }
            
            var clockService = worldClockService.GetComponent<WorldClockService>();
            
            var presenter = GameObject.Find("WorldClockPresenter");
            if (presenter == null)
            {
                presenter = new GameObject("WorldClockPresenter");
                var presenterComponent = presenter.AddComponent<WorldClockPresenter>();
                presenterComponent.worldClockService = clockService;
            }
            
            // Beach lighting - natural sun/moon cycle
            var directionalLight = GameObject.Find("Directional Light");
            if (directionalLight == null)
            {
                directionalLight = new GameObject("Directional Light");
                var light = directionalLight.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.2f;
                light.color = new Color(1f, 1f, 0.9f); // Bright sunlight
                directionalLight.transform.rotation = Quaternion.Euler(50, -30, 0);
            }
            
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Setup world clock lighting for Beach scene");
        }
    }
}
