using UnityEngine;
using System.Collections.Generic;

namespace NAAD.Performance
{
    /// <summary>
    /// Performance optimization across all gameplay enhancements.
    /// Handles object pooling, LOD management, and resource optimization.
    /// </summary>
    public class PerformanceOptimizer : MonoBehaviour
    {
        public static PerformanceOptimizer Instance { get; private set; }

        [Header("Object Pooling")]
        [SerializeField] private bool enableObjectPooling = true;
        [SerializeField] private int dialogueBubblePoolSize = 10;
        [SerializeField] private int responseButtonPoolSize = 5;

        [Header("LOD Settings")]
        [SerializeField] private bool enableLOD = true;
        [SerializeField] private float lodSwitchDistance = 20f;
        [SerializeField] private int maxHighDetailNpcs = 5;

        [Header("Update Optimization")]
        [SerializeField] private bool reduceUpdateFrequency = true;
        [SerializeField] private float npcUpdateInterval = 0.05f;
        [SerializeField] private float dialogueUpdateInterval = 0.1f;

        [Header("Memory Management")]
        [SerializeField] private bool enableMemoryManagement = true;
        [SerializeField] private int maxHistoryEntries = 20;
        [SerializeField] private float memoryCleanupInterval = 60f;

        private Dictionary<string, Queue<GameObject>> objectPools = new Dictionary<string, Queue<GameObject>>();
        private List<Transform> highDetailNpcs = new List<Transform>();
        private Dictionary<Transform, int> npcLODLevels = new Dictionary<Transform, int>();
        private float lastMemoryCleanup;

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

            InitializeObjectPools();
        }

        private void Start()
        {
            if (enableLOD)
            {
                StartCoroutine(LODUpdateCoroutine());
            }

            if (enableMemoryManagement)
            {
                StartCoroutine(MemoryCleanupCoroutine());
            }
        }

        private void InitializeObjectPools()
        {
            if (!enableObjectPooling) return;

            // Pre-populate dialogue bubble pool
            CreatePool("DialogueBubble", dialogueBubblePoolSize);
            CreatePool("ResponseButton", responseButtonPoolSize);
        }

        private void CreatePool(string poolName, int size)
        {
            Queue<GameObject> pool = new Queue<GameObject>();

            for (int i = 0; i < size; i++)
            {
                GameObject obj = new GameObject($"{poolName}_Pooled_{i}");
                obj.SetActive(false);
                pool.Enqueue(obj);
            }

            objectPools[poolName] = pool;
        }

        /// <summary>
        /// Get object from pool
        /// </summary>
        public GameObject GetFromPool(string poolName)
        {
            if (!enableObjectPooling || !objectPools.ContainsKey(poolName))
            {
                return new GameObject(poolName);
            }

            Queue<GameObject> pool = objectPools[poolName];

            if (pool.Count > 0)
            {
                GameObject obj = pool.Dequeue();
                obj.SetActive(true);
                return obj;
            }

            // Pool exhausted, create new
            return new GameObject(poolName);
        }

        /// <summary>
        /// Return object to pool
        /// </summary>
        public void ReturnToPool(string poolName, GameObject obj)
        {
            if (!enableObjectPooling || !objectPools.ContainsKey(poolName))
            {
                Destroy(obj);
                return;
            }

            obj.SetActive(false);
            objectPools[poolName].Enqueue(obj);
        }

        /// <summary>
        /// LOD update coroutine
        /// </summary>
        private System.Collections.IEnumerator LODUpdateCoroutine()
        {
            while (true)
            {
                UpdateLOD();
                yield return new WaitForSeconds(npcUpdateInterval);
            }
        }

        /// <summary>
        /// Update LOD levels based on distance to camera
        /// </summary>
        private void UpdateLOD()
        {
            if (!enableLOD) return;

            Camera mainCamera = Camera.main;
            if (mainCamera == null) return;

            Vector3 cameraPos = mainCamera.transform.position;
            List<Transform> allNpcs = new List<Transform>(npcLODLevels.Keys);

            // Sort by distance
            allNpcs.Sort((a, b) => Vector3.Distance(cameraPos, a.position).CompareTo(Vector3.Distance(cameraPos, b.position)));

            // Assign LOD levels
            for (int i = 0; i < allNpcs.Count; i++)
            {
                Transform npc = allNpcs[i];
                float distance = Vector3.Distance(cameraPos, npc.position);
                int lodLevel = CalculateLODLevel(distance);

                if (npcLODLevels[npc] != lodLevel)
                {
                    npcLODLevels[npc] = lodLevel;
                    ApplyLODLevel(npc, lodLevel);
                }
            }
        }

        /// <summary>
        /// Calculate LOD level based on distance
        /// </summary>
        private int CalculateLODLevel(float distance)
        {
            if (distance < lodSwitchDistance)
            {
                return 0; // High detail
            }
            else if (distance < lodSwitchDistance * 2f)
            {
                return 1; // Medium detail
            }
            else
            {
                return 2; // Low detail
            }
        }

        /// <summary>
        /// Apply LOD level to NPC
        /// </summary>
        private void ApplyLODLevel(Transform npc, int lodLevel)
        {
            // TODO: Implement actual LOD switching (mesh quality, animation quality, etc.)
            // This could reduce animation update rate, disable non-essential components, etc.
        }

        /// <summary>
        /// Register NPC for LOD management
        /// </summary>
        public void RegisterNpcForLOD(Transform npc)
        {
            if (!npcLODLevels.ContainsKey(npc))
            {
                npcLODLevels[npc] = 0;
            }
        }

        /// <summary>
        /// Unregister NPC from LOD management
        /// </summary>
        public void UnregisterNpcFromLOD(Transform npc)
        {
            npcLODLevels.Remove(npc);
        }

        /// <summary>
        /// Memory cleanup coroutine
        /// </summary>
        private System.Collections.IEnumerator MemoryCleanupCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(memoryCleanupInterval);
                CleanupMemory();
            }
        }

        /// <summary>
        /// Clean up unused resources
        /// </summary>
        private void CleanupMemory()
        {
            if (!enableMemoryManagement) return;

            // Clean up old dialogue history entries
            var conversationUI = FindObjectOfType<UI.ConversationUI>();
            if (conversationUI != null)
            {
                conversationUI.ClearHistory();
            }

            // Clean up old NPC memories
            var ragRetriever = AI.RagContextRetriever.Instance;
            if (ragRetriever != null)
            {
                // Keep only recent memories
                // TODO: Implement memory cleanup logic
            }

            // Force garbage collection
            System.GC.Collect();
            Resources.UnloadUnusedAssets();

            Debug.Log("Performance cleanup completed");
        }

        /// <summary>
        /// Get performance statistics
        /// </summary>
        public PerformanceStats GetPerformanceStats()
        {
            return new PerformanceStats
            {
                frameRate = (int)(1f / Time.deltaTime),
                memoryUsage = System.GC.GetTotalMemory(false) / 1024 / 1024,
                pooledObjects = CountPooledObjects(),
                activeNpcs = npcLODLevels.Count
            };
        }

        /// <summary>
        /// Count total pooled objects
        /// </summary>
        private int CountPooledObjects()
        {
            int count = 0;
            foreach (var pool in objectPools.Values)
            {
                count += pool.Count;
            }
            return count;
        }

        /// <summary>
        /// Set target frame rate
        /// </summary>
        public void SetTargetFrameRate(int targetFPS)
        {
            Application.targetFrameRate = targetFPS;
        }

        /// <summary>
        /// Enable/disable vsync
        /// </summary>
        public void SetVSyncEnabled(bool enabled)
        {
            QualitySettings.vSyncCount = enabled ? 1 : 0;
        }
    }

    /// <summary>
    /// Performance statistics structure
    /// </summary>
    [System.Serializable]
    public class PerformanceStats
    {
        public int frameRate;
        public long memoryUsage; // in MB
        public int pooledObjects;
        public int activeNpcs;
    }
}
