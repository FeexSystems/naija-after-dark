using System;
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
            var clock = await FetchWorldClockAsync();
            if (clock == null || !clock.Success) return null;
            return new WorldStateDto
            {
                Id = 1,
                GameTime = clock.GameTime,
                Weather = clock.Weather,
                TrafficLevel = clock.TrafficLevel,
                NightlifeLevel = clock.NightlifeLevel,
                TimeScale = clock.TimeScale
            };
        }

        public async Task<WorldClockDto?> FetchWorldClockAsync()
        {
            return await CallClockRpc("naad_get_world_clock", null);
        }

        public async Task<WorldClockDto?> AdvanceWorldTimeAsync()
        {
            return await CallClockRpc("naad_advance_world_time", null);
        }

        public async Task<WorldClockDto?> SetPeriodAsync(string period)
        {
            var body = $"{{\"p_period\":\"{Escape(period)}\"}}";
            return await CallClockRpc("naad_set_world_period", body);
        }

        private async Task<WorldClockDto?> CallClockRpc(string fn, string? body)
        {
            var token = await _auth.GetAccessTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                _log.Warn("World", "Not authenticated");
                return null;
            }

            var url = $"{_config.RestBaseUrl}/rpc/{fn}";
            var result = await _http.SendAsync(
                url,
                UnityEngine.Networking.UnityWebRequest.kHttpVerbPOST,
                body ?? "{}",
                token);

            if (!result.Ok)
            {
                _log.Error("World", $"{fn} failed: {result.StatusCode} {result.Body}");
                return null;
            }

            var dto = ParseClock(result.Body);
            if (dto.Success)
                _log.Info("World", $"period={dto.Period} minutes={dto.MinutesSinceMidnight} nightlife={dto.NightlifeLevel}");
            return dto;
        }

        private static WorldClockDto ParseClock(string json)
        {
            var success = json.IndexOf("\"success\":true", StringComparison.OrdinalIgnoreCase) >= 0
                          || json.IndexOf("\"success\": true", StringComparison.OrdinalIgnoreCase) >= 0;
            return new WorldClockDto
            {
                Success = success,
                ErrorCode = ExtractString(json, "errorCode"),
                GameDate = ExtractString(json, "gameDate") ?? "",
                GameTime = ExtractString(json, "gameTime") ?? "",
                MinutesSinceMidnight = ExtractInt(json, "minutesSinceMidnight") ?? 0,
                Period = ExtractString(json, "period") ?? "DAY",
                TimeScale = ExtractFloat(json, "timeScale") ?? 60f,
                Weather = ExtractString(json, "weather") ?? "CLEAR",
                TrafficLevel = ExtractInt(json, "trafficLevel") ?? 50,
                NightlifeLevel = ExtractInt(json, "nightlifeLevel") ?? 50
            };
        }

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

        private static float? ExtractFloat(string json, string key)
        {
            var pattern = $"\"{key}\"";
            var idx = json.IndexOf(pattern, StringComparison.Ordinal);
            if (idx < 0) return null;
            idx = json.IndexOf(':', idx);
            if (idx < 0) return null;
            var rest = json.Substring(idx + 1).TrimStart();
            var end = 0;
            while (end < rest.Length && (char.IsDigit(rest[end]) || rest[end] == '-' || rest[end] == '.'))
                end++;
            if (end == 0) return null;
            return float.TryParse(rest.Substring(0, end),
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var n) ? n : null;
        }
    }
}
