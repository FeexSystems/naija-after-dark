using UnityEngine;
using NAAD.Networking;

namespace NAAD.Environment
{
    /// <summary>
    /// Triggers ambient dialogue based on player actions and environmental context.
    /// NPCs can comment on player interactions, time of day, or nearby events.
    /// </summary>
    public class AmbientDialogueTrigger : MonoBehaviour
    {
        [Header("Trigger Settings")]
        [SerializeField] private string triggerTag = "AmbientTrigger";
        [SerializeField] private float triggerRadius = 5f;
        [SerializeField] private float cooldownBetweenTriggers = 30f;

        [Header("Dialogue Lines")]
        [SerializeField] private string[] ambientLines;
        [SerializeField] private bool randomizeOrder = true;

        [Header("NPC Assignment")]
        [SerializeField] private GameObject targetNpc;
        [SerializeField] private bool findNearestNpc = true;

        [Header("Conditions")]
        [SerializeField] private bool requirePlayerAction = true;
        [SerializeField] private string[] requiredPlayerTags = new string[] { "Player" };

        private float lastTriggerTime;
        private int lastLineIndex = -1;
        private List<int> availableLines = new List<int>();

        private void Start()
        {
            InitializeLineIndices();
        }

        private void InitializeLineIndices()
        {
            availableLines.Clear();
            for (int i = 0; i < ambientLines.Length; i++)
            {
                availableLines.Add(i);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!CanTrigger(other))
            {
                return;
            }

            TriggerAmbientDialogue();
        }

        private bool CanTrigger(Collider other)
        {
            // Check cooldown
            if (Time.time - lastTriggerTime < cooldownBetweenTriggers)
            {
                return false;
            }

            // Check if player action is required
            if (requirePlayerAction)
            {
                bool isPlayer = false;
                foreach (string tag in requiredPlayerTags)
                {
                    if (other.CompareTag(tag))
                    {
                        isPlayer = true;
                        break;
                    }
                }
                if (!isPlayer)
                {
                    return false;
                }
            }

            return true;
        }

        private void TriggerAmbientDialogue()
        {
            // Find target NPC
            GameObject npc = targetNpc;
            if (findNearestNpc || npc == null)
            {
                npc = FindNearestNpc();
            }

            if (npc == null)
            {
                return;
            }

            // Get dialogue line
            string line = GetNextLine();
            if (string.IsNullOrEmpty(line))
            {
                return;
            }

            // Trigger dialogue on NPC
            var dialogueManager = npc.GetComponent<IDialogueService>();
            if (dialogueManager != null)
            {
                dialogueManager.TalkToTundeAsync(line, System.Guid.NewGuid().ToString());
            }

            lastTriggerTime = Time.time;
        }

        private GameObject FindNearestNpc()
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, triggerRadius);
            GameObject nearestNpc = null;
            float nearestDistance = Mathf.Infinity;

            foreach (var collider in hitColliders)
            {
                if (collider.CompareTag("NPC"))
                {
                    float distance = Vector3.Distance(transform.position, collider.transform.position);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestNpc = collider.gameObject;
                    }
                }
            }

            return nearestNpc;
        }

        private string GetNextLine()
        {
            if (ambientLines == null || ambientLines.Length == 0)
            {
                return null;
            }

            if (randomizeOrder)
            {
                if (availableLines.Count == 0)
                {
                    InitializeLineIndices();
                }

                if (availableLines.Count > 0)
                {
                    int randomIndex = Random.Range(0, availableLines.Count);
                    int lineIndex = availableLines[randomIndex];
                    availableLines.RemoveAt(randomIndex);
                    return ambientLines[lineIndex];
                }
            }
            else
            {
                lastLineIndex = (lastLineIndex + 1) % ambientLines.Length;
                return ambientLines[lastLineIndex];
            }

            return null;
        }

        /// <summary>
        /// Manually trigger ambient dialogue
        /// </summary>
        public void ForceTrigger()
        {
            lastTriggerTime = 0f;
            TriggerAmbientDialogue();
        }

        /// <summary>
        /// Reset cooldown
        /// </summary>
        public void ResetCooldown()
        {
            lastTriggerTime = 0f;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, triggerRadius);
        }
    }
}
