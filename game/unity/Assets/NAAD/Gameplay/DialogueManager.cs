using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using NAAD.Dialogue;
using NAAD.Networking;

namespace NAAD.Gameplay
{
    /// <summary>
    /// Dialogue coordinator with branching dialogue support.
    /// <para>
    /// Provides ambient line playback (legacy) and full branching dialogue system.
    /// Presentation-only: never mutates authoritative game state directly.
    /// All authoritative mutations go through Supabase Edge Function via naad_execute_command.
    /// </para>
    /// </summary>
    public sealed class DialogueManager : MonoBehaviour, IDialogueConditionContext
    {
        [SerializeField] private bool _isActive;
        [SerializeField] private IDialogueService _dialogueService;

        // Dialogue tree state
        private DialogueNode _currentNode;
        private Dictionary<string, DialogueNode> _dialogueTree;
        private string _currentNpcId;
        private int _playerReputation = 0;
        private Dictionary<string, int> _relationships = new Dictionary<string, int>();
        private HashSet<string> _seenDialogues = new HashSet<string>();

        // Events for UI integration
        public event System.Action<DialogueNode> OnDialogueNodeChanged;
        public event System.Action<List<DialogueResponse>> OnResponsesAvailable;
        public event System.Action OnDialogueEnded;

        /// <summary>Whether a dialogue interaction is currently active.</summary>
        public bool IsActive() => _isActive;

        /// <summary>Current dialogue node.</summary>
        public DialogueNode CurrentNode => _currentNode;

        private void Awake()
        {
            _dialogueService = GetComponent<IDialogueService>();
            if (_dialogueService == null)
            {
                Debug.LogWarning("[DialogueManager] IDialogueService not found - dialogue branching will not work");
            }
        }

        /// <summary>
        /// Present an ambient line. Presentation only: logs the line and marks the
        /// dialogue active so state-sync consumers observe a consistent flag.
        /// </summary>
        public void PlayAmbientLine(string line)
        {
            if (string.IsNullOrEmpty(line))
                return;

            _isActive = true;
            Debug.Log("[DialogueManager] Ambient: " + line);
        }

        /// <summary>
        /// Start a branching dialogue with an NPC.
        /// </summary>
        public void StartDialogue(string dialogueId, string npcId)
        {
            if (string.IsNullOrEmpty(dialogueId))
            {
                Debug.LogError("[DialogueManager] Cannot start dialogue: dialogueId is null or empty");
                return;
            }

            _currentNpcId = npcId;
            _isActive = true;

            // Try to load dialogue from local tree first (for testing/development)
            if (_dialogueTree != null && _dialogueTree.ContainsKey(dialogueId))
            {
                LoadNode(dialogueId);
            }
            // Otherwise, fetch from Supabase Edge Function
            else if (_dialogueService != null)
            {
                _ = LoadDialogueAsync(dialogueId);
            }
            else
            {
                Debug.LogError("[DialogueManager] No dialogue service available and dialogue not in local tree");
                EndDialogue();
            }
        }

        private async Task LoadDialogueAsync(string dialogueId)
        {
            try
            {
                var node = await _dialogueService.GetDialogueAsync(dialogueId);
                if (node != null)
                {
                    _dialogueTree = _dialogueTree ?? new Dictionary<string, DialogueNode>();
                    _dialogueTree[dialogueId] = node;
                    LoadNode(dialogueId);
                }
                else
                {
                    Debug.LogError($"[DialogueManager] Failed to load dialogue: {dialogueId}");
                    EndDialogue();
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[DialogueManager] Dialogue load failed: {ex.Message}");
                EndDialogue();
            }
        }

        /// <summary>
        /// Load a dialogue node by ID.
        /// </summary>
        private void LoadNode(string nodeId)
        {
            if (_dialogueTree == null || !_dialogueTree.ContainsKey(nodeId))
            {
                Debug.LogError($"[DialogueManager] Node not found: {nodeId}");
                EndDialogue();
                return;
            }

            _currentNode = _dialogueTree[nodeId];
            _seenDialogues.Add(nodeId);

            Debug.Log($"[DialogueManager] Loaded node: {nodeId} - {_currentNode.text}");

            // Notify UI of new node
            OnDialogueNodeChanged?.Invoke(_currentNode);

            // If node has responses, filter and notify
            if (_currentNode.responses != null && _currentNode.responses.Count > 0)
            {
                List<DialogueResponse> availableResponses = new List<DialogueResponse>();
                foreach (var response in _currentNode.responses)
                {
                    if (response.IsAvailable(this))
                    {
                        availableResponses.Add(response);
                    }
                }
                OnResponsesAvailable?.Invoke(availableResponses);
            }
            // If node has linear progression, auto-advance
            else if (!string.IsNullOrEmpty(_currentNode.nextNodeId))
            {
                StartCoroutine(AutoAdvanceAfterDelay(_currentNode.nextNodeId, 2f));
            }
            // Otherwise, end dialogue
            else
            {
                StartCoroutine(EndDialogueAfterDelay(1f));
            }
        }

        /// <summary>
        /// Select a response from the current dialogue node.
        /// </summary>
        public void SelectResponse(int responseIndex)
        {
            if (_currentNode == null || _currentNode.responses == null)
            {
                Debug.LogError("[DialogueManager] Cannot select response: no current node or responses");
                return;
            }

            if (responseIndex < 0 || responseIndex >= _currentNode.responses.Count)
            {
                Debug.LogError($"[DialogueManager] Invalid response index: {responseIndex}");
                return;
            }

            DialogueResponse selectedResponse = _currentNode.responses[responseIndex];

            // Apply local preview of changes (server will validate and apply authoritatively)
            _playerReputation += selectedResponse.reputationChange;

            if (!string.IsNullOrEmpty(selectedResponse.relationshipFaction))
            {
                if (!_relationships.ContainsKey(selectedResponse.relationshipFaction))
                {
                    _relationships[selectedResponse.relationshipFaction] = 0;
                }
                _relationships[selectedResponse.relationshipFaction] += selectedResponse.relationshipChange;
            }

            Debug.Log($"[DialogueManager] Selected response: {selectedResponse.text}");

            // Send command to server for authoritative mutation
            if (_dialogueService != null)
            {
                _ = SendDialogueCommandAsync(selectedResponse);
            }

            // Advance to next node
            if (!string.IsNullOrEmpty(selectedResponse.nextNodeId))
            {
                LoadNode(selectedResponse.nextNodeId);
            }
            else
            {
                EndDialogue();
            }
        }

        private async Task SendDialogueCommandAsync(DialogueResponse response)
        {
            try
            {
                var success = await _dialogueService.SendDialogueCommandAsync(
                    _currentNpcId,
                    response.text,
                    response.reputationChange,
                    response.relationshipFaction,
                    response.relationshipChange);
                if (!success)
                {
                    Debug.LogWarning("[DialogueManager] Server rejected dialogue command - reverting local changes");
                    _playerReputation -= response.reputationChange;
                    if (!string.IsNullOrEmpty(response.relationshipFaction))
                    {
                        _relationships[response.relationshipFaction] -= response.relationshipChange;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[DialogueManager] Dialogue command failed: {ex.Message}");
            }
        }

        /// <summary>End the current dialogue interaction.</summary>
        public void EndDialogue()
        {
            _isActive = false;
            _currentNode = null;
            _currentNpcId = null;
            OnDialogueEnded?.Invoke();
            Debug.Log("[DialogueManager] Dialogue ended");
        }

        // IDialogueConditionContext implementation
        public int GetReputation()
        {
            return _playerReputation;
        }

        public bool HasSeenDialogue(string dialogueId)
        {
            return _seenDialogues.Contains(dialogueId);
        }

        public int GetRelationship(string faction)
        {
            if (_relationships.ContainsKey(faction))
            {
                return _relationships[faction];
            }
            return 0;
        }

        // Helper coroutines
        private System.Collections.IEnumerator AutoAdvanceAfterDelay(string nodeId, float delay)
        {
            yield return new WaitForSeconds(delay);
            LoadNode(nodeId);
        }

        private System.Collections.IEnumerator EndDialogueAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            EndDialogue();
        }

        /// <summary>
        /// Load a dialogue tree from JSON (for ScriptableObject integration).
        /// </summary>
        public void LoadDialogueTree(Dictionary<string, DialogueNode> tree)
        {
            _dialogueTree = tree;
            Debug.Log($"[DialogueManager] Loaded dialogue tree with {tree.Count} nodes");
        }
    }
}
