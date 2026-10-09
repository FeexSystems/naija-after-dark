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
            NaadJson.TryGetString(result.Body, "npcId", out var gotNpcId);
            NaadJson.TryGetString(result.Body, "npcName", out var npcName);
            NaadJson.TryGetString(result.Body, "dialogue", out var greetDialogue);
            NaadJson.TryGetString(result.Body, "emotion", out var emotion);
            NaadJson.TryGetString(result.Body, "dialogueMode", out var mode);
            return new DialogueResult
            {
                Success = NaadJson.ReadSuccess(result.Body),
                NpcId = gotNpcId ?? npcId,
                NpcName = npcName ?? "",
                Dialogue = greetDialogue ?? "",
                Emotion = emotion ?? "neutral",
                DialogueMode = mode ?? "DETERMINISTIC"
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

            // The Edge Function may nest the validated Gemini payload under
            // "response" (or return it flat). Prefer the nested object.
            string scope = result.Body;
            if (NaadJson.TryGetObject(result.Body, "response", out var nested)
                && !string.IsNullOrEmpty(nested))
                scope = nested;

            NaadJson.TryGetString(scope, "dialogue", out var aiDialogue);
            NaadJson.TryGetString(scope, "emotion", out var aiEmotion);
            string? memSummary = null;
            string? memType = null;
            if (NaadJson.TryGetObject(scope, "memoryCandidate", out var mem)
                && !string.IsNullOrEmpty(mem))
            {
                NaadJson.TryGetString(mem, "summary", out memSummary);
                NaadJson.TryGetString(mem, "type", out memType);
            }
            return new DialogueResult
            {
                Success = NaadJson.ReadSuccess(result.Body),
                NpcId = TundeId,
                NpcName = "Tunde",
                Dialogue = aiDialogue ?? "",
                Emotion = aiEmotion ?? "neutral",
                DialogueMode = "AI",
                MemorySummary = memSummary,
                MemoryImportance = null,
                MemoryType = memType
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

            return result.Ok && NaadJson.ReadSuccess(result.Body);
        }

        private static string Escape(string s) =>
            s.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}
