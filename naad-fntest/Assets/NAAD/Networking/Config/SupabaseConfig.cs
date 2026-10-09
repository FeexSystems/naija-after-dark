using UnityEngine;

namespace NAAD.Networking.Config
{
    /// <summary>
    /// Client-safe Supabase settings only.
    /// NEVER put service_role or Gemini keys here.
    /// Create via Assets → Create → NAAD → Supabase Config.
    /// </summary>
    [CreateAssetMenu(fileName = "SupabaseConfig", menuName = "NAAD/Supabase Config")]
    public sealed class SupabaseConfig : ScriptableObject
    {
        [Header("Supabase project (public)")]
        [Tooltip("https://YOUR_PROJECT_REF.supabase.co")]
        public string projectUrl = "https://unzfqrfyejkyisalzkhc.supabase.co";

        [Tooltip("anon / publishable key only — never service_role")]
        public string anonKey = "";

        public string AuthBaseUrl => $"{projectUrl.TrimEnd('/')}/auth/v1";
        public string RestBaseUrl => $"{projectUrl.TrimEnd('/')}/rest/v1";

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(projectUrl) &&
            !string.IsNullOrWhiteSpace(anonKey) &&
            !anonKey.Contains("service_role", System.StringComparison.OrdinalIgnoreCase);
    }
}
