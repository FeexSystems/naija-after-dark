using System.Threading.Tasks;
using NAAD.Core.Logging;
using NAAD.Networking.Config;

namespace NAAD.Networking.Supabase
{
    /// <summary>
    /// Loads authoritative player row via PostgREST using user JWT.
    /// </summary>
    public sealed class SupabasePlayerService : IPlayerService
    {
        private readonly SupabaseConfig _config;
        private readonly IAuthService _auth;
        private readonly SupabaseHttp _http;
        private readonly INAADLogger _log;

        public SupabasePlayerService(SupabaseConfig config, IAuthService auth, INAADLogger log)
        {
            _config = config;
            _auth = auth;
            _log = log;
            _http = new SupabaseHttp(config, log);
        }

        public async Task<PlayerDto?> FetchCurrentPlayerAsync()
        {
            var token = await _auth.GetAccessTokenAsync();
            var playerId = await _auth.GetPlayerIdAsync();
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(playerId))
            {
                _log.Warn("Player", "Not authenticated");
                return null;
            }

            var url = $"{_config.RestBaseUrl}/players?id=eq.{playerId}&select=id,display_name,created_at,updated_at";
            var result = await _http.SendAsync(
                url,
                UnityEngine.Networking.UnityWebRequest.kHttpVerbGET,
                null,
                token,
                ("Accept", "application/json"));

            if (!result.Ok)
            {
                _log.Error("Player", $"Fetch failed: {result.StatusCode} {result.Body}");
                return null;
            }

            // Response is a JSON array
            var body = result.Body.Trim();
            if (body == "[]" || body.Length < 3)
            {
                _log.Warn("Player", "No player row yet (trigger may still be running)");
                return null;
            }

            var id = ExtractField(body, "id") ?? playerId;
            var name = ExtractField(body, "display_name") ?? "Player";
            var created = ExtractField(body, "created_at") ?? "";
            var updated = ExtractField(body, "updated_at") ?? "";

            _log.Info("Player", $"Loaded {name} ({id})");
            return new PlayerDto
            {
                Id = id,
                DisplayName = name,
                CreatedAt = created,
                UpdatedAt = updated
            };
        }

        private static string? ExtractField(string json, string key)
        {
            var pattern = $"\"{key}\"";
            var idx = json.IndexOf(pattern, System.StringComparison.Ordinal);
            if (idx < 0) return null;
            idx = json.IndexOf(':', idx);
            if (idx < 0) return null;
            // value may be string or other — handle quoted strings
            var rest = json.Substring(idx + 1).TrimStart();
            if (rest.StartsWith("\""))
            {
                var end = rest.IndexOf('"', 1);
                if (end < 0) return null;
                return rest.Substring(1, end - 1);
            }
            return null;
        }
    }
}
