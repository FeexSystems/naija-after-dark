using System;
using System.Threading.Tasks;
using NAAD.Core.Logging;
using NAAD.Networking.Config;

namespace NAAD.Networking.Supabase
{
    public sealed class SupabasePhoneService : IPhoneService
    {
        private readonly SupabaseConfig _config;
        private readonly IAuthService _auth;
        private readonly SupabaseHttp _http;
        private readonly INAADLogger _log;

        public SupabasePhoneService(SupabaseConfig config, IAuthService auth, INAADLogger log)
        {
            _config = config;
            _auth = auth;
            _log = log;
            _http = new SupabaseHttp(config, log);
        }

        public async Task<bool> SeedTundeInviteAsync()
        {
            var token = await _auth.GetAccessTokenAsync();
            if (string.IsNullOrEmpty(token)) return false;
            var r = await _http.SendAsync($"{_config.RestBaseUrl}/rpc/naad_seed_tunde_beach_invite", "POST", "{}", token);
            return r.Ok && r.Body.Contains("\"success\":true");
        }

        public async Task<string?> ListMessagesJsonAsync()
        {
            var token = await _auth.GetAccessTokenAsync();
            if (string.IsNullOrEmpty(token)) return null;
            var r = await _http.SendAsync($"{_config.RestBaseUrl}/rpc/naad_list_messages", "POST", "{}", token);
            return r.Ok ? r.Body : null;
        }

        public async Task<bool> RespondAsync(string requestId, string messageId, string action)
        {
            var token = await _auth.GetAccessTokenAsync();
            if (string.IsNullOrEmpty(token)) return false;
            var body =
                $"{{\"p_request_id\":\"{requestId}\",\"p_message_id\":\"{messageId}\",\"p_action\":\"{action}\"}}";
            var r = await _http.SendAsync($"{_config.RestBaseUrl}/rpc/naad_respond_message", "POST", body, token);
            return r.Ok && r.Body.Contains("\"success\":true");
        }

        public async Task<bool> StartNightAsync(string requestId)
        {
            var token = await _auth.GetAccessTokenAsync();
            if (string.IsNullOrEmpty(token)) return false;
            var body = $"{{\"p_request_id\":\"{requestId}\"}}";
            var r = await _http.SendAsync($"{_config.RestBaseUrl}/rpc/naad_start_night", "POST", body, token);
            _log.Info("Night", r.Body);
            return r.Ok && r.Body.Contains("\"success\":true");
        }

        public async Task<NightSummaryDto?> CompleteNightAsync(string requestId)
        {
            var token = await _auth.GetAccessTokenAsync();
            if (string.IsNullOrEmpty(token)) return null;
            var body = $"{{\"p_request_id\":\"{requestId}\"}}";
            var r = await _http.SendAsync($"{_config.RestBaseUrl}/rpc/naad_complete_night", "POST", body, token);
            if (!r.Ok) return null;
            return new NightSummaryDto
            {
                Success = r.Body.Contains("\"success\":true"),
                SpentNgn = ExtractInt(r.Body, "spentNgn") ?? 0,
                Transactions = ExtractInt(r.Body, "transactions") ?? 0,
                PeopleMet = ExtractInt(r.Body, "peopleMet") ?? 0,
                NewConnection = ExtractString(r.Body, "newConnection"),
                TrustWithTunde = ExtractInt(r.Body, "trustWithTunde"),
                BestMoment = ExtractString(r.Body, "bestMoment") ?? ""
            };
        }

        public async Task<int?> GetBalanceAsync()
        {
            var token = await _auth.GetAccessTokenAsync();
            if (string.IsNullOrEmpty(token)) return null;
            var r = await _http.SendAsync($"{_config.RestBaseUrl}/rpc/naad_get_wallet", "POST", "{}", token);
            return ExtractInt(r.Body, "balance");
        }

        private static string? ExtractString(string json, string key)
        {
            var pattern = $"\"{key}\"";
            var idx = json.IndexOf(pattern, StringComparison.Ordinal);
            if (idx < 0) return null;
            idx = json.IndexOf(':', idx);
            if (idx < 0) return null;
            var rest = json.Substring(idx + 1).TrimStart();
            if (rest.StartsWith("null")) return null;
            if (!rest.StartsWith("\"")) return null;
            var end = rest.IndexOf('"', 1);
            return end < 0 ? null : rest.Substring(1, end - 1);
        }

        private static int? ExtractInt(string json, string key)
        {
            var pattern = $"\"{key}\"";
            var idx = json.IndexOf(pattern, StringComparison.Ordinal);
            if (idx < 0) return null;
            idx = json.IndexOf(':', idx);
            if (idx < 0) return null;
            var rest = json.Substring(idx + 1).TrimStart();
            var end = 0;
            while (end < rest.Length && (char.IsDigit(rest[end]) || rest[end] == '-')) end++;
            if (end == 0) return null;
            return int.TryParse(rest.Substring(0, end), out var n) ? n : null;
        }
    }
}
