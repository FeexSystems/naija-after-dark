using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using NAAD.Dialogue;
using NAAD.AI;

namespace NAAD.UI
{
    /// <summary>
    /// Manages multiple dialogue bubbles for conversation history and multi-bubble display.
    /// Provides screen-space positioning with bounds clamping and bubble pooling for performance.
    /// </summary>
    public class DialogueBubbleManager : MonoBehaviour
    {
        [SerializeField] private DialogueBubble _bubblePrefab;
        [SerializeField] private Transform _bubbleContainer;
        [SerializeField] private int _maxBubbles = 5;
        [SerializeField] private float _bubbleSpacing = 50f;
        [SerializeField] private float _bubbleLifetime = 10f;

        private Queue<DialogueBubble> _activeBubbles = new Queue<DialogueBubble>();
        private Stack<DialogueBubble> _bubblePool = new Stack<DialogueBubble>();
        private Transform _targetTransform; // NPC transform for positioning

        private void Awake()
        {
            if (_bubbleContainer == null)
            {
                _bubbleContainer = transform;
            }
        }

        /// <summary>
        /// Set the target transform (usually NPC) for bubble positioning.
        /// </summary>
        public void SetTarget(Transform target)
        {
            _targetTransform = target;
        }

        /// <summary>
        /// Show a dialogue bubble with the given text and emotion.
        /// </summary>
        public void ShowBubble(string text, DialogueEmotion emotion = DialogueEmotion.Neutral)
        {
            DialogueBubble bubble = GetBubbleFromPool();
            bubble.Initialize(text, emotion);
            bubble.transform.SetParent(_bubbleContainer, false);

            if (_targetTransform != null)
            {
                PositionBubble(bubble, _targetTransform);
            }

            _activeBubbles.Enqueue(bubble);

            // Enforce max bubbles
            while (_activeBubbles.Count > _maxBubbles)
            {
                DialogueBubble oldBubble = _activeBubbles.Dequeue();
                ReturnBubbleToPool(oldBubble);
            }

            // Auto-hide after lifetime
            StartCoroutine(HideBubbleAfterDelay(bubble, _bubbleLifetime));
        }

        /// <summary>
        /// Show a dialogue node with full bubble display.
        /// </summary>
        public void ShowDialogueNode(DialogueNode node)
        {
            if (node == null) return;

            ShowBubble(node.text, node.emotion);
        }

        /// <summary>
        /// Clear all active bubbles.
        /// </summary>
        public void ClearAllBubbles()
        {
            while (_activeBubbles.Count > 0)
            {
                DialogueBubble bubble = _activeBubbles.Dequeue();
                ReturnBubbleToPool(bubble);
            }
        }

        /// <summary>
        /// Position a bubble in screen space with bounds clamping.
        /// </summary>
        private void PositionBubble(DialogueBubble bubble, Transform target)
        {
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(target.position + Vector3.up * 2f);

            // Clamp to screen bounds
            float padding = 50f;
            screenPosition.x = Mathf.Clamp(screenPosition.x, padding, Screen.width - padding);
            screenPosition.y = Mathf.Clamp(screenPosition.y, padding, Screen.height - padding);

            // Convert back to world position for UI
            RectTransform canvasRect = _bubbleContainer as RectTransform;
            if (canvasRect != null)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect,
                    screenPosition,
                    null,
                    out Vector2 localPoint
                );
                bubble.transform.localPosition = localPoint;
            }
        }

        /// <summary>
        /// Get a bubble from the pool or create a new one.
        /// </summary>
        private DialogueBubble GetBubbleFromPool()
        {
            if (_bubblePool.Count > 0)
            {
                DialogueBubble bubble = _bubblePool.Pop();
                bubble.gameObject.SetActive(true);
                return bubble;
            }

            if (_bubblePrefab != null)
            {
                DialogueBubble bubble = Instantiate(_bubblePrefab, _bubbleContainer);
                return bubble;
            }

            Debug.LogError("[DialogueBubbleManager] No bubble prefab available");
            return null;
        }

        /// <summary>
        /// Return a bubble to the pool.
        /// </summary>
        private void ReturnBubbleToPool(DialogueBubble bubble)
        {
            if (bubble == null) return;

            bubble.gameObject.SetActive(false);
            _bubblePool.Push(bubble);
        }

        /// <summary>
        /// Hide a bubble after a delay.
        /// </summary>
        private System.Collections.IEnumerator HideBubbleAfterDelay(DialogueBubble bubble, float delay)
        {
            yield return new WaitForSeconds(delay);

            if (bubble != null && _activeBubbles.Contains(bubble))
            {
                _activeBubbles = new Queue<DialogueBubble>(_activeBubbles.Where(b => b != bubble));
                ReturnBubbleToPool(bubble);
            }
        }

        private void OnDestroy()
        {
            ClearAllBubbles();
        }
    }
}
