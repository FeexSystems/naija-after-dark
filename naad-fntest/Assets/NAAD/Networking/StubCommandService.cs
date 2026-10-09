using System.Collections.Generic;
using System.Threading.Tasks;
using NAAD.Core.Logging;

namespace NAAD.Networking
{
    public sealed class StubCommandService : ICommandService
    {
        private readonly INAADLogger _log;

        public StubCommandService(INAADLogger log)
        {
            _log = log;
        }

        public Task<CommandResult> ExecuteAsync(GameCommand command)
        {
            _log.Info("Command", $"Stub {command.Type.ToApiString()} requestId={command.RequestId}");
            return Task.FromResult(new CommandResult
            {
                RequestId = command.RequestId,
                Success = true,
                Payload = new Dictionary<string, string> { ["stub"] = "true" }
            });
        }
    }
}
