namespace NAAD.AI
{
    /// <summary>
    /// Emotion tag returned with AI dialogue responses.
    /// <para>
    /// Expanded emotion taxonomy for dialogue system. Maps to Edge Function schema.
    /// </para>
    /// </summary>
    public enum DialogueEmotion
    {
        Neutral = 0,
        Happy = 1,
        Angry = 2,
        Sad = 3,
        Surprised = 4,
        Skeptical = 5,
        Flirty = 6
    }
}