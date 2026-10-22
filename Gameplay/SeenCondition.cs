using UnityEngine;
using System.Collections.Generic;

public class SeenCondition : DialogueCondition
{
    public override bool CheckCondition(EnhancedDialogueConfig config, int playerReputation, List<string> pastIds = null)
    {
        if (pastIds == null || pastIds.Contains(config.DialogueId))
        {
            return false; // Already seen
        }
        return true; // Not seen yet
    }
}