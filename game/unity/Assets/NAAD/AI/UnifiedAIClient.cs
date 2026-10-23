using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace NAAD.AI
{
    /// <summary>
    /// Provider-agnostic AI client supporting multiple LLM providers.
    /// Routes requests to Edge Function → Local AI → OpenAI with fallback.
    /// </summary>
    public class UnifiedAIClient : MonoBehaviour
    {
        public static UnifiedAIClient Instance { get; private set; }

        [Header("Provider Priority")]
        [SerializeField] private LLMProvider[] providerPriority = new LLMProvider[]
        {
            LLMProvider.EdgeFunction,
            LLMProvider.LocalLLM,
            LLMProvider.OpenAI
        };

        [Header("Edge Function Settings")]
        [SerializeField] private string edgeFunctionUrl;
        [SerializeField] private float edgeFunctionTimeout = 10f;

        [Header("Local LLM Settings")]
        [SerializeField] private string localLLMUrl = "http://127.0.0.1:8080";
        [SerializeField] private string localLLMApiKey = "";
        [SerializeField] private float localLLMTimeout = 30f;

        [Header("OpenAI Settings")]
        [SerializeField] private string openAIKey = "";
        [SerializeField] private string openAIModel = "gpt-4";
        [SerializeField] private float openAITimeout = 30f;

        [Header("Fallback Settings")]
        [SerializeField] private bool enableFallback = true;
        [SerializeField] private bool logFailures = true;

        private Dictionary<LLMProvider, ILLMProvider> providers = new Dictionary<LLMProvider, ILLMProvider>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            InitializeProviders();
        }

        private void InitializeProviders()
        {
            providers.Clear();

            // Edge Function Provider
            if (!string.IsNullOrEmpty(edgeFunctionUrl))
            {
                providers[LLMProvider.EdgeFunction] = new EdgeFunctionProvider(edgeFunctionUrl, edgeFunctionTimeout);
            }

            // Local LLM Provider
            if (!string.IsNullOrEmpty(localLLMUrl))
            {
                providers[LLMProvider.LocalLLM] = new LocalLLMProvider(localLLMUrl, localLLMApiKey, localLLMTimeout);
            }

            // OpenAI Provider
            if (!string.IsNullOrEmpty(openAIKey))
            {
                providers[LLMProvider.OpenAI] = new OpenAIProvider(openAIKey, openAIModel, openAITimeout);
            }
        }

        /// <summary>
        /// Generate dialogue response using available providers with fallback
        /// </summary>
        public async Task<DialogueResponse> GenerateDialogueAsync(DialogueRequest request)
        {
            foreach (LLMProvider providerType in providerPriority)
            {
                if (!providers.ContainsKey(providerType))
                {
                    if (logFailures)
                    {
                        Debug.LogWarning($"Provider {providerType} not configured, skipping");
                    }
                    continue;
                }

                ILLMProvider provider = providers[providerType];

                try
                {
                    Debug.Log($"Attempting dialogue generation with {providerType}");
                    DialogueResponse response = await provider.GenerateDialogueAsync(request);
                    
                    if (response != null && !string.IsNullOrEmpty(response.text))
                    {
                        Debug.Log($"Dialogue generation successful with {providerType}");
                        response.provider = providerType;
                        return response;
                    }
                }
                catch (System.Exception e)
                {
                    if (logFailures)
                    {
                        Debug.LogError($"Provider {providerType} failed: {e.Message}");
                    }

                    if (!enableFallback)
                    {
                        throw;
                    }
                }
            }

            // All providers failed
            Debug.LogError("All LLM providers failed");
            return new DialogueResponse
            {
                text = "I'm sorry, I couldn't generate a response right now.",
                emotion = DialogueEmotion.Neutral,
                provider = LLMProvider.None
            };
        }

        /// <summary>
        /// Generate dialogue with specific provider
        /// </summary>
        public async Task<DialogueResponse> GenerateDialogueWithProviderAsync(DialogueRequest request, LLMProvider providerType)
        {
            if (!providers.ContainsKey(providerType))
            {
                Debug.LogError($"Provider {providerType} not configured");
                return null;
            }

            ILLMProvider provider = providers[providerType];
            DialogueResponse response = await provider.GenerateDialogueAsync(request);
            response.provider = providerType;
            return response;
        }

        /// <summary>
        /// Check if provider is available
        /// </summary>
        public bool IsProviderAvailable(LLMProvider providerType)
        {
            return providers.ContainsKey(providerType);
        }

        /// <summary>
        /// Update provider configuration at runtime
        /// </summary>
        public void UpdateProviderConfig(LLMProvider providerType, string url, string key = "")
        {
            switch (providerType)
            {
                case LLMProvider.EdgeFunction:
                    edgeFunctionUrl = url;
                    providers[LLMProvider.EdgeFunction] = new EdgeFunctionProvider(edgeFunctionUrl, edgeFunctionTimeout);
                    break;
                case LLMProvider.LocalLLM:
                    localLLMUrl = url;
                    localLLMApiKey = key;
                    providers[LLMProvider.LocalLLM] = new LocalLLMProvider(localLLMUrl, localLLMApiKey, localLLMTimeout);
                    break;
                case LLMProvider.OpenAI:
                    openAIKey = key;
                    providers[LLMProvider.OpenAI] = new OpenAIProvider(openAIKey, openAIModel, openAITimeout);
                    break;
            }
        }

        /// <summary>
        /// Get performance metrics for all providers
        /// </summary>
        public Dictionary<LLMProvider, ProviderMetrics> GetProviderMetrics()
        {
            Dictionary<LLMProvider, ProviderMetrics> metrics = new Dictionary<LLMProvider, ProviderMetrics>();

            foreach (var kvp in providers)
            {
                metrics[kvp.Key] = kvp.Value.GetMetrics();
            }

            return metrics;
        }
    }

    /// <summary>
    /// LLM Provider types
    /// </summary>
    public enum LLMProvider
    {
        None,
        EdgeFunction,
        LocalLLM,
        OpenAI
    }

    /// <summary>
    /// Interface for LLM providers
    /// </summary>
    public interface ILLMProvider
    {
        Task<DialogueResponse> GenerateDialogueAsync(DialogueRequest request);
        ProviderMetrics GetMetrics();
    }

    /// <summary>
    /// Dialogue request structure
    /// </summary>
    [System.Serializable]
    public class DialogueRequest
    {
        public string npcId;
        public string playerMessage;
        public string context;
        public string[] previousMessages;
        public float reputation;
        public string relationship;
    }

    /// <summary>
    /// Dialogue response structure
    /// </summary>
    [System.Serializable]
    public class DialogueResponse
    {
        public string text;
        public DialogueEmotion emotion;
        public LLMProvider provider;
        public float latency;
        public int tokenCount;
    }

    /// <summary>
    /// Provider metrics
    /// </summary>
    [System.Serializable]
    public class ProviderMetrics
    {
        public int totalRequests;
        public int successfulRequests;
        public int failedRequests;
        public float averageLatency;
        public float totalLatency;

        public void RecordRequest(float latency, bool success)
        {
            totalRequests++;
            if (success)
            {
                successfulRequests++;
            }
            else
            {
                failedRequests++;
            }
            totalLatency += latency;
            averageLatency = totalLatency / totalRequests;
        }
    }

    // Provider implementations (simplified for structure)

    public class EdgeFunctionProvider : ILLMProvider
    {
        private string url;
        private float timeout;
        private ProviderMetrics metrics = new ProviderMetrics();

        public EdgeFunctionProvider(string url, float timeout)
        {
            this.url = url;
            this.timeout = timeout;
        }

        public async Task<DialogueResponse> GenerateDialogueAsync(DialogueRequest request)
        {
            // TODO: Implement HTTP request to Edge Function
            float startTime = Time.realtimeSinceStartup;
            
            // Placeholder implementation
            await Task.Delay(1000);
            
            float latency = Time.realtimeSinceStartup - startTime;
            metrics.RecordRequest(latency, true);

            return new DialogueResponse
            {
                text = "Response from Edge Function",
                emotion = DialogueEmotion.Neutral,
                latency = latency
            };
        }

        public ProviderMetrics GetMetrics()
        {
            return metrics;
        }
    }

    public class LocalLLMProvider : ILLMProvider
    {
        private string url;
        private string apiKey;
        private float timeout;
        private ProviderMetrics metrics = new ProviderMetrics();

        public LocalLLMProvider(string url, string apiKey, float timeout)
        {
            this.url = url;
            this.apiKey = apiKey;
            this.timeout = timeout;
        }

        public async Task<DialogueResponse> GenerateDialogueAsync(DialogueRequest request)
        {
            // TODO: Implement HTTP request to local LLM server
            float startTime = Time.realtimeSinceStartup;
            
            // Placeholder implementation
            await Task.Delay(1500);
            
            float latency = Time.realtimeSinceStartup - startTime;
            metrics.RecordRequest(latency, true);

            return new DialogueResponse
            {
                text = "Response from Local LLM",
                emotion = DialogueEmotion.Neutral,
                latency = latency
            };
        }

        public ProviderMetrics GetMetrics()
        {
            return metrics;
        }
    }

    public class OpenAIProvider : ILLMProvider
    {
        private string apiKey;
        private string model;
        private float timeout;
        private ProviderMetrics metrics = new ProviderMetrics();

        public OpenAIProvider(string apiKey, string model, float timeout)
        {
            this.apiKey = apiKey;
            this.model = model;
            this.timeout = timeout;
        }

        public async Task<DialogueResponse> GenerateDialogueAsync(DialogueRequest request)
        {
            // TODO: Implement OpenAI API request
            float startTime = Time.realtimeSinceStartup;
            
            // Placeholder implementation
            await Task.Delay(2000);
            
            float latency = Time.realtimeSinceStartup - startTime;
            metrics.RecordRequest(latency, true);

            return new DialogueResponse
            {
                text = "Response from OpenAI",
                emotion = DialogueEmotion.Neutral,
                latency = latency
            };
        }

        public ProviderMetrics GetMetrics()
        {
            return metrics;
        }
    }
}
