using System.Collections.Generic;
using System.Threading.Tasks;

namespace NAAD.Networking
{
    public enum GameCommandType
    {
        Travel,
        Spend,
        Buy,
        StartNight,
        CompleteNight,
        PhoneReply,
        JoinRoom,
        LeaveRoom,
        AttendEvent,
        RelationshipDelta,
        SetCareer,
        AcceptOpportunity,
        CommitMemory,
        SetWorldPeriod
    }

    public sealed class GameCommand
    {
        public string RequestId = string.Empty;
        public string PlayerId = string.Empty;
        public GameCommandType Type;
        public Dictionary<string, object> Payload = new();
        public string ClientTimestamp = string.Empty;
    }

    public sealed class CommandResult
    {
        public string RequestId = string.Empty;
        public bool Success;
        public string? ErrorCode;
        public string? ErrorMessage;
        public Dictionary<string, string>? Payload;
    }

    public interface ICommandService
    {
        Task<CommandResult> ExecuteAsync(GameCommand command);
    }

    public static class GameCommandTypeExtensions
    {
        public static string ToApiString(this GameCommandType type) => type switch
        {
            GameCommandType.Travel => "TRAVEL",
            GameCommandType.Spend => "SPEND",
            GameCommandType.Buy => "BUY",
            GameCommandType.StartNight => "START_NIGHT",
            GameCommandType.CompleteNight => "COMPLETE_NIGHT",
            GameCommandType.PhoneReply => "PHONE_REPLY",
            GameCommandType.JoinRoom => "JOIN_ROOM",
            GameCommandType.LeaveRoom => "LEAVE_ROOM",
            GameCommandType.AttendEvent => "ATTEND_EVENT",
            GameCommandType.RelationshipDelta => "RELATIONSHIP_DELTA",
            GameCommandType.SetCareer => "SET_CAREER",
            GameCommandType.AcceptOpportunity => "ACCEPT_OPPORTUNITY",
            GameCommandType.CommitMemory => "COMMIT_MEMORY",
            GameCommandType.SetWorldPeriod => "SET_WORLD_PERIOD",
            _ => type.ToString().ToUpperInvariant()
        };
    }
}
