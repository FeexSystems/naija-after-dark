using UnityEngine;
using UnityEditor;

namespace NAAD.Editor
{
    /// <summary>
    /// Test component for verifying player rig animation functionality.
    /// Attach this to a character prefab to test locomotion, phone interaction, and social greeting.
    /// 
    /// Usage:
    /// 1. Attach this component to a character GameObject with Animator
    /// 2. Ensure CharacterAnimationController and AnimationStateManager are also attached
    /// 3. Use the context menu (right-click component) to trigger test animations
    /// 4. Observe the console for state change logs
    /// </summary>
    public class AnimationTestComponent : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private NAAD.Animation.AnimationStateManager _stateManager;
        [SerializeField] private Animator _animator;

        [Header("Test Settings")]
        [SerializeField] private bool _enableDebugLogging = true;
        [SerializeField] private float _testWalkSpeed = 1.0f;
        [SerializeField] private float _testRunSpeed = 1.5f;

        private void Awake()
        {
            _stateManager = GetComponent<NAAD.Animation.AnimationStateManager>();
            _animator = GetComponent<Animator>();

            if (_stateManager == null)
            {
                Debug.LogError("[AnimationTestComponent] AnimationStateManager not found on this GameObject");
            }

            if (_animator == null)
            {
                Debug.LogError("[AnimationTestComponent] Animator not found on this GameObject");
            }

            if (_stateManager != null)
            {
                _stateManager.OnStateChanged += OnStateChanged;
            }
        }

        private void OnDestroy()
        {
            if (_stateManager != null)
            {
                _stateManager.OnStateChanged -= OnStateChanged;
            }
        }

        private void OnStateChanged(NAAD.Animation.AnimationCommandType previous, NAAD.Animation.AnimationCommandType current)
        {
            if (_enableDebugLogging)
            {
                Debug.Log($"[AnimationTest] State changed: {previous} → {current}");
            }
        }

        #region Context Menu Tests

        [ContextMenu("Test/Idle")]
        public void TestIdle()
        {
            if (_stateManager != null)
            {
                _stateManager.SetIdle();
                Debug.Log("[AnimationTest] Triggered: Idle");
            }
        }

        [ContextMenu("Test/Walk")]
        public void TestWalk()
        {
            if (_stateManager != null)
            {
                _stateManager.SetWalk(_testWalkSpeed);
                Debug.Log($"[AnimationTest] Triggered: Walk (speed: {_testWalkSpeed})");
            }
        }

        [ContextMenu("Test/Run")]
        public void TestRun()
        {
            if (_stateManager != null)
            {
                _stateManager.SetRun(_testRunSpeed);
                Debug.Log($"[AnimationTest] Triggered: Run (speed: {_testRunSpeed})");
            }
        }

        [ContextMenu("Test/Stop")]
        public void TestStop()
        {
            if (_stateManager != null)
            {
                _stateManager.Stop();
                Debug.Log("[AnimationTest] Triggered: Stop");
            }
        }

        [ContextMenu("Test/Turn Left")]
        public void TestTurnLeft()
        {
            if (_stateManager != null)
            {
                _stateManager.TurnLeft();
                Debug.Log("[AnimationTest] Triggered: Turn Left");
            }
        }

        [ContextMenu("Test/Turn Right")]
        public void TestTurnRight()
        {
            if (_stateManager != null)
            {
                _stateManager.TurnRight();
                Debug.Log("[AnimationTest] Triggered: Turn Right");
            }
        }

        [ContextMenu("Test/Greeting")]
        public void TestGreeting()
        {
            if (_stateManager != null)
            {
                _stateManager.Greet();
                Debug.Log("[AnimationTest] Triggered: Greeting");
            }
        }

        [ContextMenu("Test/Phone/Retrieve")]
        public void TestPhoneRetrieve()
        {
            if (_stateManager != null)
            {
                _stateManager.RetrievePhone();
                Debug.Log("[AnimationTest] Triggered: Phone Retrieve");
            }
        }

        [ContextMenu("Test/Phone/Hold")]
        public void TestPhoneHold()
        {
            if (_stateManager != null)
            {
                _stateManager.HoldPhone();
                Debug.Log("[AnimationTest] Triggered: Phone Hold");
            }
        }

        [ContextMenu("Test/Phone/Browse")]
        public void TestPhoneBrowse()
        {
            if (_stateManager != null)
            {
                _stateManager.BrowsePhone();
                Debug.Log("[AnimationTest] Triggered: Phone Browse");
            }
        }

        [ContextMenu("Test/Phone/Put Away")]
        public void TestPhonePutAway()
        {
            if (_stateManager != null)
            {
                _stateManager.PutAwayPhone();
                Debug.Log("[AnimationTest] Triggered: Phone Put Away");
            }
        }

        [ContextMenu("Test/Full Sequence/Phone Interaction")]
        public void TestPhoneSequence()
        {
            StartCoroutine(PhoneInteractionSequence());
        }

        [ContextMenu("Test/Full Sequence/Locomotion")]
        public void TestLocomotionSequence()
        {
            StartCoroutine(LocomotionSequence());
        }

        [ContextMenu("Test/Full Sequence/Social Greeting")]
        public void TestSocialSequence()
        {
            StartCoroutine(SocialSequence());
        }

        [ContextMenu("Test/Full Sequence/Complete MVP Test")]
        public void TestCompleteMVP()
        {
            StartCoroutine(CompleteMVPTest());
        }

        #endregion

        #region Test Sequences

        private System.Collections.IEnumerator PhoneInteractionSequence()
        {
            Debug.Log("[AnimationTest] Starting Phone Interaction Sequence");

            // Start from idle
            _stateManager.SetIdle();
            yield return new WaitForSeconds(1.0f);

            // Retrieve phone
            _stateManager.RetrievePhone();
            yield return new WaitForSeconds(2.0f);

            // Hold phone
            _stateManager.HoldPhone();
            yield return new WaitForSeconds(1.0f);

            // Browse phone
            _stateManager.BrowsePhone();
            yield return new WaitForSeconds(3.0f);

            // Put away phone
            _stateManager.PutAwayPhone();
            yield return new WaitForSeconds(2.0f);

            Debug.Log("[AnimationTest] Phone Interaction Sequence Complete");
        }

        private System.Collections.IEnumerator LocomotionSequence()
        {
            Debug.Log("[AnimationTest] Starting Locomotion Sequence");

            // Start idle
            _stateManager.SetIdle();
            yield return new WaitForSeconds(1.0f);

            // Walk
            _stateManager.SetWalk(_testWalkSpeed);
            yield return new WaitForSeconds(2.0f);

            // Run
            _stateManager.SetRun(_testRunSpeed);
            yield return new WaitForSeconds(2.0f);

            // Walk again
            _stateManager.SetWalk(_testWalkSpeed);
            yield return new WaitForSeconds(1.0f);

            // Stop
            _stateManager.Stop();
            yield return new WaitForSeconds(1.0f);

            // Turn
            _stateManager.TurnLeft();
            yield return new WaitForSeconds(1.0f);

            _stateManager.TurnRight();
            yield return new WaitForSeconds(1.0f);

            Debug.Log("[AnimationTest] Locomotion Sequence Complete");
        }

        private System.Collections.IEnumerator SocialSequence()
        {
            Debug.Log("[AnimationTest] Starting Social Sequence");

            // Start idle
            _stateManager.SetIdle();
            yield return new WaitForSeconds(1.0f);

            // Greeting
            _stateManager.Greet();
            yield return new WaitForSeconds(3.0f);

            // Conversation
            _stateManager.StartConversation();
            yield return new WaitForSeconds(3.0f);

            // Listening
            _stateManager.StartListening();
            yield return new WaitForSeconds(2.0f);

            // Laugh
            _stateManager.Laugh();
            yield return new WaitForSeconds(2.0f);

            // Celebrate
            _stateManager.Celebrate();
            yield return new WaitForSeconds(2.0f);

            Debug.Log("[AnimationTest] Social Sequence Complete");
        }

        private System.Collections.IEnumerator CompleteMVPTest()
        {
            Debug.Log("[AnimationTest] Starting Complete MVP Test");
            Debug.Log("[AnimationTest] This test covers: Locomotion, Phone Interaction, Social Greeting");

            // 1. Locomotion test
            yield return LocomotionSequence();
            yield return new WaitForSeconds(1.0f);

            // 2. Phone interaction test
            yield return PhoneInteractionSequence();
            yield return new WaitForSeconds(1.0f);

            // 3. Social greeting test
            _stateManager.Greet();
            yield return new WaitForSeconds(3.0f);

            // Return to idle
            _stateManager.SetIdle();
            yield return new WaitForSeconds(1.0f);

            Debug.Log("[AnimationTest] Complete MVP Test Finished");
            Debug.Log("[AnimationTest] Verify:");
            Debug.Log("  - All animations played smoothly");
            Debug.Log("  - Transitions were clean");
            Debug.Log("  - Character returned to idle state");
            Debug.Log("  - No animation locking occurred");
        }

        #endregion

        #region Editor GUI

#if UNITY_EDITOR
        [CustomEditor(typeof(AnimationTestComponent))]
        public class AnimationTestComponentEditor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                DrawDefaultInspector();

                var component = (AnimationTestComponent)target;

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Quick Tests", EditorStyles.boldLabel);

                if (GUILayout.Button("Test Idle"))
                    component.TestIdle();

                if (GUILayout.Button("Test Walk"))
                    component.TestWalk();

                if (GUILayout.Button("Test Run"))
                    component.TestRun();

                if (GUILayout.Button("Test Stop"))
                    component.TestStop();

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Phone Tests", EditorStyles.boldLabel);

                if (GUILayout.Button("Phone: Retrieve"))
                    component.TestPhoneRetrieve();

                if (GUILayout.Button("Phone: Hold"))
                    component.TestPhoneHold();

                if (GUILayout.Button("Phone: Browse"))
                    component.TestPhoneBrowse();

                if (GUILayout.Button("Phone: Put Away"))
                    component.TestPhonePutAway();

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Social Tests", EditorStyles.boldLabel);

                if (GUILayout.Button("Test Greeting"))
                    component.TestGreeting();

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Full Sequences", EditorStyles.boldLabel);

                if (GUILayout.Button("Phone Interaction Sequence"))
                    component.TestPhoneSequence();

                if (GUILayout.Button("Locomotion Sequence"))
                    component.TestLocomotionSequence();

                if (GUILayout.Button("Social Sequence"))
                    component.TestSocialSequence();

                if (GUILayout.Button("COMPLETE MVP TEST"))
                {
                    GUI.backgroundColor = Color.green;
                    component.TestCompleteMVP();
                    GUI.backgroundColor = Color.white;
                }
            }
        }
#endif

        #endregion
    }
}
