using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NAAD.Core.Logging;
using NAAD.Networking.Config;

namespace NAAD.Networking.Supabase
{
    /// <summary>
    /// Sends validated commands to Postgres RPCs via PostgREST.
    /// Never mutates local authoritative state optimistically for money/location.
    /// </summary>
    public sealed class SupabaseCommandService : ICommandService
    {
        private readonly SupabaseConfig _config;
        private readonly IAuthService _auth;
        private readonly SupabaseHttp _http;
        private readonly INAADLogger _log;

        public SupabaseCommandService(SupabaseConfig config, IAuthService auth, INAADLogger log)
        {
            _config = config;
            _auth = auth;
            _log = log;
            _http = new SupabaseHttp(config, log);
        }

        public async Task<CommandResult> ExecuteAsync(GameCommand command)
        {
            if (string.IsNullOrWhiteSpace(command.RequestId))
                command.RequestId = Guid.NewGuid().ToString("N");

            if (command.Type != GameCommandType.Travel)
            {
                return Fail(command.RequestId, "UNSUPPORTED", $"Command {command.Type} not implemented in Gate 5");
            }

            if (!command.Payload.TryGetValue("toLocationId", out var toObj) || toObj == null)
                return Fail(command.RequestId, "INVALID_PAYLOAD", "toLocationId required");

            var toLocationId = toObj.ToString() ?? "";
            var token = await _auth.GetAccessTokenAsync();
            if (string.IsNullOrEmpty(token))
                return Fail(command.RequestId, "UNAUTHENTICATED", "Not signed in");

            var body =
                $"{{\"p_request_id\":\"{Escape(command.RequestId)}\",\"p_to_location_id\":\"{Escape(toLocationId)}\"}}";

            var url = $"{_config.RestBaseUrl}/rpc/naad_travel";
            _log.Info("Command", $"TRAVEL requestId={command.RequestId} to={toLocationId}");

            var result = await _http.SendAsync(
                url,
                UnityEngine.Networking.UnityWebRequest.kHttpVerbPOST,
                body,
                token,
                ("Prefer", "return=representation"));

            if (!result.Ok)
            {
                _log.Error("Command", $"RPC failed: {result.StatusCode} {result.Body}");
                return Fail(command.RequestId, "RPC_ERROR", result.Body);
            }

            return ParseResult(result.Body, command.RequestId);
        }

        private static CommandResult ParseResult(string json, string fallbackRequestId)
        {
            var success = json.IndexOf("\"success\":true", StringComparison.OrdinalIgnoreCase) >= 0
                          || json.IndexOf("\"success\": true", StringComparison.OrdinalIgnoreCase) >= 0;

            var requestId = ExtractString(json, "requestId") ?? fallbackRequestId;
            var errorCode = ExtractString(json, "errorCode");
            var errorMessage = ExtractString(json, "errorMessage");

            var payload = new Dictionary<string, string>();
            var fromId = ExtractString(json, "fromLocationId");
            var toId = ExtractString(json, "toLocationId");
            if (!string.IsNullOrEmpty(fromId)) payload["fromLocationId"] = fromId;
            if (!string.IsNullOrEmpty(toId)) payload["toLocationId"] = toId;

            return new CommandResult
            {
                RequestId = requestId,
                Success = success,
                ErrorCode = errorCode,
                ErrorMessage = errorMessage,
                Payload = payload
            };
        }

        private static CommandResult Fail(string requestId, string code, string message) =>
            new()
            {
                RequestId = requestId,
                Success = false,
                ErrorCode = code,
                ErrorMessage = message
            };

        private static string Escape(string s) =>
            s.Replace("\\", "\\\\").Replace("\"", "\\\"");

        private static string? ExtractString(string json, string key)
        {
            var pattern = $"\"{key}\"";
            var idx = json.IndexOf(pattern, StringComparison.Ordinal);
            if (idx < 0) return null;
            idx = json.IndexOf(':', idx);
            if (idx < 0) return null;
            var rest = json.Substring(idx + 1).TrimStart();
            if (rest.StartsWith("null", StringComparison.Ordinal)) return null;
            if (!rest.StartsWith("\"")) return null;
            var end = rest.IndexOf('"', 1);
            if (end < 0) return null;
            return rest.Substring(1, end - 1);
        }
    }
}
