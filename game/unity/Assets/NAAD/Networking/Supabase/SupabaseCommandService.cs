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
            var success = NaadJson.ReadSuccess(json);

            NaadJson.TryGetString(json, "requestId", out var requestId);
            NaadJson.TryGetString(json, "errorCode", out var errorCode);
            NaadJson.TryGetString(json, "errorMessage", out var errorMessage);

            // Payload may be nested under "payload" (router envelope) or flat
            // (legacy direct RPCs). Prefer the nested object when present.
            string? scope = json;
            if (NaadJson.TryGetObject(json, "payload", out var nested) && !string.IsNullOrEmpty(nested))
                scope = nested;

            var payload = new Dictionary<string, string>();
            foreach (var key in new[]
                     {
                         "fromLocationId", "toLocationId", "sku", "amount", "balanceAfter",
                         "sessionId", "messageId", "action", "roomId", "locationId",
                         "eventId", "careerId", "opportunityId", "targetId",
                         "spentNgn", "transactions", "peopleMet", "trustWithTunde",
                         "bestMoment", "newConnection"
                     })
            {
                if (scope != null && NaadJson.TryGetString(scope, key, out var v) && v != null)
                {
                    payload[key] = v;
                    continue;
                }
                if (scope != null && NaadJson.TryGetLong(scope, key, out var n))
                    payload[key] = n.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }

            return new CommandResult
            {
                RequestId = requestId ?? fallbackRequestId,
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
    }
}
