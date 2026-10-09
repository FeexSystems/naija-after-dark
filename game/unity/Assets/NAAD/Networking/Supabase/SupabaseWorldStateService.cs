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
            var success = NaadJson.ReadSuccess(json);
            NaadJson.TryGetString(json, "errorCode", out var errorCode);
            NaadJson.TryGetString(json, "gameDate", out var gameDate);
            NaadJson.TryGetString(json, "gameTime", out var gameTime);
            var minutes = NaadJson.TryGetInt(json, "minutesSinceMidnight", out var m) ? m : 0;
            NaadJson.TryGetString(json, "period", out var period);
            var timeScale = NaadJson.TryGetDouble(json, "timeScale", out var ts) ? (float)ts : 60f;
            NaadJson.TryGetString(json, "weather", out var weather);
            var traffic = NaadJson.TryGetInt(json, "trafficLevel", out var tr) ? tr : 50;
            var nightlife = NaadJson.TryGetInt(json, "nightlifeLevel", out var nl) ? nl : 50;
            return new WorldClockDto
            {
                Success = success,
                ErrorCode = errorCode,
                GameDate = gameDate ?? "",
                GameTime = gameTime ?? "",
                MinutesSinceMidnight = minutes,
                Period = period ?? "DAY",
                TimeScale = timeScale,
                Weather = weather ?? "CLEAR",
                TrafficLevel = traffic,
                NightlifeLevel = nightlife
            };
        }

        private static string Escape(string s) =>
            s.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}
