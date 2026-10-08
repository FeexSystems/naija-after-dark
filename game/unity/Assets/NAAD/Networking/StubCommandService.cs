using System;
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
            _log.Info("Command", $"Stub {command.Type} requestId={command.RequestId}");
            command.Payload.TryGetValue("toLocationId", out var to);

            return Task.FromResult(new CommandResult
            {
                RequestId = command.RequestId,
                Success = true,
                Payload = new Dictionary<string, string>
                {
                    ["fromLocationId"] = "a1111111-1111-1111-1111-111111111101",
                    ["toLocationId"] = to?.ToString() ?? "a1111111-1111-1111-1111-111111111102"
                }
            });
        }
    }
}
