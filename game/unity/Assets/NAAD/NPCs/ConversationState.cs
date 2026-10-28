using UnityEngine;
using NAAD.Gameplay.ContentExpansion;
using NAAD.AI;

[CreateAssetMenu(fileName = "ConversationState", menuName = "NAAD/Behavior/Conversation")]
public class ConversationState : BehaviorState
{
    [Header("Conversation State Settings")]
    [Tooltip("Dialogue bubble prefab to use")]
    [SerializeField] protected DialogueBubble dialogueBubblePrefab;

    [Tooltip("Override dialogue for this state")]
    [SerializeField] protected DialogueConfig overrideDialogue;

    public override void Enter(BehaviorStateMachine stateMachine)
    {
        base.Enter(stateMachine);
        Debug.Log($"[BehaviorState] {stateName}: Initiating conversation.");

        if (dialogueBubblePrefab != null)
        {
            var bubble = Instantiate(dialogueBubblePrefab, stateMachine.transform.position + Vector3.up, Quaternion.identity);
            // bubble?.InitializeForNPCConversation(this); // TODO: Type mismatch
        }
    }

    public override void UpdateState(BehaviorStateMachine stateMachine)
    {
        base.UpdateState(stateMachine);
    }

    public override void Exit(BehaviorStateMachine stateMachine)
    {
        base.Exit(stateMachine);
        Debug.Log($"[BehaviorState] {stateName}: Conversation ended.");
    }
}