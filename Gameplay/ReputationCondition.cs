using UnityEngine;

public class ReputationCondition : DialogueCondition
{
    [SerializeField] protected int minimumReputation = 0;
    [SerializeField] protected bool requireExactReputation = false;
    [SerializeField] protected string requiredFaction = "";

    public override bool CheckCondition(EnhancedDialogueConfig config, int playerReputation, List<string> pastIds = null)
    {
        if (requireExactReputation)
        {
            return playerReputation >= minimumReputation;
        }
        return playerReputation >= minimumReputation;
    }
}