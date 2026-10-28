using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

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
            
            Debug.Log("All scenes have been set up with lighting GameObjects.");
        }
        
        [MenuItem("NAAD/Setup World Clock Lighting/Apartment")]
        public static void SetupApartmentLighting()
        {
            var scene = EditorSceneManager.OpenScene("Assets/NAAD/Scenes/Apartment.unity", OpenSceneMode.Single);
            
            var worldClockPresenter = new GameObject("WorldClockPresenter");
            var lightingProfile = new GameObject("NightLightingProfile");
            
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Setup world clock lighting for Apartment scene");
        }
        
        [MenuItem("NAAD/Setup World Clock Lighting/CityStreet")]
        public static void SetupCityStreetLighting()
        {
            var scene = EditorSceneManager.OpenScene("Assets/NAAD/Scenes/CityStreet.unity", OpenSceneMode.Single);
            
            var worldClockPresenter = new GameObject("WorldClockPresenter");
            var streetLights = new GameObject("StreetLights");
            
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Setup world clock lighting for CityStreet scene");
        }
        
        [MenuItem("NAAD/Setup World Clock Lighting/SuyaSpot")]
        public static void SetupSuyaSpotLighting()
        {
            var scene = EditorSceneManager.OpenScene("Assets/NAAD/Scenes/SuyaSpot.unity", OpenSceneMode.Single);
            
            var worldClockPresenter = new GameObject("WorldClockPresenter");
            var suyaLights = new GameObject("SuyaLights");
            
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Setup world clock lighting for SuyaSpot scene");
        }
        
        [MenuItem("NAAD/Setup World Clock Lighting/Nightclub")]
        public static void SetupNightclubLighting()
        {
            var scene = EditorSceneManager.OpenScene("Assets/NAAD/Scenes/Nightclub.unity", OpenSceneMode.Single);
            
            var worldClockPresenter = new GameObject("WorldClockPresenter");
            var clubLights = new GameObject("ClubLights");
            
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Setup world clock lighting for Nightclub scene");
        }
        
        [MenuItem("NAAD/Setup World Clock Lighting/Beach")]
        public static void SetupBeachLighting()
        {
            var scene = EditorSceneManager.OpenScene("Assets/NAAD/Scenes/Beach.unity", OpenSceneMode.Single);
            
            var worldClockPresenter = new GameObject("WorldClockPresenter");
            
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Setup world clock lighting for Beach scene");
        }
    }
}
