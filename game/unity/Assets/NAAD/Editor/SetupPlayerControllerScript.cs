using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class SetupPlayerControllerScript
{
    public static void Run()
    {
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/NAAD/Animation/Controllers/PlayerController.controller");
        if (controller == null)
        {
            Debug.LogError("PlayerController not found");
            return;
        }

        controller.AddParameter("LocomotionSpeed", AnimatorControllerParameterType.Float);
        controller.AddParameter("LocomotionState", AnimatorControllerParameterType.Int);
        controller.AddParameter("SocialState", AnimatorControllerParameterType.Int);
        controller.AddParameter("PhoneState", AnimatorControllerParameterType.Int);
        controller.AddParameter("SeatingState", AnimatorControllerParameterType.Int);
        controller.AddParameter("Trigger", AnimatorControllerParameterType.Trigger);

        AssetDatabase.SaveAssets();
        Debug.Log("Added 6 parameters to PlayerController");
    }
}
