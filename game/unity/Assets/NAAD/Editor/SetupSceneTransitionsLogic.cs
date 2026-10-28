using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace NAAD.Editor
{
    public class SetupSceneTransitionsLogic
    {
        [MenuItem("NAAD/Setup Scene Transitions Logic/All Scenes")]
        public static void SetupAllScenes()
        {
            SetupApartmentTransitions();
            SetupCityStreetTransitions();
            SetupSuyaSpotTransitions();
            SetupNightclubTransitions();
            SetupBeachTransitions();
            
            Debug.Log("All scenes have scene transitions configured.");
        }
        
        [MenuItem("NAAD/Setup Scene Transitions Logic/Apartment")]
        public static void SetupApartmentTransitions()
        {
            var scene = EditorSceneManager.OpenScene("Assets/NAAD/Scenes/Apartment.unity", OpenSceneMode.Single);
            
            var door = GameObject.Find("Door_Apartment_To_Street");
            if (door != null)
            {
                var sceneLoader = door.GetComponent<SceneLoader>();
                if (sceneLoader == null)
                {
                    sceneLoader = door.AddComponent<SceneLoader>();
                }
            }
            
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Setup scene transitions for Apartment");
        }
        
        [MenuItem("NAAD/Setup Scene Transitions Logic/CityStreet")]
        public static void SetupCityStreetTransitions()
        {
            var scene = EditorSceneManager.OpenScene("Assets/NAAD/Scenes/CityStreet.unity", OpenSceneMode.Single);
            
            var doors = new[] { "Door_Street_To_Apartment", "Door_Street_To_SuyaSpot", "Door_Street_To_Nightclub" };
            foreach (var doorName in doors)
            {
                var door = GameObject.Find(doorName);
                if (door != null)
                {
                    var sceneLoader = door.GetComponent<SceneLoader>();
                    if (sceneLoader == null)
                    {
                        sceneLoader = door.AddComponent<SceneLoader>();
                    }
                }
            }
            
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Setup scene transitions for CityStreet");
        }
        
        [MenuItem("NAAD/Setup Scene Transitions Logic/SuyaSpot")]
        public static void SetupSuyaSpotTransitions()
        {
            var scene = EditorSceneManager.OpenScene("Assets/NAAD/Scenes/SuyaSpot.unity", OpenSceneMode.Single);
            
            var door = GameObject.Find("Door_SuyaSpot_To_Street");
            if (door != null)
            {
                var sceneLoader = door.GetComponent<SceneLoader>();
                if (sceneLoader == null)
                {
                    sceneLoader = door.AddComponent<SceneLoader>();
                }
            }
            
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Setup scene transitions for SuyaSpot");
        }
        
        [MenuItem("NAAD/Setup Scene Transitions Logic/Nightclub")]
        public static void SetupNightclubTransitions()
        {
            var scene = EditorSceneManager.OpenScene("Assets/NAAD/Scenes/Nightclub.unity", OpenSceneMode.Single);
            
            var door = GameObject.Find("Door_Nightclub_To_Street");
            if (door != null)
            {
                var sceneLoader = door.GetComponent<SceneLoader>();
                if (sceneLoader == null)
                {
                    sceneLoader = door.AddComponent<SceneLoader>();
                }
            }
            
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Setup scene transitions for Nightclub");
        }
        
        [MenuItem("NAAD/Setup Scene Transitions Logic/Beach")]
        public static void SetupBeachTransitions()
        {
            var scene = EditorSceneManager.OpenScene("Assets/NAAD/Scenes/Beach.unity", OpenSceneMode.Single);
            
            // Beach currently has no door transitions in the base setup
            // This can be added later if needed
            
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Setup scene transitions for Beach");
        }
    }
}
