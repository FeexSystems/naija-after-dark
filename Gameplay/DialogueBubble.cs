using UnityEngine;

[CreateAssetMenu(menu = "NAAD/Dialogue/Bubble")]
public class DialogueBubble : ScriptableObject
{
    [SerializeField] protected string text = "Dialogue text";
    [SerializeField] protected float displayTime = 5f;
    [SerializeField] protected DialogueEmotion emotion = DialogueEmotion.Neutral;
    [SerializeField] protected Color textColor = Color.white;

    private bool isScreenSpaceMode = false;

    public string Text => text;
    public float DisplayTime => displayTime;
    public DialogueEmotion Emotion => emotion;
    public Color TextColor => textColor;
    public bool IsScreenSpaceMode => isScreenSpaceMode;

    public void Initialize(DialogueLine line)
    {
        text = line.Text;
        // Could also copy other properties from line
    }

    public void SetScreenSpaceMode(bool screenSpace)
    {
        isScreenSpaceMode = screenSpace;
    }
}

public enum DialogueEmotion { Neutral, Happy, Sad, Angry, Surprised, Fearful, Thoughtful }