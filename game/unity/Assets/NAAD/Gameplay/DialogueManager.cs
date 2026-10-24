using UnityEngine;

namespace NAAD.Gameplay
{
    /// <summary>
    /// Minimal NPC dialogue coordinator.
    /// <para>
    /// Referenced by <c>NpcStateSync</c> (dialogue-state sync) and
    /// <c>AmbientDialogueTrigger</c> (ambient lines). The full dialogue system was
    /// never committed to this repository (git history contains no prior
    /// definition), so this class provides only the contract those call sites use.
    /// It is presentation-only and never mutates authoritative game state.
    /// </para>
    /// </summary>
    public sealed class DialogueManager : MonoBehaviour
    {
        [SerializeField] private bool _isActive;

        /// <summary>Whether a dialogue interaction is currently active.</summary>
        public bool IsActive() => _isActive;

        /// <summary>
        /// Present an ambient line. Presentation only: logs the line and marks the
        /// dialogue active so state-sync consumers observe a consistent flag.
        /// </summary>
        public void PlayAmbientLine(string line)
        {
            if (string.IsNullOrEmpty(line))
                return;

            _isActive = true;
            Debug.Log("[DialogueManager] " + line);
        }

        /// <summary>End the current dialogue interaction.</summary>
        public void EndDialogue()
        {
            _isActive = false;
        }
    }
}