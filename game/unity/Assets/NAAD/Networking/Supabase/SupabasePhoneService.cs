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
            return r.Ok && NaadJson.ReadSuccess(r.Body);
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
            return r.Ok && NaadJson.ReadSuccess(r.Body);
        }

        public async Task<bool> StartNightAsync(string requestId)
        {
            var token = await _auth.GetAccessTokenAsync();
            if (string.IsNullOrEmpty(token)) return false;
            var body = $"{{\"p_request_id\":\"{requestId}\"}}";
            var r = await _http.SendAsync($"{_config.RestBaseUrl}/rpc/naad_start_night", "POST", body, token);
            _log.Info("Night", r.Body);
            return r.Ok && NaadJson.ReadSuccess(r.Body);
        }

        public async Task<NightSummaryDto?> CompleteNightAsync(string requestId)
        {
            var token = await _auth.GetAccessTokenAsync();
            if (string.IsNullOrEmpty(token)) return null;
            var body = $"{{\"p_request_id\":\"{requestId}\"}}";
            var r = await _http.SendAsync($"{_config.RestBaseUrl}/rpc/naad_complete_night", "POST", body, token);
            if (!r.Ok) return null;
            NaadJson.TryGetString(r.Body, "newConnection", out var newConnection);
            NaadJson.TryGetString(r.Body, "bestMoment", out var bestMoment);
            var hasSpent = NaadJson.TryGetInt(r.Body, "spentNgn", out var spent);
            var hasTx = NaadJson.TryGetInt(r.Body, "transactions", out var tx);
            var hasPeople = NaadJson.TryGetInt(r.Body, "peopleMet", out var people);
            var success = NaadJson.ReadSuccess(r.Body);
            int? trust = NaadJson.TryGetInt(r.Body, "trustWithTunde", out var trustVal) ? trustVal : null;
            if (!success && !(hasSpent && hasTx && hasPeople)) return null;
            return new NightSummaryDto
            {
                Success = success || (hasSpent && hasTx && hasPeople),
                SpentNgn = spent,
                Transactions = tx,
                PeopleMet = people,
                NewConnection = newConnection,
                TrustWithTunde = trust,
                BestMoment = bestMoment ?? ""
            };
        }

        public async Task<int?> GetBalanceAsync()
        {
            var token = await _auth.GetAccessTokenAsync();
            if (string.IsNullOrEmpty(token)) return null;
            var r = await _http.SendAsync($"{_config.RestBaseUrl}/rpc/naad_get_wallet", "POST", "{}", token);
            if (!r.Ok || !NaadJson.ReadSuccess(r.Body)) return null;
            return NaadJson.TryGetInt(r.Body, "balance", out var balance) ? balance : null;
        }
    }
}
