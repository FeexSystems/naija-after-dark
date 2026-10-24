using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace NAAD.Editor
{
    [InitializeOnLoad]
    public static class SetupURP
    {
        static SetupURP()
        {
            EditorApplication.delayCall += ConfigureURP;
        }

        private static void ConfigureURP()
        {
            var urpAssetPath = "Assets/NAAD/URP/NAAD_URP_Asset.asset";
            var urpAsset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(urpAssetPath);
            
            if (urpAsset == null)
            {
                Debug.LogWarning("[NAAD][URP] URP Asset not found at " + urpAssetPath);
                return;
            }

            GraphicsSettings.defaultRenderPipeline = urpAsset;
            GraphicsSettings.renderPipelineAsset = urpAsset;

            UpgradeMaterialsToURP();
            UpgradeShadersToURP();

            AssetDatabase.SaveAssets();
            Debug.Log("[NAAD][URP] URP configured successfully with asset: " + urpAssetPath);
        }

        private static void UpgradeMaterialsToURP()
        {
            var guids = AssetDatabase.FindAssets("t:Material");
            int upgraded = 0;
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material != null && material.shader != null)
                {
                    string shaderName = material.shader.name;
                    string urpShader = GetURPShaderEquivalent(shaderName);
                    if (urpShader != shaderName)
                    {
                        var newShader = Shader.Find(urpShader);
                        if (newShader != null)
                        {
                            material.shader = newShader;
                            upgraded++;
                        }
                    }
                }
            }
            if (upgraded > 0)
                Debug.Log($"[NAAD][URP] Upgraded {upgraded} materials to URP shaders");
        }

        private static string GetURPShaderEquivalent(string builtinShader)
        {
            return builtinShader switch
            {
                "Standard" => "Universal Render Pipeline/Lit",
                "Standard (Specular setup)" => "Universal Render Pipeline/Lit",
                "Standard (Roughness setup)" => "Universal Render Pipeline/Lit",
                "Autodesk Interactive" => "Universal Render Pipeline/Lit",
                "Mobile/Diffuse" => "Universal Render Pipeline/Simple Lit",
                "Mobile/VertexLit" => "Universal Render Pipeline/Simple Lit",
                "Mobile/Particles/Alpha Blended" => "Universal Render Pipeline/Particles/Lit",
                "Mobile/Particles/Additive" => "Universal Render Pipeline/Particles/Lit",
                "Mobile/Particles/Multiply" => "Universal Render Pipeline/Particles/Lit",
                "Mobile/Particles/VertexLit" => "Universal Render Pipeline/Particles/Lit",
                "Particles/Alpha Blended" => "Universal Render Pipeline/Particles/Lit",
                "Particles/Additive" => "Universal Render Pipeline/Particles/Lit",
                "Particles/Multiply" => "Universal Render Pipeline/Particles/Lit",
                "Particles/VertexLit" => "Universal Render Pipeline/Particles/Lit",
                "Sprites/Default" => "Universal Render Pipeline/2D/Sprite-Lit-Default",
                "Sprites/Diffuse" => "Universal Render Pipeline/2D/Sprite-Lit-Default",
                "UI/Default" => "Universal Render Pipeline/UI/Default",
                "UI/DefaultFont" => "Universal Render Pipeline/UI/DefaultFont",
                "UI/Lit" => "Universal Render Pipeline/UI/Lit",
                "Unlit/Texture" => "Universal Render Pipeline/Unlit",
                "Unlit/Color" => "Universal Render Pipeline/Unlit",
                "Unlit/Transparent" => "Universal Render Pipeline/Unlit",
                "Unlit/Transparent Cutout" => "Universal Render Pipeline/Unlit",
                "Legacy Shaders/Diffuse" => "Universal Render Pipeline/Lit",
                "Legacy Shaders/VertexLit" => "Universal Render Pipeline/Lit",
                "Legacy Shaders/Bumped Diffuse" => "Universal Render Pipeline/Lit",
                "Legacy Shaders/Bumped Specular" => "Universal Render Pipeline/Lit",
                "Legacy Shaders/Parallax Diffuse" => "Universal Render Pipeline/Lit",
                "Legacy Shaders/Parallax Specular" => "Universal Render Pipeline/Lit",
                "Legacy Shaders/Decal" => "Universal Render Pipeline/Lit",
                "Legacy Shaders/Transparent/Diffuse" => "Universal Render Pipeline/Lit",
                "Legacy Shaders/Transparent/VertexLit" => "Universal Render Pipeline/Lit",
                "Legacy Shaders/Transparent/Bumped Diffuse" => "Universal Render Pipeline/Lit",
                "Legacy Shaders/Transparent/Bumped Specular" => "Universal Render Pipeline/Lit",
                "Legacy Shaders/Transparent/Parallax Diffuse" => "Universal Render Pipeline/Lit",
                "Legacy Shaders/Transparent/Parallax Specular" => "Universal Render Pipeline/Lit",
                _ => builtinShader
            };
        }

        private static void UpgradeShadersToURP()
        {
            var graphicsSettings = AssetDatabase.LoadAssetAtPath<GraphicsSettings>("ProjectSettings/GraphicsSettings.asset");
            if (graphicsSettings != null)
            {
                var so = new SerializedObject(graphicsSettings);
                var stripProp = so.FindProperty("m_StripUnusedShaders");
                if (stripProp != null)
                    stripProp.boolValue = true;
                so.ApplyModifiedProperties();
            }
        }

        [MenuItem("NAAD/URP/Reconfigure URP Settings")]
        public static void ReconfigureURPMenu()
        {
            ConfigureURP();
        }

        [MenuItem("NAAD/URP/Upgrade Project Materials to URP")]
        public static void UpgradeProjectMaterialsMenu()
        {
            UpgradeMaterialsToURP();
            UpgradeShadersToURP();
            AssetDatabase.SaveAssets();
            Debug.Log("[NAAD][URP] Material upgrade complete");
        }
    }
}