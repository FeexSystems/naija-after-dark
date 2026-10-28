using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using NAAD.Characters.Player;
using NAAD.Gameplay.Interaction;
using System.Linq;

namespace NAAD.Editor
{
    public class WireComponents
    {
        [MenuItem("NAAD/Wire Components/All Scenes")]
        public static void WireAllScenes()
        {
            WireApartment();
            WireCityStreet();
            WireSuyaSpot();
            WireNightclub();
            WireBeach();
            
            Debug.Log("All scenes have been wired with components.");
        }
        
        [MenuItem("NAAD/Wire Components/Apartment")]
        public static void WireApartment()
        {
            WireScene("Assets/NAAD/Scenes/Apartment.unity");
        }
        
        [MenuItem("NAAD/Wire Components/CityStreet")]
        public static void WireCityStreet()
        {
            WireScene("Assets/NAAD/Scenes/CityStreet.unity");
        }
        
        [MenuItem("NAAD/Wire Components/SuyaSpot")]
        public static void WireSuyaSpot()
        {
            WireScene("Assets/NAAD/Scenes/SuyaSpot.unity");
        }
        
        [MenuItem("NAAD/Wire Components/Nightclub")]
        public static void WireNightclub()
        {
            WireScene("Assets/NAAD/Scenes/Nightclub.unity");
        }
        
        [MenuItem("NAAD/Wire Components/Beach")]
        public static void WireBeach()
        {
            WireScene("Assets/NAAD/Scenes/Beach.unity");
        }
        
        private static void WireScene(string scenePath)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            
            // Find Player GameObject and add PlayerLocomotionController
            var player = GameObject.Find("Player");
            if (player != null)
            {
                var locomotion = player.GetComponent<PlayerLocomotionController>();
                if (locomotion == null)
                {
                    player.AddComponent<PlayerLocomotionController>();
                    Debug.Log($"Added PlayerLocomotionController to Player in {scenePath}");
                }
            }
            
            // Find ThirdPersonCamera and wire to Player
            var cameraObj = GameObject.Find("ThirdPersonCamera");
            if (cameraObj != null && player != null)
            {
                var cameraController = cameraObj.GetComponent<ThirdPersonCamera>();
                if (cameraController != null)
                {
                    // Camera will need manual follow target assignment in Inspector
                    Debug.Log($"ThirdPersonCamera found in {scenePath} - needs manual follow target assignment");
                }
            }
            
            // Find all interaction points and add Interactable component
            var interactionPoints = GameObject.FindObjectsOfType<Transform>()
                .Where(t => t.name.StartsWith("Door_") || t.name.StartsWith("Table_") || 
                          t.name.StartsWith("Bed") || t.name.StartsWith("Phone") || t.name.StartsWith("Fridge") ||
                          t.name.StartsWith("Suya_") || t.name.StartsWith("Bar") || t.name.StartsWith("DJ_") ||
                          t.name.StartsWith("Dance_") || t.name.StartsWith("VIP_") || t.name.StartsWith("Restroom") ||
                          t.name.StartsWith("Taxi_") || t.name.StartsWith("Street_") || t.name.StartsWith("Beach_") ||
                          t.name.StartsWith("Water_") || t.name.StartsWith("Message") || t.name.StartsWith("Receipt"))
                .Select(t => t.gameObject)
                .ToList();
            
            foreach (var point in interactionPoints)
            {
                var interactable = point.GetComponent<Interactable>();
                if (interactable == null)
                {
                    point.AddComponent<Interactable>();
                }
            }
            
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"Wired components in {scenePath}");
        }
    }
}
