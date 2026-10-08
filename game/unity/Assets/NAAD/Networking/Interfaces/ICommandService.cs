using System.Collections.Generic;
using System.Threading.Tasks;

namespace NAAD.Networking
{
    public enum GameCommandType
    {
        Move,
        Interact,
        Talk,
        Travel,
        Buy,
        Sell,
        SendMessage,
        AcceptEvent
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
}
