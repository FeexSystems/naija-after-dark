using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace NAAD.Editor
{
    public class SetupLightingComponents
    {
        [MenuItem("NAAD/Setup Lighting Components/All Scenes")]
        public static void SetupAllScenes()
        {
            SetupApartmentLighting();
            SetupCityStreetLighting();
            SetupSuyaSpotLighting();
            SetupNightclubLighting();
            SetupBeachLighting();
            
            Debug.Log("All scenes have been set up with lighting components.");
        }
        
        [MenuItem("NAAD/Setup Lighting Components/Apartment")]
        public static void SetupApartmentLighting()
        {
            var scene = EditorSceneManager.OpenScene("Assets/NAAD/Scenes/Apartment.unity", OpenSceneMode.Single);
            
            var directionalLight = GameObject.Find("Directional Light");
            if (directionalLight == null)
            {
                directionalLight = new GameObject("Directional Light");
                var light = directionalLight.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1f;
                light.color = new Color(1f, 0.95f, 0.8f);
                directionalLight.transform.rotation = Quaternion.Euler(50, -30, 0);
            }
            
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Setup lighting for Apartment");
        }
        
        [MenuItem("NAAD/Setup Lighting Components/CityStreet")]
        public static void SetupCityStreetLighting()
        {
            var scene = EditorSceneManager.OpenScene("Assets/NAAD/Scenes/CityStreet.unity", OpenSceneMode.Single);
            
            var streetLights = GameObject.Find("StreetLights");
            if (streetLights != null)
            {
                for (int i = 0; i < 5; i++)
                {
                    var lightObj = new GameObject($"StreetLight_{i}");
                    lightObj.transform.SetParent(streetLights.transform);
                    lightObj.transform.position = new Vector3(i * 10 - 20, 5, 0);
                    var light = lightObj.AddComponent<Light>();
                    light.type = LightType.Point;
                    light.intensity = 2f;
                    light.range = 15f;
                    light.color = new Color(1f, 0.9f, 0.7f);
                }
            }
            
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Setup lighting for CityStreet");
        }
        
        [MenuItem("NAAD/Setup Lighting Components/SuyaSpot")]
        public static void SetupSuyaSpotLighting()
        {
            var scene = EditorSceneManager.OpenScene("Assets/NAAD/Scenes/SuyaSpot.unity", OpenSceneMode.Single);
            
            var suyaLights = GameObject.Find("SuyaLights");
            if (suyaLights != null)
            {
                var mainLight = new GameObject("MainSuyaLight");
                mainLight.transform.SetParent(suyaLights.transform);
                mainLight.transform.position = new Vector3(0, 4, 0);
                var light = mainLight.AddComponent<Light>();
                light.type = LightType.Point;
                light.intensity = 3f;
                light.range = 20f;
                light.color = new Color(1f, 0.7f, 0.3f);
            }
            
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Setup lighting for SuyaSpot");
        }
        
        [MenuItem("NAAD/Setup Lighting Components/Nightclub")]
        public static void SetupNightclubLighting()
        {
            var scene = EditorSceneManager.OpenScene("Assets/NAAD/Scenes/Nightclub.unity", OpenSceneMode.Single);
            
            var clubLights = GameObject.Find("ClubLights");
            if (clubLights != null)
            {
                var colors = new Color[] { Color.red, Color.blue, Color.magenta, Color.cyan };
                for (int i = 0; i < 4; i++)
                {
                    var lightObj = new GameObject($"ClubLight_{i}");
                    lightObj.transform.SetParent(clubLights.transform);
                    lightObj.transform.position = new Vector3((i - 1.5f) * 5, 6, 0);
                    var light = lightObj.AddComponent<Light>();
                    light.type = LightType.Point;
                    light.intensity = 2f;
                    light.range = 15f;
                    light.color = colors[i];
                }
            }
            
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Setup lighting for Nightclub");
        }
        
        [MenuItem("NAAD/Setup Lighting Components/Beach")]
        public static void SetupBeachLighting()
        {
            var scene = EditorSceneManager.OpenScene("Assets/NAAD/Scenes/Beach.unity", OpenSceneMode.Single);
            
            var directionalLight = GameObject.Find("Directional Light");
            if (directionalLight == null)
            {
                directionalLight = new GameObject("Directional Light");
                var light = directionalLight.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.2f;
                light.color = new Color(1f, 1f, 0.9f);
                directionalLight.transform.rotation = Quaternion.Euler(50, -30, 0);
            }
            
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Setup lighting for Beach");
        }
    }
}
