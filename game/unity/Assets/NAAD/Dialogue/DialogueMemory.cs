using UnityEngine;
using System.Collections.Generic;

namespace NAAD.Dialogue
{
    /// <summary>
    /// Local dialogue memory system for tracking seen dialogues and conversation history.
    /// This is a local convenience cache, not the source of truth. Server-side memory
    /// is authoritative and synchronized via Supabase Edge Function.
    /// </summary>
    public class DialogueMemory : MonoBehaviour
    {
        private static DialogueMemory _instance;
        public static DialogueMemory Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("DialogueMemory");
                    _instance = go.AddComponent<DialogueMemory>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        // Local memory storage
        private HashSet<string> _seenDialogues = new HashSet<string>();
        private Dictionary<string, List<string>> _conversationHistory = new Dictionary<string, List<string>>();
        private Dictionary<string, int> _interactionCounts = new Dictionary<string, int>();

        // Persistence key
        private const string SAVE_KEY = "NAAD_DialogueMemory";

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            LoadMemory();
        }

        /// <summary>
        /// Mark a dialogue as seen.
        /// </summary>
        public void MarkSeen(string dialogueId)
        {
            if (string.IsNullOrEmpty(dialogueId)) return;

            _seenDialogues.Add(dialogueId);
            SaveMemory();
        }

        /// <summary>
        /// Check if a dialogue has been seen.
        /// </summary>
        public bool HasSeen(string dialogueId)
        {
            return _seenDialogues.Contains(dialogueId);
        }

        /// <summary>
        /// Record a conversation entry with an NPC.
        /// </summary>
        public void RecordConversation(string npcId, string dialogueText)
        {
            if (string.IsNullOrEmpty(npcId) || string.IsNullOrEmpty(dialogueText)) return;

            if (!_conversationHistory.ContainsKey(npcId))
            {
                _conversationHistory[npcId] = new List<string>();
            }

            _conversationHistory[npcId].Add(dialogueText);

            // Keep only last 20 conversations per NPC
            if (_conversationHistory[npcId].Count > 20)
            {
                _conversationHistory[npcId].RemoveAt(0);
            }

            // Increment interaction count
            if (!_interactionCounts.ContainsKey(npcId))
            {
                _interactionCounts[npcId] = 0;
            }
            _interactionCounts[npcId]++;

            SaveMemory();
        }

        /// <summary>
        /// Get conversation history for an NPC.
        /// </summary>
        public List<string> GetConversationHistory(string npcId)
        {
            if (_conversationHistory.ContainsKey(npcId))
            {
                return new List<string>(_conversationHistory[npcId]);
            }
            return new List<string>();
        }

        /// <summary>
        /// Get interaction count for an NPC.
        /// </summary>
        public int GetInteractionCount(string npcId)
        {
            if (_interactionCounts.ContainsKey(npcId))
            {
                return _interactionCounts[npcId];
            }
            return 0;
        }

        /// <summary>
        /// Clear all memory (useful for testing).
        /// </summary>
        public void ClearMemory()
        {
            _seenDialogues.Clear();
            _conversationHistory.Clear();
            _interactionCounts.Clear();
            SaveMemory();
        }

        /// <summary>
        /// Clear memory for a specific NPC.
        /// </summary>
        public void ClearNpcMemory(string npcId)
        {
            if (_conversationHistory.ContainsKey(npcId))
            {
                _conversationHistory[npcId].Clear();
            }
            if (_interactionCounts.ContainsKey(npcId))
            {
                _interactionCounts[npcId] = 0;
            }
            SaveMemory();
        }

        /// <summary>
        /// Save memory to PlayerPrefs.
        /// </summary>
        private void SaveMemory()
        {
            DialogueMemoryData data = ConvertToSerializable();
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Load memory from PlayerPrefs.
        /// </summary>
        private void LoadMemory()
        {
            string json = PlayerPrefs.GetString(SAVE_KEY, "");
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    DialogueMemoryData data = JsonUtility.FromJson<DialogueMemoryData>(json);
                    ConvertFromSerializable(data);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[DialogueMemory] Failed to load memory: {e.Message}");
                }
            }
        }

        /// <summary>
        /// Serializable data structure for PlayerPrefs.
        /// Note: Dictionaries are converted to lists for JSON serialization.
        /// </summary>
        [System.Serializable]
        private class DialogueMemoryData
        {
            public List<string> seenDialogues;
            public List<NpcConversationEntry> conversationHistory;
            public List<NpcInteractionEntry> interactionCounts;
        }

        [System.Serializable]
        private class NpcConversationEntry
        {
            public string npcId;
            public List<string> conversations;
        }

        [System.Serializable]
        private class NpcInteractionEntry
        {
            public string npcId;
            public int count;
        }

        /// <summary>
        /// Convert Dictionary to serializable format.
        /// </summary>
        private DialogueMemoryData ConvertToSerializable()
        {
            DialogueMemoryData data = new DialogueMemoryData();
            data.seenDialogues = new List<string>(_seenDialogues);

            data.conversationHistory = new List<NpcConversationEntry>();
            foreach (var kvp in _conversationHistory)
            {
                data.conversationHistory.Add(new NpcConversationEntry
                {
                    npcId = kvp.Key,
                    conversations = kvp.Value
                });
            }

            data.interactionCounts = new List<NpcInteractionEntry>();
            foreach (var kvp in _interactionCounts)
            {
                data.interactionCounts.Add(new NpcInteractionEntry
                {
                    npcId = kvp.Key,
                    count = kvp.Value
                });
            }

            return data;
        }

        /// <summary>
        /// Convert from serializable format to Dictionary.
        /// </summary>
        private void ConvertFromSerializable(DialogueMemoryData data)
        {
            _seenDialogues = new HashSet<string>(data.seenDialogues);

            _conversationHistory = new Dictionary<string, List<string>>();
            foreach (var entry in data.conversationHistory)
            {
                _conversationHistory[entry.npcId] = entry.conversations;
            }

            _interactionCounts = new Dictionary<string, int>();
            foreach (var entry in data.interactionCounts)
            {
                _interactionCounts[entry.npcId] = entry.count;
            }
        }
    }
}
