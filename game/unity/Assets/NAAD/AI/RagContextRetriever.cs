using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace NAAD.AI
{
    /// <summary>
    /// RAG (Retrieval-Augmented Generation) context retrieval for NPC memory facts.
    /// Retrieves relevant NPC facts and context for dialogue generation.
    /// </summary>
    public class RagContextRetriever : MonoBehaviour
    {
        public static RagContextRetriever Instance { get; private set; }

        [Header("NPC Memory Database")]
        [SerializeField] private NpcMemoryDatabase memoryDatabase;

        [Header("Retrieval Settings")]
        [SerializeField] private int maxContextItems = 5;
        [SerializeField] private float relevanceThreshold = 0.3f;

        private Dictionary<string, List<NpcMemoryFact>> npcMemories = new Dictionary<string, List<NpcMemoryFact>>();

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

            LoadMemoryDatabase();
        }

        private void LoadMemoryDatabase()
        {
            if (memoryDatabase != null)
            {
                npcMemories = memoryDatabase.GetNpcMemories();
            }
        }

        /// <summary>
        /// Retrieve relevant context for an NPC based on query
        /// </summary>
        public List<NpcMemoryFact> RetrieveContext(string npcId, string query)
        {
            if (!npcMemories.ContainsKey(npcId))
            {
                return new List<NpcMemoryFact>();
            }

            List<NpcMemoryFact> allFacts = npcMemories[npcId];
            List<FactRelevance> scoredFacts = new List<FactRelevance>();

            foreach (var fact in allFacts)
            {
                float relevance = CalculateRelevance(query, fact);
                if (relevance >= relevanceThreshold)
                {
                    scoredFacts.Add(new FactRelevance(fact, relevance));
                }
            }

            // Sort by relevance and return top N
            scoredFacts.Sort((a, b) => b.relevance.CompareTo(a.relevance));
            int count = Mathf.Min(maxContextItems, scoredFacts.Count);
            
            return scoredFacts.GetRange(0, count).Select(fr => fr.fact).ToList();
        }

        /// <summary>
        /// Retrieve all memories for an NPC
        /// </summary>
        public List<NpcMemoryFact> GetAllMemories(string npcId)
        {
            if (npcMemories.ContainsKey(npcId))
            {
                return new List<NpcMemoryFact>(npcMemories[npcId]);
            }
            return new List<NpcMemoryFact>();
        }

        /// <summary>
        /// Add a new memory fact for an NPC
        /// </summary>
        public void AddMemory(string npcId, NpcMemoryFact fact)
        {
            if (!npcMemories.ContainsKey(npcId))
            {
                npcMemories[npcId] = new List<NpcMemoryFact>();
            }
            
            npcMemories[npcId].Add(fact);
        }

        /// <summary>
        /// Calculate relevance score between query and fact using simple keyword matching
        /// </summary>
        private float CalculateRelevance(string query, NpcMemoryFact fact)
        {
            if (string.IsNullOrEmpty(query) || fact == null)
            {
                return 0f;
            }

            string queryLower = query.ToLower();
            string factLower = fact.content.ToLower();
            string tagsLower = string.Join(" ", fact.tags).ToLower();

            // Count matching keywords
            string[] queryWords = queryLower.Split(' ');
            int matches = 0;

            foreach (string word in queryWords)
            {
                if (factLower.Contains(word) || tagsLower.Contains(word))
                {
                    matches++;
                }
            }

            // Calculate relevance as ratio of matches to query length
            float relevance = (float)matches / queryWords.Length;

            // Boost relevance for high-priority facts
            if (fact.priority == NpcMemoryFact.Priority.High)
            {
                relevance *= 1.5f;
            }
            else if (fact.priority == NpcMemoryFact.Priority.Low)
            {
                relevance *= 0.7f;
            }

            return Mathf.Clamp01(relevance);
        }

        /// <summary>
        /// Format retrieved context for LLM prompt
        /// </summary>
        public string FormatContextForPrompt(List<NpcMemoryFact> facts)
        {
            if (facts == null || facts.Count == 0)
            {
                return "";
            }

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("NPC Context:");
            
            foreach (var fact in facts)
            {
                sb.AppendLine($"- {fact.content}");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Clear all memories for an NPC
        /// </summary>
        public void ClearMemories(string npcId)
        {
            if (npcMemories.ContainsKey(npcId))
            {
                npcMemories[npcId].Clear();
            }
        }

        /// <summary>
        /// Clear all memories
        /// </summary>
        public void ClearAllMemories()
        {
            npcMemories.Clear();
        }

        private struct FactRelevance
        {
            public NpcMemoryFact fact;
            public float relevance;

            public FactRelevance(NpcMemoryFact fact, float relevance)
            {
                this.fact = fact;
                this.relevance = relevance;
            }
        }
    }

    /// <summary>
    /// NPC memory fact data structure
    /// </summary>
    [System.Serializable]
    public class NpcMemoryFact
    {
        public string id;
        public string content;
        public string[] tags;
        public Priority priority;
        public float timestamp;

        public enum Priority { Low, Normal, High }
    }

    /// <summary>
    /// ScriptableObject database for NPC memories
    /// </summary>
    [CreateAssetMenu(menu = "NAAD/AI/NPC Memory Database")]
    public class NpcMemoryDatabase : ScriptableObject
    {
        [System.Serializable]
        public class NpcMemoryEntry
        {
            public string npcId;
            public NpcMemoryFact[] memories;
        }

        public NpcMemoryEntry[] npcMemoryEntries;

        public Dictionary<string, List<NpcMemoryFact>> GetNpcMemories()
        {
            Dictionary<string, List<NpcMemoryFact>> memories = new Dictionary<string, List<NpcMemoryFact>>();

            if (npcMemoryEntries == null)
            {
                return memories;
            }

            foreach (var entry in npcMemoryEntries)
            {
                if (!memories.ContainsKey(entry.npcId))
                {
                    memories[entry.npcId] = new List<NpcMemoryFact>();
                }

                if (entry.memories != null)
                {
                    memories[entry.npcId].AddRange(entry.memories);
                }
            }

            return memories;
        }
    }
}
