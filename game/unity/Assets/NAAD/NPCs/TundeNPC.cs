using UnityEngine;
using NAAD.Gameplay;
using NAAD.Dialogue;

public class TundeNPC : MonoBehaviour
{
    [Header("Tunde NPC Settings")]
    [SerializeField] protected BehaviorStateMachine behaviorStateMachine;
    [SerializeField] protected DialogueBubble dialogueBubblePrefab;
    [SerializeField] protected DialogueManager dialogueManager;
    [SerializeField] protected string npcId = "tunde";
    [SerializeField] protected string startingDialogueId = "tunde_greeting";

    protected virtual void Awake()
    {
        behaviorStateMachine = behaviorStateMachine ?? GetComponent<BehaviorStateMachine>();
        if (behaviorStateMachine == null)
        {
            behaviorStateMachine = gameObject.AddComponent<BehaviorStateMachine>();
        }

        dialogueManager = dialogueManager ?? FindObjectOfType<DialogueManager>();
        if (dialogueManager == null)
        {
            Debug.LogWarning("[TundeNPC] DialogueManager not found - using legacy bubble instantiation");
        }

        SetupStates();
    }

    protected virtual void SetupStates() { }

    protected virtual void Update()
    {
        // BehaviorStateMachine owns its Unity Update loop.
    }

    public void InteractWithPlayer()
    {
        Debug.Log($"[TundeNPC] Starting conversation with {npcId}.");
        behaviorStateMachine?.RequestStateTransition("Conversation");

        // Use new DialogueManager if available
        if (dialogueManager != null)
        {
            dialogueManager.StartDialogue(startingDialogueId, npcId);
        }
        // Fallback to legacy bubble instantiation
        else if (dialogueBubblePrefab != null)
        {
            var bubble = Instantiate(dialogueBubblePrefab, transform.position + Vector3.up, Quaternion.identity);
            bubble?.InitializeForNPCConversation(this);
        }
    }

    public void EndConversation()
    {
        Debug.Log($"[TundeNPC] Ending conversation.");
        behaviorStateMachine?.RequestStateTransition("Idle");

        if (dialogueManager != null)
        {
            dialogueManager.EndDialogue();
        }
    }

    public bool CanInteract() => !behaviorStateMachine.IsInState("Conversation");

    /// <summary>
    /// Set the starting dialogue ID for this NPC.
    /// </summary>
    public void SetStartingDialogue(string dialogueId)
    {
        startingDialogueId = dialogueId;
    }
}
