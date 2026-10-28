using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using NAAD.Characters.Player;
using NAAD.Characters.Player.Placeholder;
using NAAD.Gameplay.World;

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
            
            Debug.Log("All scenes have been set up with core components.");
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
            
            // Create Player GameObject with PlayerLocomotionController
            var player = new GameObject("Player");
            player.transform.position = new Vector3(0, 1, 0);
            var locomotion = player.AddComponent<PlayerLocomotionController>();
            
            // Add WorldClockService
            var worldClockService = new GameObject("WorldClockService");
            worldClockService.AddComponent<WorldClockService>();
            
            // Add ThirdPersonCamera
            var cameraObj = new GameObject("ThirdPersonCamera");
            var camera = cameraObj.AddComponent<Camera>();
            cameraObj.AddComponent<ThirdPersonCamera>();
            camera.tag = "MainCamera";
            
            // Add Mobile Input Controller
            var mobileControls = new GameObject("MobileControls");
            mobileControls.AddComponent<MobileTouchControls>();
            
            // Save scene
            EditorSceneManager.SaveScene(scene);
            
            Debug.Log($"Setup {scenePath} with core components: PlayerSpawn, Player with PlayerLocomotionController, WorldClockService, ThirdPersonCamera, MobileTouchControls");
        }
    }
}
