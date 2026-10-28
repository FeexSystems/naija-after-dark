using UnityEngine;
using System.Collections.Generic;
using NAAD.Networking;

namespace NAAD.Networking
{
    /// <summary>
    /// Multiplayer synchronization of NPC states across connected clients.
    /// Syncs position, rotation, current behavior state, and dialogue state.
    /// </summary>
    public class NpcStateSync : MonoBehaviour
    {
        public static NpcStateSync Instance { get; private set; }

        [Header("Sync Settings")]
        [SerializeField] private float syncInterval = 0.1f;
        [SerializeField] private float positionThreshold = 0.01f;
        [SerializeField] private float rotationThreshold = 1f;
        [SerializeField] private bool syncPosition = true;
        [SerializeField] private bool syncRotation = true;
        [SerializeField] private bool syncBehaviorState = true;
        [SerializeField] private bool syncDialogueState = true;

        private Dictionary<string, NpcStateData> npcStates = new Dictionary<string, NpcStateData>();
        private Dictionary<string, Transform> npcTransforms = new Dictionary<string, Transform>();
        private Dictionary<string, BehaviorStateMachine> npcStateMachines = new Dictionary<string, BehaviorStateMachine>();
        private Dictionary<string, IDialogueService> npcDialogueManagers = new Dictionary<string, IDialogueService>();

        private float lastSyncTime;

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
        }

        private void Update()
        {
            if (Time.time - lastSyncTime >= syncInterval)
            {
                SyncAllNpcs();
                lastSyncTime = Time.time;
            }
        }

        /// <summary>
        /// Register an NPC for synchronization
        /// </summary>
        public void RegisterNpc(string npcId, Transform npcTransform, BehaviorStateMachine stateMachine = null, IDialogueService dialogueManager = null)
        {
            if (!npcTransforms.ContainsKey(npcId))
            {
                npcTransforms[npcId] = npcTransform;
                npcStates[npcId] = new NpcStateData { npcId = npcId };
            }

            if (stateMachine != null)
            {
                npcStateMachines[npcId] = stateMachine;
            }

            if (dialogueManager != null)
            {
                npcDialogueManagers[npcId] = dialogueManager;
            }
        }

        /// <summary>
        /// Unregister an NPC from synchronization
        /// </summary>
        public void UnregisterNpc(string npcId)
        {
            npcTransforms.Remove(npcId);
            npcStates.Remove(npcId);
            npcStateMachines.Remove(npcId);
            npcDialogueManagers.Remove(npcId);
        }

        /// <summary>
        /// Sync all registered NPCs
        /// </summary>
        private void SyncAllNpcs()
        {
            foreach (var kvp in npcTransforms)
            {
                string npcId = kvp.Key;
                Transform transform = kvp.Value;

                if (transform == null) continue;

                NpcStateData currentState = npcStates[npcId];
                NpcStateData newState = CaptureNpcState(npcId, transform);

                // Check if state changed significantly
                if (HasStateChanged(currentState, newState))
                {
                    npcStates[npcId] = newState;
                    BroadcastNpcState(newState);
                }
            }
        }

        /// <summary>
        /// Capture current state of an NPC
        /// </summary>
        private NpcStateData CaptureNpcState(string npcId, Transform transform)
        {
            NpcStateData state = new NpcStateData
            {
                npcId = npcId,
                position = transform.position,
                rotation = transform.rotation.eulerAngles,
                timestamp = Time.time
            };

            if (syncBehaviorState && npcStateMachines.ContainsKey(npcId))
            {
                state.behaviorState = npcStateMachines[npcId].GetCurrentState();
            }

            if (syncDialogueState && npcDialogueManagers.ContainsKey(npcId))
            {
                state.isInDialogue = false; // TODO: Implement proper dialogue state tracking
                state.dialogueLineIndex = GetCurrentDialogueLineIndex(npcId);
            }

            return state;
        }

        /// <summary>
        /// Check if state changed significantly
        /// </summary>
        private bool HasStateChanged(NpcStateData oldState, NpcStateData newState)
        {
            if (syncPosition)
            {
                float positionDelta = Vector3.Distance(oldState.position, newState.position);
                if (positionDelta > positionThreshold)
                {
                    return true;
                }
            }

            if (syncRotation)
            {
                float rotationDelta = Quaternion.Angle(Quaternion.Euler(oldState.rotation), Quaternion.Euler(newState.rotation));
                if (rotationDelta > rotationThreshold)
                {
                    return true;
                }
            }

            if (syncBehaviorState && oldState.behaviorState != newState.behaviorState)
            {
                return true;
            }

            if (syncDialogueState && (oldState.isInDialogue != newState.isInDialogue || oldState.dialogueLineIndex != newState.dialogueLineIndex))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get current dialogue line index for NPC
        /// </summary>
        private int GetCurrentDialogueLineIndex(string npcId)
        {
            // TODO: Implement dialogue line index tracking
            return 0;
        }

        /// <summary>
        /// Broadcast NPC state to other clients (placeholder for network implementation)
        /// </summary>
        private void BroadcastNpcState(NpcStateData state)
        {
            // TODO: Implement actual network broadcasting
            // This would integrate with Supabase real-time or Unity Netcode
            Debug.Log($"Broadcasting state for {state.npcId}: Pos={state.position}, State={state.behaviorState}");
        }

        /// <summary>
        /// Apply received NPC state from network
        /// </summary>
        public void ApplyNpcState(NpcStateData state)
        {
            if (!npcTransforms.ContainsKey(state.npcId))
            {
                return;
            }

            Transform transform = npcTransforms[state.npcId];

            if (transform != null && syncPosition)
            {
                transform.position = state.position;
            }

            if (transform != null && syncRotation)
            {
                transform.rotation = Quaternion.Euler(state.rotation);
            }

            if (syncBehaviorState && npcStateMachines.ContainsKey(state.npcId))
            {
                npcStateMachines[state.npcId].RequestStateTransition(state.behaviorState);
            }

            if (syncDialogueState && npcDialogueManagers.ContainsKey(state.npcId))
            {
                // TODO: Apply dialogue state
            }
        }

        /// <summary>
        /// Get state of a specific NPC
        /// </summary>
        public NpcStateData GetNpcState(string npcId)
        {
            if (npcStates.ContainsKey(npcId))
            {
                return npcStates[npcId];
            }
            return null;
        }

        /// <summary>
        /// Get all NPC states
        /// </summary>
        public Dictionary<string, NpcStateData> GetAllNpcStates()
        {
            return new Dictionary<string, NpcStateData>(npcStates);
        }
    }

    /// <summary>
    /// NPC state data structure for synchronization
    /// </summary>
    [System.Serializable]
    public class NpcStateData
    {
        public string npcId;
        public Vector3 position;
        public Vector3 rotation;
        public string behaviorState;
        public bool isInDialogue;
        public int dialogueLineIndex;
        public float timestamp;
    }
}
