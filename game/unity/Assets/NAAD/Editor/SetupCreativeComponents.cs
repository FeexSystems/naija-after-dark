using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace NAAD.Editor
{
    public class SetupCreativeComponents
    {
        [MenuItem("NAAD/Setup Creative Components/All Creative Features")]
        public static void SetupAllCreativeFeatures()
        {
            SetupRainSystem();
            SetupBonfire();
            
            Debug.Log("All creative components have been set up.");
        }
        
        [MenuItem("NAAD/Setup Creative Components/Setup Rain System")]
        public static void SetupRainSystem()
        {
            var scene = EditorSceneManager.OpenScene("Assets/NAAD/Scenes/CityStreet.unity", OpenSceneMode.Single);
            
            var weatherSystem = GameObject.Find("WeatherSystem");
            var rainSystem = GameObject.Find("RainSystem");
            
            if (weatherSystem != null && rainSystem != null)
            {
                var rainParticles = rainSystem.GetComponent<ParticleSystem>();
                if (rainParticles == null)
                {
                    rainParticles = rainSystem.AddComponent<ParticleSystem>();
                    
                    var main = rainParticles.main;
                    main.startLifetime = 2f;
                    main.startSpeed = 10f;
                    main.startSize = 0.1f;
                    main.gravityModifier = 1f;
                    main.maxParticles = 1000;
                    
                    var emission = rainParticles.emission;
                    emission.rateOverTime = 100;
                    
                    var shape = rainParticles.shape;
                    shape.shapeType = ParticleSystemShapeType.Box;
                    shape.scale = new Vector3(50, 0, 50);
                    
                    Debug.Log("Added rain particle system to CityStreet");
                }
            }
            
            EditorSceneManager.SaveScene(scene);
        }
        
        [MenuItem("NAAD/Setup Creative Components/Setup Bonfire")]
        public static void SetupBonfire()
        {
            var scene = EditorSceneManager.OpenScene("Assets/NAAD/Scenes/Beach.unity", OpenSceneMode.Single);
            
            var nightBeachObjects = GameObject.Find("NightBeachObjects");
            if (nightBeachObjects != null)
            {
                var bonfire = GameObject.Find("Bonfire");
                if (bonfire == null)
                {
                    bonfire = new GameObject("Bonfire");
                    bonfire.transform.SetParent(nightBeachObjects.transform);
                    bonfire.transform.position = new Vector3(0, 0.5f, 10);
                    var bonfireLight = bonfire.AddComponent<Light>();
                    bonfireLight.type = LightType.Point;
                    bonfireLight.intensity = 3f;
                    bonfireLight.range = 10f;
                    bonfireLight.color = new Color(1f, 0.5f, 0.2f);
                    
                    Debug.Log("Added bonfire to Beach");
                }
            }
            
            EditorSceneManager.SaveScene(scene);
        }
    }
}
