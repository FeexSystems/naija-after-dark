using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace NAAD.UI
{
    /// <summary>
    /// Enhanced conversation UI with typing effect, history scroll, and avatar portraits.
    /// Integrates with DialogueManager and DialogueBubbleManager for immersive dialogue.
    /// </summary>
    public class ConversationUI : MonoBehaviour
    {
        [Header("Dialogue Display")]
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private Image npcPortrait;
        [SerializeField] private Sprite defaultPortrait;

        [Header("Typing Effect")]
        [SerializeField] private float typingSpeed = 0.03f;
        [SerializeField] private bool skipTypingOnInteract = true;
        [SerializeField] private KeyCode skipKey = KeyCode.Space;

        [Header("Response Buttons")]
        [SerializeField] private GameObject responseContainer;
        [SerializeField] private GameObject responseButtonPrefab;
        [SerializeField] private float responseButtonSpacing = 10f;

        [Header("History Scroll")]
        [SerializeField] private GameObject historyPanel;
        [SerializeField] private ScrollRect historyScroll;
        [SerializeField] private GameObject historyEntryPrefab;
        [SerializeField] private int maxHistoryEntries = 10;

        [Header("Emotion Indicators")]
        [SerializeField] private Image emotionIcon;
        [SerializeField] private Sprite[] emotionSprites; // Index: 0=neutral, 1=happy, 2=sad, 3=surprised, 4=angry

        [Header("Relationship Meter")]
        [SerializeField] private Slider relationshipSlider;
        [SerializeField] private TextMeshProUGUI relationshipText;

        private Coroutine typingCoroutine;
        private bool isTyping;
        private List<GameObject> historyEntries = new List<GameObject>();
        private List<GameObject> responseButtons = new List<GameObject>();

        private void Awake()
        {
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
            }

            if (historyPanel != null)
            {
                historyPanel.SetActive(false);
            }

            if (responseContainer != null)
            {
                responseContainer.SetActive(false);
            }

            if (emotionIcon != null)
            {
                emotionIcon.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            // Skip typing effect on key press
            if (isTyping && skipTypingOnInteract && Input.GetKeyDown(skipKey))
            {
                SkipTyping();
            }
        }

        /// <summary>
        /// Show dialogue with typing effect
        /// </summary>
        public void ShowDialogue(string text, Sprite portrait = null, int emotionIndex = 0)
        {
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(true);
            }

            // Set portrait
            if (npcPortrait != null)
            {
                npcPortrait.sprite = portrait != null ? portrait : defaultPortrait;
            }

            // Set emotion icon
            if (emotionIcon != null && emotionSprites != null && emotionIndex >= 0 && emotionIndex < emotionSprites.Length)
            {
                emotionIcon.sprite = emotionSprites[emotionIndex];
                emotionIcon.gameObject.SetActive(true);
            }

            // Start typing effect
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }
            typingCoroutine = StartCoroutine(TypeText(text));

            // Add to history
            AddToHistory(text, portrait, emotionIndex);
        }

        /// <summary>
        /// Typing effect coroutine
        /// </summary>
        private IEnumerator TypeText(string text)
        {
            isTyping = true;
            dialogueText.text = "";

            foreach (char c in text)
            {
                dialogueText.text += c;
                yield return new WaitForSeconds(typingSpeed);
            }

            isTyping = false;
        }

        /// <summary>
        /// Skip typing effect and show full text immediately
        /// </summary>
        private void SkipTyping()
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }
            isTyping = false;
            // The full text should be set by the caller or stored
        }

        /// <summary>
        /// Set dialogue text immediately (skip typing effect)
        /// </summary>
        public void SetDialogueTextImmediate(string text)
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }
            isTyping = false;
            dialogueText.text = text;
        }

        /// <summary>
        /// Show response options
        /// </summary>
        public void ShowResponses(string[] responses, System.Action<int> onResponseSelected)
        {
            ClearResponseButtons();

            if (responseContainer != null)
            {
                responseContainer.SetActive(true);
            }

            for (int i = 0; i < responses.Length; i++)
            {
                int index = i;
                GameObject buttonObj = Instantiate(responseButtonPrefab, responseContainer.transform);
                Button button = buttonObj.GetComponent<Button>();
                TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

                if (buttonText != null)
                {
                    buttonText.text = responses[i];
                }

                if (button != null)
                {
                    button.onClick.AddListener(() => {
                        onResponseSelected?.Invoke(index);
                        ClearResponseButtons();
                    });
                }

                responseButtons.Add(buttonObj);
            }
        }

        /// <summary>
        /// Clear all response buttons
        /// </summary>
        private void ClearResponseButtons()
        {
            foreach (GameObject button in responseButtons)
            {
                if (button != null)
                {
                    Destroy(button);
                }
            }
            responseButtons.Clear();

            if (responseContainer != null)
            {
                responseContainer.SetActive(false);
            }
        }

        /// <summary>
        /// Add dialogue entry to history
        /// </summary>
        private void AddToHistory(string text, Sprite portrait, int emotionIndex)
        {
            if (historyPanel == null || historyScroll == null || historyEntryPrefab == null)
            {
                return;
            }

            // Show history panel if hidden
            historyPanel.SetActive(true);

            // Create history entry
            GameObject entry = Instantiate(historyEntryPrefab, historyScroll.content);
            HistoryEntry entryScript = entry.GetComponent<HistoryEntry>();

            if (entryScript != null)
            {
                entryScript.Setup(text, portrait, emotionIndex);
            }

            historyEntries.Add(entry);

            // Limit history entries
            while (historyEntries.Count > maxHistoryEntries)
            {
                GameObject oldEntry = historyEntries[0];
                historyEntries.RemoveAt(0);
                Destroy(oldEntry);
            }

            // Scroll to bottom
            StartCoroutine(ScrollToBottom());
        }

        /// <summary>
        /// Scroll history to bottom
        /// </summary>
        private IEnumerator ScrollToBottom()
        {
            yield return new WaitForEndOfFrame();
            if (historyScroll != null)
            {
                historyScroll.verticalNormalizedPosition = 0f;
            }
        }

        /// <summary>
        /// Update relationship meter
        /// </summary>
        public void UpdateRelationship(float value, float maxValue = 100f)
        {
            if (relationshipSlider != null)
            {
                relationshipSlider.value = Mathf.Clamp01(value / maxValue);
            }

            if (relationshipText != null)
            {
                relationshipText.text = $"{value:F0}/{maxValue:F0}";
            }
        }

        /// <summary>
        /// Hide dialogue UI
        /// </summary>
        public void HideDialogue()
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }
            isTyping = false;

            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
            }

            ClearResponseButtons();

            if (emotionIcon != null)
            {
                emotionIcon.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Toggle history panel visibility
        /// </summary>
        public void ToggleHistory()
        {
            if (historyPanel != null)
            {
                historyPanel.SetActive(!historyPanel.activeSelf);
            }
        }

        /// <summary>
        /// Clear all history entries
        /// </summary>
        public void ClearHistory()
        {
            foreach (GameObject entry in historyEntries)
            {
                if (entry != null)
                {
                    Destroy(entry);
                }
            }
            historyEntries.Clear();

            if (historyPanel != null)
            {
                historyPanel.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            ClearResponseButtons();
            ClearHistory();
        }
    }

    /// <summary>
    /// History entry component for dialogue history
    /// </summary>
    public class HistoryEntry : MonoBehaviour
    {
        [SerializeField] private Image portraitImage;
        [SerializeField] private TextMeshProUGUI textComponent;
        [SerializeField] private Image emotionIcon;

        public void Setup(string text, Sprite portrait, int emotionIndex)
        {
            if (textComponent != null)
            {
                textComponent.text = text;
            }

            if (portraitImage != null)
            {
                portraitImage.sprite = portrait;
                portraitImage.gameObject.SetActive(portrait != null);
            }

            if (emotionIcon != null)
            {
                // Could set emotion icon here if needed
                emotionIcon.gameObject.SetActive(false);
            }
        }
    }
}
