using System.Threading.Tasks;
using NAAD.Core.Logging;
using NAAD.Networking.Config;

namespace NAAD.Networking.Supabase
{
    public sealed class SupabaseWorldStateService : IWorldStateService
    {
        private readonly SupabaseConfig _config;
        private readonly IAuthService _auth;
        private readonly SupabaseHttp _http;
        private readonly INAADLogger _log;

        public SupabaseWorldStateService(SupabaseConfig config, IAuthService auth, INAADLogger log)
        {
            _config = config;
            _auth = auth;
            _log = log;
            _http = new SupabaseHttp(config, log);
        }

        public async Task<WorldStateDto?> FetchWorldStateAsync()
        {
            var token = await _auth.GetAccessTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                _log.Warn("World", "Not authenticated");
                return null;
            }

            var url = $"{_config.RestBaseUrl}/world_state?id=eq.1&select=id,game_time,weather,traffic_level,nightlife_level";
            var result = await _http.SendAsync(
                url,
                UnityEngine.Networking.UnityWebRequest.kHttpVerbGET,
                null,
                token,
                ("Accept", "application/json"));

            if (!result.Ok)
            {
                _log.Error("World", $"Fetch failed: {result.StatusCode} {result.Body}");
                return null;
            }

            var body = result.Body;
            var dto = new WorldStateDto
            {
                Id = 1,
                GameTime = ExtractString(body, "game_time") ?? "",
                Weather = ExtractString(body, "weather") ?? "CLEAR",
                TrafficLevel = ExtractInt(body, "traffic_level") ?? 50,
                NightlifeLevel = ExtractInt(body, "nightlife_level") ?? 50
            };

            _log.Info("World", $"Loaded weather={dto.Weather} time={dto.GameTime}");
            return dto;
        }

        private static string? ExtractString(string json, string key)
        {
            var pattern = $"\"{key}\"";
            var idx = json.IndexOf(pattern, System.StringComparison.Ordinal);
            if (idx < 0) return null;
            idx = json.IndexOf(':', idx);
            if (idx < 0) return null;
            var rest = json.Substring(idx + 1).TrimStart();
            if (!rest.StartsWith("\"")) return null;
            var end = rest.IndexOf('"', 1);
            if (end < 0) return null;
            return rest.Substring(1, end - 1);
        }

        private static int? ExtractInt(string json, string key)
        {
            var pattern = $"\"{key}\"";
            var idx = json.IndexOf(pattern, System.StringComparison.Ordinal);
            if (idx < 0) return null;
            idx = json.IndexOf(':', idx);
            if (idx < 0) return null;
            var rest = json.Substring(idx + 1).TrimStart();
            var end = 0;
            while (end < rest.Length && (char.IsDigit(rest[end]) || rest[end] == '-'))
                end++;
            if (end == 0) return null;
            if (int.TryParse(rest.Substring(0, end), out var n))
                return n;
            return null;
        }
    }
}
