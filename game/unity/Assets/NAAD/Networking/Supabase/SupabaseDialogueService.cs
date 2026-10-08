using System;
using System.Threading.Tasks;
using NAAD.Core.Logging;
using NAAD.Networking.Config;

namespace NAAD.Networking.Supabase
{
    public sealed class SupabaseDialogueService : IDialogueService
    {
        private readonly SupabaseConfig _config;
        private readonly IAuthService _auth;
        private readonly SupabaseHttp _http;
        private readonly INAADLogger _log;

        public static readonly string TundeId = "e1111111-1111-1111-1111-111111111101";
        public static readonly string MamaSeyiId = "e1111111-1111-1111-1111-111111111102";

        public SupabaseDialogueService(SupabaseConfig config, IAuthService auth, INAADLogger log)
        {
            _config = config;
            _auth = auth;
            _log = log;
            _http = new SupabaseHttp(config, log);
        }

        public async Task<DialogueResult?> GreetAsync(string npcId)
        {
            var token = await _auth.GetAccessTokenAsync();
            if (string.IsNullOrEmpty(token)) return null;

            var body = $"{{\"p_npc_id\":\"{npcId}\"}}";
            var result = await _http.SendAsync(
                $"{_config.RestBaseUrl}/rpc/naad_get_npc_greeting",
                UnityEngine.Networking.UnityWebRequest.kHttpVerbPOST,
                body,
                token);

            if (!result.Ok) return null;
            return new DialogueResult
            {
                Success = true,
                NpcId = Extract(result.Body, "npcId") ?? npcId,
                NpcName = Extract(result.Body, "npcName") ?? "",
                Dialogue = Extract(result.Body, "dialogue") ?? "",
                Emotion = Extract(result.Body, "emotion") ?? "neutral",
                DialogueMode = Extract(result.Body, "dialogueMode") ?? "DETERMINISTIC"
            };
        }

        public async Task<DialogueResult?> TalkToTundeAsync(string playerMessage, string requestId)
        {
            var token = await _auth.GetAccessTokenAsync();
            if (string.IsNullOrEmpty(token)) return null;

            // Edge function path (requires deploy + GEMINI_API_KEY)
            var url = $"{_config.projectUrl.TrimEnd('/')}/functions/v1/npc-dialogue";
            var body =
                $"{{\"requestId\":\"{Escape(requestId)}\",\"npcId\":\"{TundeId}\",\"playerMessage\":\"{Escape(playerMessage)}\"}}";

            var result = await _http.SendAsync(url, "POST", body, token);
            if (!result.Ok)
            {
                _log.Warn("Dialogue", $"AI path failed ({result.StatusCode}); check Edge Function deploy");
                return null;
            }

            var dialogue = ExtractNested(result.Body, "dialogue") ?? Extract(result.Body, "dialogue");
            return new DialogueResult
            {
                Success = result.Body.IndexOf("\"success\":true", StringComparison.OrdinalIgnoreCase) >= 0,
                NpcId = TundeId,
                NpcName = "Tunde",
                Dialogue = dialogue ?? "",
                Emotion = ExtractNested(result.Body, "emotion") ?? "neutral",
                DialogueMode = "AI",
                MemorySummary = ExtractNested(result.Body, "summary"),
                MemoryImportance = null,
                MemoryType = ExtractNested(result.Body, "type")
            };
        }

        public async Task<bool> CommitMemoryAsync(
            string requestId, string npcId, string type, string summary, float importance)
        {
            var token = await _auth.GetAccessTokenAsync();
            if (string.IsNullOrEmpty(token)) return false;

            var body =
                $"{{\"p_request_id\":\"{Escape(requestId)}\",\"p_npc_id\":\"{npcId}\",\"p_memory_type\":\"{Escape(type)}\",\"p_summary\":\"{Escape(summary)}\",\"p_importance\":{importance.ToString(System.Globalization.CultureInfo.InvariantCulture)}}}";

            var result = await _http.SendAsync(
                $"{_config.RestBaseUrl}/rpc/naad_commit_npc_memory",
                "POST",
                body,
                token);

            return result.Ok && result.Body.IndexOf("\"success\":true", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string Escape(string s) =>
            s.Replace("\\", "\\\\").Replace("\"", "\\\"");

        private static string? Extract(string json, string key)
        {
            var pattern = $"\"{key}\"";
            var idx = json.IndexOf(pattern, StringComparison.Ordinal);
            if (idx < 0) return null;
            idx = json.IndexOf(':', idx);
            if (idx < 0) return null;
            var rest = json.Substring(idx + 1).TrimStart();
            if (!rest.StartsWith("\"")) return null;
            var end = rest.IndexOf('"', 1);
            return end < 0 ? null : rest.Substring(1, end - 1);
        }

        private static string? ExtractNested(string json, string key) => Extract(json, key);
    }
}
