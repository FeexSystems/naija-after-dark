using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Minimal TextMesh Pro dialogue bubble for NPC interactions.
/// Attach to a UI Canvas with a TextMeshPro - Text component.
/// </summary>
[DisallowMultipleComponent]
public class DialogueBubble : MonoBehaviour
{
    [Header("Dialogue Settings")]
    [Tooltip("The text mesh pro text component")]
    public TextMeshProUGUI textComponent;

    [Tooltip("Maximum characters per line before wrapping")]
    public int maxCharsPerLine = 40;

    [Tooltip("Duration in seconds to display the dialogue before auto-hiding")]
    public float displayDuration = 5.0f;

    [Tooltip("Whether the bubble should automatically fade out")]
    public bool autoFade = true;

    [Tooltip("Fade duration in seconds")]
    public float fadeDuration = 1.0f;

    [Header("Bubble Appearance")]
    [Tooltip("The background image of the dialogue bubble")]
    public Graphic backgroundGraphic;

    [Tooltip("The padding inside the bubble")]
    public Vector4 padding = new Vector4(10, 10, 10, 10);

    [Tooltip("The corner rounding radius")]
    public float cornerRadius = 15f;

    [Header("References")]
    [Tooltip("Optional portrait/image displayed next to the dialogue")]
    public Image portraitImage;

    [Tooltip("The maximum height of the dialogue text area")]
    public float maxTextHeight = 100f;

    private Coroutine displayCoroutine;

    /// <summary>
    /// Display a dialogue line with optional auto-hide.
    /// </summary>
    /// <param name="dialogueText">The text to display</param>
    public void Show(string dialogueText)
    {
        if (textComponent != null)
        {
            // Wrap text at maxCharsPerLine
            textComponent.text = WrapText(dialogueText, maxCharsPerLine);
            textComponent.ForceMeshUpdate();

            // Reset coroutine if already running
            if (displayCoroutine != null)
            {
                StopCoroutine(displayCoroutine);
            }

            // Set up auto-hide if enabled
            if (autoFade)
            {
                displayCoroutine = StartCoroutine(FadeAndHide());
            }
        }
    }

    /// <summary>
    /// Initialize the dialogue bubble for an NPC conversation.
    /// </summary>
    /// <param name="npc">The NPC initiating the conversation.</param>
    public void InitializeForNPCConversation(TundeNPC npc)
    {
        // Set up default dialogue state for NPC conversation
        // This can be overridden or extended per-NPC type
        Debug.Log($"[DialogueBubble] Initializing NPC conversation for Unknown");
    }
    /// </summary>
    public void Hide()
    {
        if (textComponent != null)
        {
            textComponent.text = "";
        }
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Fade out and hide the bubble.
    /// </summary>
    private System.Collections.IEnumerator FadeAndHide()
    {
        float elapsed = 0f;
        Color originalColor = textComponent.color;
        Color targetColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            textComponent.color = Color.Lerp(originalColor, targetColor, t);
            yield return null;
        }

        textComponent.color = targetColor;
        textComponent.text = "";
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Wraps text at character boundaries for TMPro.
    /// </summary>
    /// <param name="text">The text to wrap</param>
    /// <param name="maxChars">Maximum characters per line</param>
    /// <returns>Wrapped text with newlines</returns>
    private string WrapText(string text, int maxChars)
    {
        if (string.IsNullOrEmpty(text)) return text;

        var words = text.Split(' ');
        var lines = new System.Text.StringBuilder();
        int currentLineLength = 0;

        for (int i = 0; i < words.Length; i++)
        {
            string word = words[i];
            int wordLength = word.Length;

            // If adding this word would exceed the limit and we already have text on this line
            if (currentLineLength + wordLength > maxChars && currentLineLength > 0)
            {
                lines.Append('\n');
                currentLineLength = 0;
            }

            if (currentLineLength > 0)
            {
                lines.Append(' ');
                currentLineLength++;
            }

            lines.Append(word);
            currentLineLength += wordLength;
        }

        return lines.ToString();
    }
}