using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace NAAD.Editor
{
    public static class SetupPlayerController
    {
        [MenuItem("NAAD/Setup Player Controller")]
        public static void Run()
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/NAAD/Animation/Controllers/PlayerController.controller");
            if (controller == null)
            {
                Debug.LogError("PlayerController not found at Assets/NAAD/Animation/Controllers/PlayerController.controller");
                EditorApplication.Exit(1);
                return;
            }

            // Add required parameters
            controller.AddParameter("LocomotionSpeed", AnimatorControllerParameterType.Float);
            controller.AddParameter("LocomotionState", AnimatorControllerParameterType.Int);
            controller.AddParameter("SocialState", AnimatorControllerParameterType.Int);
            controller.AddParameter("PhoneState", AnimatorControllerParameterType.Int);
            controller.AddParameter("SeatingState", AnimatorControllerParameterType.Int);
            controller.AddParameter("Trigger", AnimatorControllerParameterType.Trigger);

            AssetDatabase.SaveAssets();
            Debug.Log("[NAAD][GateC] Added 6 parameters to PlayerController");
            EditorApplication.Exit(0);
        }
    }
}
