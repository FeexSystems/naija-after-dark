using System;
using UnityEngine;

namespace NAAD.Networking.Config
{
    /// <summary>
    /// Configuration for Supabase integration.
    /// </summary>
    [CreateAssetMenu(menu = "NAAD/Supabase Config")]
    public class SupabaseConfig : ScriptableObject
    {
        [Tooltip("Supabase project URL (e.g., https://your-project.supabase.co)")]
        public string projectUrl = "https://your-project.supabase.co";

        [Tooltip("Supabase anon/public API key")]
        public string anonKey = "YOUR_SUPABASE_ANON_KEY";

        [Tooltip("Use local LLM fallback instead of Edge Function (requires LLMUnity local setup)")]
        public bool useLocalLLM = false;

        [Tooltip("LLMUnity remote server URL (when using local LLM fallback)")]
        public string localLLMUrl = "http://127.0.0.1:8080";

        [Tooltip("LLMUnity API key (when using local LLM fallback, can be empty for GGUF models)")]
        public string localLLMApiKey = "";
    }
}