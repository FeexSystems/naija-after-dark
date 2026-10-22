using UnityEngine;

[CreateAssetMenu(menu = "NAAD/Dialogue/Response")]
public class DialogueResponse : ScriptableObject
{
    [SerializeField] protected string responseText = "Response";
    [SerializeField] protected DialogueCondition selectionCondition;
    [SerializeField] protected string nextDialogueId;
    [SerializeField] protected int reputationChange = 0;
    [SerializeField] protected int relationshipChange = 0;
    [SerializeField] protected bool unlocksTask = false;
    [SerializeField] protected string taskId = "";

    public string ResponseText => responseText;
    public DialogueCondition SelectionCondition => selectionCondition;
    public string NextDialogueId => nextDialogueId;
    public int ReputationChange => reputationChange;
    public int RelationshipChange => relationshipChange;
    public bool UnlocksTask => unlocksTask;
    public string TaskId => taskId;

    public bool CanSelect(int playerReputation, List<string> pastIds = null)
    {
        if (selectionCondition != null)
        {
            return selectionCondition.CheckCondition(this, playerReputation, pastIds);
        }
        return true;
    }
}