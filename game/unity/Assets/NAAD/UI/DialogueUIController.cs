using NAAD.Networking;
using NAAD.Networking.Supabase;
using UnityEngine;
using TMPro;

/// <summary>
/// Controller that manages the dialogue UI bubble and connects to the Supabase dialogue service.
/// </summary>
public class DialogueUIController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The dialogue bubble prefab to display")]
    public DialogueBubble dialogueBubble;

    [Tooltip("The Supabase dialogue service")]
    public SupabaseDialogueService dialogueService;

    [Tooltip("Canvas reference for UI scaling")]
    public Canvas uiCanvas;

    [Header("Settings")]
    [Tooltip("Whether to auto-display dialogue when received")]
    public bool autoDisplay = true;

    // Current active dialogue result
    private DialogueResult currentDialogue;

    private void Awake()
    {
        if (dialogueBubble == null)
        {
            Debug.LogError("DialogueBubble reference not set on DialogueUIController!");
        }

        if (dialogueService == null)
        {
            Debug.LogError("DialogueService reference not set on DialogueUIController!");
        }
    }

    private void OnEnable()
    {
        // We subscribe to dialogue events here, but the actual integration
        // would depend on the game's event system
    }

    /// <summary>
    /// Initiate a dialogue with Tunde NPC.
    /// </summary>
    /// <param name="playerMessage">The message from the player</param>
    public async void InitiateDialogue(string playerMessage)
    {
        if (dialogueService == null) return;

        var result = await dialogueService.TalkToTundeAsync(playerMessage, System.Guid.NewGuid().ToString());

        if (result != null)
        {
            currentDialogue = result;

            if (autoDisplay && dialogueBubble != null)
            {
                DisplayDialogue(result);
            }
        }
        else
        {
            Debug.LogWarning("Dialogue failed - check Edge Function deployment");
        }
    }

    /// <summary>
    /// Display the dialogue result in the UI bubble.
    /// </summary>
    /// <param name="result">The dialogue result from the service</param>
    private void DisplayDialogue(DialogueResult result)
    {
        if (dialogueBubble == null) return;

        // Set the emotion-based text color or styling
        Color emotionColor = GetEmotionColor(result.Emotion);
        dialogueBubble.textComponent.color = emotionColor;

        // Display the dialogue text
        dialogueBubble.Show(result.Dialogue);

        // Log the dialogue details
        Debug.Log($"Dialogue displayed: [{result.Emotion}] {result.Dialogue}");
    }

    /// <summary>
    /// Gets a color based on the emotion type.
    /// </summary>
    /// <param name="emotion">The emotion type</param>
    /// <returns>Color representing the emotion</returns>
    private Color GetEmotionColor(string emotion)
    {
        switch (emotion.ToLower())
        {
            case "happy":
                return Color.yellow;
            case "excited":
                return Color.cyan;
            case "angry":
                return Color.red;
            case "sad":
                return Color.blue;
            case "nervous":
                return Color.magenta;
            case "neutral":
            default:
                return Color.white;
        }
    }
}