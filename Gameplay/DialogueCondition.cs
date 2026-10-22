using UnityEngine;

[CreateAssetMenu(menu = "NAAD/Dialogue/Condition")]
public class DialogueCondition : ScriptableObject
{
    public virtual bool CheckCondition(EnhancedDialogueConfig config, int playerReputation, List<string> pastIds = null)
    {
        return true; // Default: always true
    }
}