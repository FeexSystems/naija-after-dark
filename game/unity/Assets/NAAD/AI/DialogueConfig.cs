using System.Collections.Generic;
using UnityEngine;

namespace NAAD.AI
{
    /// <summary>
    /// Authoritative dialogue configuration for an NPC conversation.
    /// <para>
    /// Defines the dialogue lines, branching responses, and optional memory
    /// commits that an NPC can present during a conversation state.
    /// </para>
    /// </summary>
    [CreateAssetMenu(fileName = "DialogueConfig", menuName = "NAAD/Dialogue Config", order = 110)]
    public sealed class DialogueConfig : ScriptableObject
    {
        [Header("NPC Identity")]
        [Tooltip("The NPC entity ID this dialogue config applies to.")]
        public string npcId;

        [Tooltip("Display name of the NPC in dialogue UI.")]
        public string npcName = "NPC";

        [Header("Dialogue Lines")]
        [Tooltip("Sequential dialogue lines for this NPC, presented in order.")]
        public List<string> dialogueLines = new List<string>();

        [Header("Player Responses")]
        [Tooltip("Available player response options for branching dialogue.")]
        public List<PlayerResponseOption> responses = new List<PlayerResponseOption>();

        [Header("Memory Commit")]
        [Tooltip("If set, commits a memory of this type when the dialogue completes.")]
        public string memoryType = "";

        [Tooltip("Summary text for the committed memory.")]
        public string memorySummary = "";

        [Tooltip("Importance score (0–1) for the committed memory.")]
        [Range(0f, 1f)]
        public float memoryImportance = 0.5f;
    }

    /// <summary>
    /// A single player response option in a branched dialogue.
    /// </summary>
    [System.Serializable]
    public sealed class PlayerResponseOption
    {
        [Tooltip("The text displayed to the player for this response.")]
        public string text = "";

        [Tooltip("The action or next-state this response triggers.")]
        public string action = "";
    }
}