using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NAAD.AI
{
    /// <summary>
    /// Performance benchmarking tool for LLM providers.
    /// Tests cost/performance tradeoffs between Edge, Local, and Remote LLMs.
    /// </summary>
    public class LLMPerformanceBenchmark : MonoBehaviour
    {
        public static LLMPerformanceBenchmark Instance { get; private set; }

        [Header("Benchmark Settings")]
        [SerializeField] private int testIterations = 10;
        [SerializeField] private string testQuery = "Hello, how are you today?";
        [SerializeField] private bool runOnStart = false;

        [Header("Test Providers")]
        [SerializeField] private bool testEdgeFunction = true;
        [SerializeField] private bool testLocalLLM = true;
        [SerializeField] private bool testOpenAI = false;

        private List<BenchmarkResult> results = new List<BenchmarkResult>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (runOnStart)
            {
                RunBenchmark();
            }
        }

        /// <summary>
        /// Run full benchmark across all providers
        /// </summary>
        public async void RunBenchmark()
        {
            Debug.Log("Starting LLM Performance Benchmark...");
            results.Clear();

            if (testEdgeFunction && UnifiedAIClient.Instance.IsProviderAvailable(LLMProvider.EdgeFunction))
            {
                await BenchmarkProvider(LLMProvider.EdgeFunction);
            }

            if (testLocalLLM && UnifiedAIClient.Instance.IsProviderAvailable(LLMProvider.LocalLLM))
            {
                await BenchmarkProvider(LLMProvider.LocalLLM);
            }

            if (testOpenAI && UnifiedAIClient.Instance.IsProviderAvailable(LLMProvider.OpenAI))
            {
                await BenchmarkProvider(LLMProvider.OpenAI);
            }

            PrintResults();
        }

        /// <summary>
        /// Benchmark a single provider
        /// </summary>
        private async Task BenchmarkProvider(LLMProvider provider)
        {
            Debug.Log($"Benchmarking {provider}...");

            List<float> latencies = new List<float>();
            int successCount = 0;
            int failureCount = 0;

            for (int i = 0; i < testIterations; i++)
            {
                DialogueRequest request = new DialogueRequest
                {
                    npcId = "benchmark_npc",
                    playerMessage = testQuery,
                    context = "",
                    previousMessages = new string[0],
                    reputation = 50,
                    relationship = "neutral"
                };

                float startTime = Time.realtimeSinceStartup;

                try
                {
                    DialogueResponse response = await UnifiedAIClient.Instance.GenerateDialogueWithProviderAsync(request, provider);
                    float latency = Time.realtimeSinceStartup - startTime;

                    if (response != null && !string.IsNullOrEmpty(response.text))
                    {
                        latencies.Add(latency);
                        successCount++;
                    }
                    else
                    {
                        failureCount++;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Benchmark iteration {i} failed for {provider}: {e.Message}");
                    failureCount++;
                }

                // Small delay between requests
                await Task.Delay(500);
            }

            BenchmarkResult result = new BenchmarkResult
            {
                provider = provider,
                totalIterations = testIterations,
                successCount = successCount,
                failureCount = failureCount,
                averageLatency = latencies.Count > 0 ? latencies.Average() : 0f,
                minLatency = latencies.Count > 0 ? latencies.Min() : 0f,
                maxLatency = latencies.Count > 0 ? latencies.Max() : 0f,
                successRate = (float)successCount / testIterations
            };

            results.Add(result);
            Debug.Log($"Benchmark complete for {provider}: Avg {result.averageLatency:F2}s, Success Rate {result.successRate:P0}");
        }

        /// <summary>
        /// Print benchmark results to console
        /// </summary>
        private void PrintResults()
        {
            Debug.Log("=== LLM Performance Benchmark Results ===");
            Debug.Log($"Provider | Iterations | Success | Failure | Avg Latency | Min Latency | Max Latency | Success Rate");
            Debug.Log("---------|------------|---------|---------|-------------|-------------|-------------|-------------");

            foreach (var result in results)
            {
                Debug.Log($"{result.provider,-8} | {result.totalIterations,-10} | {result.successCount,-7} | {result.failureCount,-7} | {result.averageLatency:F2}s | {result.minLatency:F2}s | {result.maxLatency:F2}s | {result.successRate:P0}");
            }

            // Find best provider by latency
            if (results.Count > 0)
            {
                BenchmarkResult best = results.OrderBy(r => r.averageLatency).First();
                Debug.Log($"\nFastest Provider: {best.provider} ({best.averageLatency:F2}s average)");
            }
        }

        /// <summary>
        /// Get benchmark results
        /// </summary>
        public List<BenchmarkResult> GetResults()
        {
            return new List<BenchmarkResult>(results);
        }

        /// <summary>
        /// Clear benchmark results
        /// </summary>
        public void ClearResults()
        {
            results.Clear();
        }
    }

    /// <summary>
    /// Benchmark result data structure
    /// </summary>
    [System.Serializable]
    public class BenchmarkResult
    {
        public LLMProvider provider;
        public int totalIterations;
        public int successCount;
        public int failureCount;
        public float averageLatency;
        public float minLatency;
        public float maxLatency;
        public float successRate;
    }
}
