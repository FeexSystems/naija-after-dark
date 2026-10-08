using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using NAAD.Core.Logging;
using NAAD.Networking.Config;

namespace NAAD.Networking.Supabase
{
    /// <summary>
    /// Phase A: all mutations via naad_execute_command.
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

            var token = await _auth.GetAccessTokenAsync();
            if (string.IsNullOrEmpty(token))
                return Fail(command.RequestId, "UNAUTHENTICATED", "Not signed in");

            var type = command.Type.ToApiString();
            var payloadJson = BuildPayloadJson(command.Payload);
            var body =
                "{\"p_request_id\":\"" + Escape(command.RequestId) + "\",\"p_type\":\"" + Escape(type) + "\",\"p_payload\":" + payloadJson + "}";

            var url = _config.RestBaseUrl + "/rpc/naad_execute_command";
            _log.Info("Command", type + " requestId=" + command.RequestId);

            var result = await _http.SendAsync(
                url,
                UnityEngine.Networking.UnityWebRequest.kHttpVerbPOST,
                body,
                token);

            if (!result.Ok)
            {
                _log.Error("Command", "RPC failed: " + result.StatusCode + " " + result.Body);
                return Fail(command.RequestId, "RPC_ERROR", result.Body);
            }

            return ParseResult(result.Body, command.RequestId);
        }

        private static string BuildPayloadJson(Dictionary<string, object> payload)
        {
            if (payload == null || payload.Count == 0)
                return "{}";

            var sb = new StringBuilder();
            sb.Append('{');
            var first = true;
            foreach (var kv in payload)
            {
                if (!first) sb.Append(',');
                first = false;
                sb.Append('"').Append(Escape(kv.Key)).Append("\":");
                sb.Append(ValueToJson(kv.Value));
            }
            sb.Append('}');
            return sb.ToString();
        }

        private static string ValueToJson(object value)
        {
            if (value == null) return "null";
            if (value is bool b) return b ? "true" : "false";
            if (value is int || value is long || value is float || value is double || value is decimal)
                return Convert.ToString(value, CultureInfo.InvariantCulture) ?? "0";
            return "\"" + Escape(value.ToString() ?? "") + "\"";
        }

        private static CommandResult ParseResult(string json, string fallbackRequestId)
        {
            var success = json.IndexOf("\"success\":true", StringComparison.OrdinalIgnoreCase) >= 0
                          || json.IndexOf("\"success\": true", StringComparison.OrdinalIgnoreCase) >= 0;

            var requestId = ExtractString(json, "requestId") ?? fallbackRequestId;
            var errorCode = ExtractString(json, "errorCode");
            var errorMessage = ExtractString(json, "errorMessage");

            var payload = new Dictionary<string, string>();
            foreach (var key in new[]
                     {
                         "fromLocationId", "toLocationId", "sku", "amount", "balanceAfter",
                         "sessionId", "messageId", "action", "roomId", "locationId",
                         "eventId", "careerId", "opportunityId", "targetId",
                         "spentNgn", "bestMoment", "newConnection"
                     })
            {
                var v = ExtractString(json, key);
                if (v != null) payload[key] = v;
                else
                {
                    var n = ExtractNumberString(json, key);
                    if (n != null) payload[key] = n;
                }
            }

            return new CommandResult
            {
                RequestId = requestId,
                Success = success,
                ErrorCode = errorCode,
                ErrorMessage = errorMessage,
                Payload = payload.Count > 0 ? payload : null
            };
        }

        private static CommandResult Fail(string requestId, string code, string message) =>
            new CommandResult
            {
                RequestId = requestId,
                Success = false,
                ErrorCode = code,
                ErrorMessage = message
            };

        private static string Escape(string s) =>
            s.Replace("\\", "\\\\").Replace("\"", "\\\"");

        private static string ExtractString(string json, string key)
        {
            var pattern = "\"" + key + "\"";
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

        private static string ExtractNumberString(string json, string key)
        {
            var pattern = "\"" + key + "\"";
            var idx = json.IndexOf(pattern, StringComparison.Ordinal);
            if (idx < 0) return null;
            idx = json.IndexOf(':', idx);
            if (idx < 0) return null;
            var rest = json.Substring(idx + 1).TrimStart();
            var end = 0;
            while (end < rest.Length && (char.IsDigit(rest[end]) || rest[end] == '-' || rest[end] == '.'))
                end++;
            if (end == 0) return null;
            return rest.Substring(0, end);
        }
    }
}
