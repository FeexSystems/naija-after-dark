using UnityEngine;

public class RelationshipCondition : DialogueCondition
{
    [SerializeField] protected string requiredRelationship = "";
    [SerializeField] protected int minimumRelationshipRank = 0;

    public override bool CheckCondition(EnhancedDialogueConfig config, int playerReputation, List<string> pastIds = null)
    {
        if (!string.IsNullOrEmpty(requiredRelationship))
        {
            // Placeholder: check if relationship meets threshold
            // In full implementation, would check against relationship manager
            return true;
        }
        return true;
    }
}