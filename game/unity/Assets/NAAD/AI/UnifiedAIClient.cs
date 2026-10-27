using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace NAAD.AI
{
    /// <summary>
    /// Emotional tone of a dialogue response.
    /// </summary>
    public enum DialogueEmotion
    {
        Neutral,
        Happy,
        Sad,
        Angry,
        Surprised,
        Thoughtful
    }

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
        public string requestId;
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
        private static readonly HttpClient httpClient = new HttpClient();

        public EdgeFunctionProvider(string url, float timeout)
        {
            this.url = url;
            this.timeout = timeout;
        }

        public async Task<DialogueResponse> GenerateDialogueAsync(DialogueRequest request)
        {
            float startTime = Time.realtimeSinceStartup;

            try
            {
                // Build request body
                var body = new
                {
                    requestId = request.requestId,
                    npcId = request.npcId,
                    playerMessage = request.playerMessage
                };

                string jsonBody = JsonUtility.ToJson(new EdgeFunctionRequestWrapper(body));

                using var requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
                };

                HttpResponseMessage response = await httpClient.SendAsync(requestMessage);

                if (response.IsSuccessStatusCode)
                {
                    string responseText = await response.Content.ReadAsStringAsync();
                    float latency = Time.realtimeSinceStartup - startTime;

                    EdgeFunctionResponseWrapper wrapper = JsonUtility.FromJson<EdgeFunctionResponseWrapper>(responseText);

                    if (wrapper != null && wrapper.success && wrapper.response != null)
                    {
                        metrics.RecordRequest(latency, true);

                        DialogueResponse dialogueResponse = new DialogueResponse
                        {
                            text = wrapper.response.text,
                            emotion = wrapper.response.emotion,
                            latency = latency
                        };

                        return dialogueResponse;
                    }
                    else
                    {
                        metrics.RecordRequest(Time.realtimeSinceStartup - startTime, false);
                        string errorMsg = wrapper?.errorMessage ?? "Unknown Edge Function error";
                        Debug.LogWarning($"Edge Function error: {errorMsg}");
                        throw new Exception(errorMsg);
                    }
                }
                else
                {
                    metrics.RecordRequest(Time.realtimeSinceStartup - startTime, false);
                    string errorBody = await response.Content.ReadAsStringAsync();
                    Debug.LogWarning($"Edge Function HTTP {response.StatusCode}: {errorBody}");
                    throw new Exception($"Edge Function returned HTTP {response.StatusCode}: {errorBody}");
                }
            }
            catch (TaskCanceledException)
            {
                metrics.RecordRequest(Time.realtimeSinceStartup - startTime, false);
                throw;
            }
            catch (Exception ex)
            {
                metrics.RecordRequest(Time.realtimeSinceStartup - startTime, false);
                throw;
            }
        }

        [System.Serializable]
        private class EdgeFunctionRequestWrapper
        {
            public string requestId;
            public string npcId;
            public string playerMessage;

            public EdgeFunctionRequestWrapper(string requestId, string npcId, string playerMessage)
            {
                this.requestId = requestId;
                this.npcId = npcId;
                this.playerMessage = playerMessage;
            }
        }

        [System.Serializable]
        private class EdgeFunctionResponseWrapper
        {
            public bool success;
            public DialogueResponse response;
            public string errorCode;
            public string errorMessage;
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
            // TODO: Implement HTTP request to Local LLM
            // Placeholder removed - using proper HTTP client

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
            // TODO: Implement OpenAI API request
            // Placeholder removed - using proper HTTP client
            
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
