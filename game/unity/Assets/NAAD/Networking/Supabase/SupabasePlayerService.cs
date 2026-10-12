using System;
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

            var url = $"{_config.RestBaseUrl}/players?id=eq.{Uri.EscapeDataString(playerId)}&select=id,display_name,created_at,updated_at";
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

            // Response is a JSON array — wrap it so the object reader can index row 0.
            var body = result.Body.Trim();
            if (body == "[]" || body.Length < 3)
            {
                _log.Warn("Player", "No player row yet (trigger may still be running)");
                return null;
            }

            var wrapped = "{\"rows\":" + body + "}";
            if (!NaadJson.TryGetObject(wrapped, "rows", out var rows) || string.IsNullOrEmpty(rows))
            {
                _log.Error("Player", "Fetch failed: malformed player array");
                return null;
            }

            // Extract first array element using balanced brace matching
            var trimmedRows = rows.Trim();
            if (trimmedRows.Length < 2 || trimmedRows[0] != '[' || trimmedRows[trimmedRows.Length - 1] != ']')
            {
                _log.Error("Player", "Fetch failed: not an array");
                return null;
            }

            var arrayContent = trimmedRows.Substring(1, trimmedRows.Length - 2).Trim();
            if (string.IsNullOrEmpty(arrayContent))
            {
                _log.Warn("Player", "Empty player array");
                return null;
            }

            // Use ScanBalanced to extract the first object (handles nested braces correctly)
            var row = NaadJson.ScanBalanced(arrayContent);
            if (string.IsNullOrEmpty(row))
            {
                _log.Error("Player", "Fetch failed: could not extract player row");
                return null;
            }

            NaadJson.TryGetString(row, "id", out var id);
            NaadJson.TryGetString(row, "display_name", out var name);
            NaadJson.TryGetString(row, "created_at", out var created);
            NaadJson.TryGetString(row, "updated_at", out var updated);
            id ??= playerId;
            name ??= "Player";

            _log.Info("Player", $"Loaded {name} ({id})");
            return new PlayerDto
            {
                Id = id,
                DisplayName = name,
                CreatedAt = created ?? "",
                UpdatedAt = updated ?? ""
            };
        }
    }
}
