using UnityEngine;

[CreateAssetMenu(fileName = "ConversationState", menuName = "NAAD/Behavior/Conversation")]
public class ConversationState : BehaviorState
{
    [Header("Conversation State Settings")]
    [Tooltip("Dialogue bubble prefab to use")]
    [SerializeField] protected DialogueBubble dialogueBubblePrefab;

    public override void Enter(BehaviorStateMachine stateMachine)
    {
        base.Enter(stateMachine);
        Debug.Log($"[BehaviorState] {stateName}: Initiating conversation.");

        if (dialogueBubblePrefab != null)
        {
            var bubble = Instantiate(dialogueBubblePrefab, stateMachine.transform.position + Vector3.up, Quaternion.identity);
            bubble?.InitializeForNPCConversation(this);
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