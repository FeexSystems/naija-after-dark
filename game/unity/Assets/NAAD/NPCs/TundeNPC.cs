using UnityEngine;

public class TundeNPC : MonoBehaviour
{
    [Header("Tunde NPC Settings")]
    [SerializeField] protected BehaviorStateMachine behaviorStateMachine;
    [SerializeField] protected DialogueBubble dialogueBubblePrefab;
    
    protected virtual void Awake()
    {
        behaviorStateMachine = behaviorStateMachine ?? GetComponent<BehaviorStateMachine>();
        if (behaviorStateMachine == null)
        {
            behaviorStateMachine = gameObject.AddComponent<BehaviorStateMachine>();
        }
        SetupStates();
    }

    protected virtual void SetupStates() { }

    protected virtual void Update()
    {
        behaviorStateMachine?.UpdateState(this);
    }

    public void InteractWithPlayer()
    {
        Debug.Log($"[TundeNPC] Starting conversation.");
        behaviorStateMachine?.RequestStateTransition("Conversation");
        if (dialogueBubblePrefab != null)
        {
            var bubble = Instantiate(dialogueBubblePrefab, transform.position + Vector3.up, Quaternion.identity);
            bubble?.InitializeForNPCConversation(this);
        }
    }

    public bool CanInteract() => !behaviorStateMachine.IsInState("Conversation");
}