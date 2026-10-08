using System.Threading.Tasks;

namespace NAAD.Networking
{
    public sealed class DialogueResult
    {
        public bool Success;
        public string? ErrorCode;
        public string NpcId = "";
        public string NpcName = "";
        public string Dialogue = "";
        public string Emotion = "neutral";
        public string DialogueMode = "DETERMINISTIC";
        public string? MemorySummary;
        public float? MemoryImportance;
        public string? MemoryType;
    }

    public interface IDialogueService
    {
        Task<DialogueResult?> GreetAsync(string npcId);
        Task<DialogueResult?> TalkToTundeAsync(string playerMessage, string requestId);
        Task<bool> CommitMemoryAsync(string requestId, string npcId, string type, string summary, float importance);
    }
}
