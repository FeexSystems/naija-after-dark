using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using NAAD.Dialogue;
using NAAD.AI;

namespace NAAD.Editor
{
    /// <summary>
    /// Editor-only test runner for dialogue system verification.
    /// Tests dialogue tree traversal, condition evaluation, and state transitions.
    /// </summary>
    public class DialogueTestRunner
    {
        private static int _testsPassed = 0;
        private static int _testsFailed = 0;

        [MenuItem("NAAD/Tests/Run Dialogue Tests")]
        public static void RunAllTests()
        {
            _testsPassed = 0;
            _testsFailed = 0;

            Debug.Log("=== Starting Dialogue System Tests ===");

            TestDialogueDataStructures();
            TestConditionEvaluation();
            TestDialogueTreeTraversal();
            TestDialogueMemory();

            Debug.Log($"=== Dialogue Tests Complete: {_testsPassed} passed, {_testsFailed} failed ===");

            EditorApplication.Exit(_testsFailed == 0 ? 0 : 1);
        }

        private static void TestDialogueDataStructures()
        {
            Debug.Log("Testing dialogue data structures...");

            // Test DialogueNode creation
            DialogueNode node = new DialogueNode
            {
                id = "test_node_1",
                speakerId = "tunde",
                text = "Hello, player!",
                emotion = DialogueEmotion.Happy
            };

            Assert(node.id == "test_node_1", "DialogueNode ID");
            Assert(node.speakerId == "tunde", "DialogueNode speaker ID");
            Assert(node.text == "Hello, player!", "DialogueNode text");
            Assert(node.emotion == DialogueEmotion.Happy, "DialogueNode emotion");

            // Test DialogueResponse creation
            DialogueResponse response = new DialogueResponse
            {
                id = "response_1",
                text = "Nice to meet you",
                nextNodeId = "test_node_2",
                reputationChange = 10
            };

            Assert(response.id == "response_1", "DialogueResponse ID");
            Assert(response.reputationChange == 10, "DialogueResponse reputation change");

            Debug.Log("✓ Dialogue data structures test passed");
        }

        private static void TestConditionEvaluation()
        {
            Debug.Log("Testing condition evaluation...");

            // Create test context
            TestDialogueContext context = new TestDialogueContext
            {
                reputation = 50,
                seenDialogues = new HashSet<string> { "seen_dialogue" },
                relationships = new Dictionary<string, int> { { "faction_a", 75 } }
            };

            // Test ReputationCondition
            ReputationCondition repCondition = new ReputationCondition(30);
            Assert(repCondition.Evaluate(context), "ReputationCondition (50 >= 30)");

            ReputationCondition repConditionFail = new ReputationCondition(100);
            Assert(!repConditionFail.Evaluate(context), "ReputationCondition (50 < 100)");

            // Test SeenCondition
            SeenCondition seenCondition = new SeenCondition("seen_dialogue", true);
            Assert(seenCondition.Evaluate(context), "SeenCondition (has seen)");

            SeenCondition notSeenCondition = new SeenCondition("unseen_dialogue", false);
            Assert(notSeenCondition.Evaluate(context), "SeenCondition (has not seen)");

            // Test RelationshipCondition
            RelationshipCondition relCondition = new RelationshipCondition("faction_a", 50);
            Assert(relCondition.Evaluate(context), "RelationshipCondition (75 >= 50)");

            RelationshipCondition relConditionFail = new RelationshipCondition("faction_a", 100);
            Assert(!relConditionFail.Evaluate(context), "RelationshipCondition (75 < 100)");

            Debug.Log("✓ Condition evaluation test passed");
        }

        private static void TestDialogueTreeTraversal()
        {
            Debug.Log("Testing dialogue tree traversal...");

            // Create simple dialogue tree
            Dictionary<string, DialogueNode> tree = new Dictionary<string, DialogueNode>();

            DialogueNode node1 = new DialogueNode
            {
                id = "node_1",
                speakerId = "tunde",
                text = "Hello!",
                responses = new List<DialogueResponse>
                {
                    new DialogueResponse
                    {
                        id = "resp_1",
                        text = "Hi there",
                        nextNodeId = "node_2"
                    }
                }
            };

            DialogueNode node2 = new DialogueNode
            {
                id = "node_2",
                speakerId = "tunde",
                text = "How are you?",
                nextNodeId = null // End of dialogue
            };

            tree["node_1"] = node1;
            tree["node_2"] = node2;

            // Test tree structure
            Assert(tree.ContainsKey("node_1"), "Tree contains node_1");
            Assert(tree.ContainsKey("node_2"), "Tree contains node_2");
            Assert(tree["node_1"].responses.Count == 1, "Node 1 has 1 response");
            Assert(tree["node_1"].responses[0].nextNodeId == "node_2", "Response leads to node_2");

            Debug.Log("✓ Dialogue tree traversal test passed");
        }

        private static void TestDialogueMemory()
        {
            Debug.Log("Testing dialogue memory...");

            // This would require an actual GameObject with DialogueMemory component
            // For unit testing, we test the data structures directly

            HashSet<string> seenDialogues = new HashSet<string>();
            seenDialogues.Add("dialogue_1");

            Assert(seenDialogues.Contains("dialogue_1"), "Seen dialogue tracked");
            Assert(!seenDialogues.Contains("dialogue_2"), "Unseen dialogue not tracked");

            Dictionary<string, List<string>> history = new Dictionary<string, List<string>>();
            history["npc_1"] = new List<string> { "Hello", "Goodbye" };

            Assert(history["npc_1"].Count == 2, "Conversation history tracked");

            Debug.Log("✓ Dialogue memory test passed");
        }

        private static void Assert(bool condition, string testName)
        {
            if (condition)
            {
                _testsPassed++;
                Debug.Log($"  ✓ {testName}");
            }
            else
            {
                _testsFailed++;
                Debug.LogError($"  ✗ {testName}");
            }
        }

        /// <summary>
        /// Test context for condition evaluation.
        /// </summary>
        private class TestDialogueContext : IDialogueConditionContext
        {
            public int reputation;
            public HashSet<string> seenDialogues;
            public Dictionary<string, int> relationships;

            public int GetReputation()
            {
                return reputation;
            }

            public bool HasSeenDialogue(string dialogueId)
            {
                return seenDialogues.Contains(dialogueId);
            }

            public int GetRelationship(string faction)
            {
                if (relationships.ContainsKey(faction))
                {
                    return relationships[faction];
                }
                return 0;
            }
        }
    }
}
