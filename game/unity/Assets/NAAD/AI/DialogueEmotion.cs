namespace NAAD.AI
{
    /// <summary>
    /// Emotion tag returned with AI dialogue responses.
    /// <para>
    /// Only <see cref="Neutral"/> is referenced by current code. The full emotion
    /// taxonomy is pending design; values are added here as the dialogue system
    /// adopts them (never inferred at call sites).
    /// </para>
    /// </summary>
    public enum DialogueEmotion
    {
        Neutral = 0
    }
}