using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; protected set; }

    [SerializeField] protected EnhancedDialogueConfig currentConfig;
    [SerializeField] protected int currentLineIndex = 0;
    [SerializeField] protected List<string> seenDialogueIds = new List<string>();
    [SerializeField] protected int playerReputation = 0;
    [SerializeField] protected bool useLLMGeneration = false;
    [SerializeField] protected string currentNpcId = "";

    protected List<DialogueLine> currentLines;
    protected List<DialogueResponse> currentResponses;

    private AI.RagContextRetriever ragRetriever;
    private AI.UnifiedAIClient aiClient;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        ragRetriever = AI.RagContextRetriever.Instance;
        aiClient = AI.UnifiedAIClient.Instance;
    }

    public void Initialize(string dialogueId, int playerReputation, string npcId = "")
    {
        currentNpcId = npcId;
        var config = Resources.Load<EnhancedDialogueConfig>(dialogueId);
        if (config != null && config.CanActivate(playerReputation, seenDialogueIds))
        {
            this.currentConfig = config;
            this.playerReputation = playerReputation;
            currentLines = config.DialogueLines;
            currentResponses = config.GetAvailableResponses(playerReputation, seenDialogueIds);
            currentLineIndex = 0;
            seenDialogueIds.Add(dialogueId); // Mark as seen
        }
        else
        {
            Debug.LogWarning($"Dialogue {dialogueId} cannot activate");
        }
    }

    public DialogueLine GetCurrentLine()
    {
        if (currentLines != null && currentLineIndex < currentLines.Count)
        {
            return currentLines[currentLineIndex];
        }
        return null;
    }

    public List<DialogueResponse> GetCurrentResponses()
    {
        return currentResponses ?? new List<DialogueResponse>();
    }

    public void SelectResponse(int responseIndex)
    {
        if (currentResponses != null && responseIndex >= 0 && responseIndex < currentResponses.Count)
        {
            var selected = currentResponses[responseIndex];

            // Apply reputation/relationship changes
            playerReputation += selected.ReputationChange;

            // Move to next dialogue
            if (!string.IsNullOrEmpty(selected.NextDialogueId))
            {
                // Reset seen tracking for new dialogue
                seenDialogueIds.Clear();
                Initialize(selected.NextDialogueId, playerReputation, currentNpcId);
            }
            else
            {
                // Current dialogue complete
                currentLineIndex = currentLines.Count; // End of dialogue
            }
        }
    }

    public bool IsActive() => currentConfig != null && currentLineIndex < currentLines.Count;

    /// <summary>
    /// Generate LLM-powered dialogue response
    /// </summary>
    public async Task<DialogueLine> GenerateLLMResponseAsync(string playerMessage)
    {
        if (!useLLMGeneration || aiClient == null)
        {
            return null;
        }

        // Retrieve RAG context
        string context = "";
        if (ragRetriever != null && !string.IsNullOrEmpty(currentNpcId))
        {
            var facts = ragRetriever.RetrieveContext(currentNpcId, playerMessage);
            context = ragRetriever.FormatContextForPrompt(facts);
        }

        // Build request
        AI.DialogueRequest request = new AI.DialogueRequest
        {
            npcId = currentNpcId,
            playerMessage = playerMessage,
            context = context,
            previousMessages = GetPreviousMessages(),
            reputation = playerReputation,
            relationship = "neutral" // TODO: Track relationship state
        };

        // Generate response
        AI.DialogueResponse aiResponse = await aiClient.GenerateDialogueAsync(request);

        if (aiResponse != null && !string.IsNullOrEmpty(aiResponse.text))
        {
            return new DialogueLine
            {
                Text = aiResponse.text,
                Emotion = aiResponse.emotion
            };
        }

        return null;
    }

    /// <summary>
    /// Get previous messages for context
    /// </summary>
    private string[] GetPreviousMessages()
    {
        List<string> messages = new List<string>();

        if (currentLines != null)
        {
            for (int i = 0; i < currentLineIndex && i < currentLines.Count; i++)
            {
                messages.Add(currentLines[i].Text);
            }
        }

        return messages.ToArray();
    }

    /// <summary>
    /// Play ambient line (non-interactive)
    /// </summary>
    public void PlayAmbientLine(string line)
    {
        // TODO: Implement ambient dialogue playback
        Debug.Log($"Ambient line: {line}");
    }

    /// <summary>
    /// Enable or disable LLM generation
    /// </summary>
    public void SetLLMGenerationEnabled(bool enabled)
    {
        useLLMGeneration = enabled;
    }
}