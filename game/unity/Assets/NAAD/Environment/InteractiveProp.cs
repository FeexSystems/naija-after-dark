using UnityEngine;
using UnityEngine.Events;

namespace NAAD.Environment
{
    /// <summary>
    /// Interactive prop system for E-key interactions with proximity triggers.
    /// Supports context-sensitive interactions based on NPC state and provides
    /// visual/audio feedback.
    /// </summary>
    public class InteractiveProp : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [SerializeField] private string interactionPrompt = "Press E to interact";
        [SerializeField] private float interactionRange = 2.5f;
        [SerializeField] private KeyCode interactionKey = KeyCode.E;
        [SerializeField] private bool requireLineOfSight = true;
        [SerializeField] private LayerMask lineOfSightLayers = -1;

        [Header("Context Sensitivity")]
        [SerializeField] private bool npcContextSensitive = true;
        [SerializeField] private string[] requiredNpcStates = new string[] { "Idle", "Task" };
        [SerializeField] private string[] blockedNpcStates = new string[] { "Conversation" };

        [Header("Feedback")]
        [SerializeField] private GameObject promptUI;
        [SerializeField] private AudioClip interactSound;
        [SerializeField] private GameObject visualFeedback;
        [SerializeField] private float feedbackDuration = 1.5f;

        [Header("Events")]
        public UnityEvent OnInteract;
        public UnityEvent<bool> OnProximityChanged;

        private Transform player;
        private bool isInRange;
        private bool canInteract;
        private AudioSource audioSource;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            if (promptUI != null)
            {
                promptUI.SetActive(false);
            }

            if (visualFeedback != null)
            {
                visualFeedback.SetActive(false);
            }
        }

        private void Start()
        {
            FindPlayer();
        }

        private void FindPlayer()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }

        private void Update()
        {
            if (player == null)
            {
                FindPlayer();
                return;
            }

            CheckProximity();
            CheckInteraction();
        }

        private void CheckProximity()
        {
            float distance = Vector3.Distance(transform.position, player.position);
            bool wasInRange = isInRange;
            isInRange = distance <= interactionRange;

            if (isInRange != wasInRange)
            {
                OnProximityChanged?.Invoke(isInRange);
                UpdatePromptVisibility();
            }
        }

        private void CheckInteraction()
        {
            if (!isInRange) return;

            canInteract = CanInteract();

            if (canInteract && Input.GetKeyDown(interactionKey))
            {
                PerformInteraction();
            }

            UpdatePromptVisibility();
        }

        private bool CanInteract()
        {
            // Check line of sight if required
            if (requireLineOfSight && !HasLineOfSight())
            {
                return false;
            }

            // Check NPC context if enabled
            if (npcContextSensitive)
            {
                return CheckNpcContext();
            }

            return true;
        }

        private bool HasLineOfSight()
        {
            if (player == null) return false;

            Vector3 direction = (player.position - transform.position).normalized;
            float distance = Vector3.Distance(transform.position, player.position);

            return !Physics.Raycast(transform.position, direction, distance, lineOfSightLayers);
        }

        private bool CheckNpcContext()
        {
            // Find nearby NPCs
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, interactionRange * 1.5f);
            
            foreach (var collider in hitColliders)
            {
                var npcStateMachine = collider.GetComponent<BehaviorStateMachine>();
                if (npcStateMachine != null)
                {
                    var currentStateObj = npcStateMachine.GetCurrentState();
                    string currentState = currentStateObj != null ? currentStateObj.GetType().Name : string.Empty;

                    // Check if current state is blocked
                    foreach (string blockedState in blockedNpcStates)
                    {
                        if (currentState == blockedState)
                        {
                            return false;
                        }
                    }

                    // Check if current state is required (if any specified)
                    if (requiredNpcStates.Length > 0)
                    {
                        bool stateAllowed = false;
                        foreach (string requiredState in requiredNpcStates)
                        {
                            if (currentState == requiredState)
                            {
                                stateAllowed = true;
                                break;
                            }
                        }
                        if (!stateAllowed)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private void PerformInteraction()
        {
            OnInteract?.Invoke();
            PlayFeedback();
        }

        private void PlayFeedback()
        {
            // Audio feedback
            if (interactSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(interactSound);
            }

            // Visual feedback
            if (visualFeedback != null)
            {
                visualFeedback.SetActive(true);
                CancelInvoke(nameof(HideVisualFeedback));
                Invoke(nameof(HideVisualFeedback), feedbackDuration);
            }
        }

        private void HideVisualFeedback()
        {
            if (visualFeedback != null)
            {
                visualFeedback.SetActive(false);
            }
        }

        private void UpdatePromptVisibility()
        {
            if (promptUI != null)
            {
                promptUI.SetActive(isInRange && canInteract);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }

        /// <summary>
        /// Public method to trigger interaction from external scripts
        /// </summary>
        public void ForceInteract()
        {
            PerformInteraction();
        }

        /// <summary>
        /// Check if player is currently in range
        /// </summary>
        public bool IsPlayerInRange()
        {
            return isInRange;
        }

        /// <summary>
        /// Check if interaction is currently possible
        /// </summary>
        public bool CanPlayerInteract()
        {
            return canInteract;
        }
    }
}
