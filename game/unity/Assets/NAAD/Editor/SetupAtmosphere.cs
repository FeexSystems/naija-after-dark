using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace NAAD.Editor
{
    public class SetupAtmosphere
    {
        [MenuItem("NAAD/Setup Atmosphere/All Scenes")]
        public static void SetupAllScenes()
        {
            SetupCityStreetFog();
            SetupNightclubCrowd();
            
            Debug.Log("All scenes have atmosphere configured.");
        }
        
        [MenuItem("NAAD/Setup Atmosphere/CityStreet Fog")]
        public static void SetupCityStreetFog()
        {
            var scene = EditorSceneManager.OpenScene("Assets/NAAD/Scenes/CityStreet.unity", OpenSceneMode.Single);
            
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogDensity = 0.02f;
            RenderSettings.fogColor = new Color(0.5f, 0.5f, 0.6f);
            
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Setup fog for CityStreet");
        }
        
        [MenuItem("NAAD/Setup Atmosphere/Nightclub Crowd")]
        public static void SetupNightclubCrowd()
        {
            var scene = EditorSceneManager.OpenScene("Assets/NAAD/Scenes/Nightclub.unity", OpenSceneMode.Single);
            
            var crowdManager = GameObject.Find("CrowdManager");
            var danceFloorCrowd = GameObject.Find("DanceFloorCrowd");
            
            if (crowdManager != null)
            {
                for (int i = 0; i < 5; i++)
                {
                    var npc = new GameObject($"NightclubNPC_{i}");
                    npc.transform.position = new Vector3(
                        Random.Range(-5f, 5f),
                        1f,
                        Random.Range(-5f, 5f)
                    );
                    npc.transform.SetParent(crowdManager.transform);
                }
            }
            
            if (danceFloorCrowd != null)
            {
                for (int i = 0; i < 8; i++)
                {
                    var dancer = new GameObject($"Dancer_{i}");
                    dancer.transform.SetParent(danceFloorCrowd.transform);
                    dancer.transform.position = new Vector3(
                        Random.Range(-3f, 3f),
                        1f,
                        Random.Range(-3f, 3f)
                    );
                }
            }
            
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Setup crowd for Nightclub");
        }
    }
}
