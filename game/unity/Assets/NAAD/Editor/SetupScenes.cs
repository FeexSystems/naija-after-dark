using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace NAAD.Editor
{
    public class SetupScenes
    {
        [MenuItem("NAAD/Setup All Scenes")]
        public static void SetupAllScenes()
        {
            SetupApartmentScene();
            SetupCityStreetScene();
            SetupSuyaSpotScene();
            SetupNightclubScene();
            SetupBeachScene();
            
            Debug.Log("All scenes have been set up with core GameObjects.");
        }
        
        [MenuItem("NAAD/Setup Apartment Scene")]
        public static void SetupApartmentScene()
        {
            SetupScene("Assets/NAAD/Scenes/Apartment.unity");
        }
        
        [MenuItem("NAAD/Setup CityStreet Scene")]
        public static void SetupCityStreetScene()
        {
            SetupScene("Assets/NAAD/Scenes/CityStreet.unity");
        }
        
        [MenuItem("NAAD/Setup SuyaSpot Scene")]
        public static void SetupSuyaSpotScene()
        {
            SetupScene("Assets/NAAD/Scenes/SuyaSpot.unity");
        }
        
        [MenuItem("NAAD/Setup Nightclub Scene")]
        public static void SetupNightclubScene()
        {
            SetupScene("Assets/NAAD/Scenes/Nightclub.unity");
        }
        
        [MenuItem("NAAD/Setup Beach Scene")]
        public static void SetupBeachScene()
        {
            SetupScene("Assets/NAAD/Scenes/Beach.unity");
        }
        
        private static void SetupScene(string scenePath)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            
            // Create PlayerSpawn GameObject
            var playerSpawn = new GameObject("PlayerSpawn");
            playerSpawn.transform.position = new Vector3(0, 1, 0);
            
            // Create Player GameObject (components to be added manually)
            var player = new GameObject("Player");
            player.transform.position = new Vector3(0, 1, 0);
            
            // Create WorldClockService GameObject (service to be configured manually)
            var worldClockService = new GameObject("WorldClockService");
            
            // Create ThirdPersonCamera GameObject (camera to be configured manually)
            var cameraObj = new GameObject("ThirdPersonCamera");
            var camera = cameraObj.AddComponent<Camera>();
            camera.tag = "MainCamera";
            
            // Create Mobile Input Controller GameObject (controls to be configured manually)
            var mobileControls = new GameObject("MobileControls");
            
            // Save scene
            EditorSceneManager.SaveScene(scene);
            
            Debug.Log($"Setup {scenePath} with core GameObjects: PlayerSpawn, Player, WorldClockService, ThirdPersonCamera, MobileControls");
        }
    }
}
