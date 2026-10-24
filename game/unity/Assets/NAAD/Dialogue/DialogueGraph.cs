using UnityEngine;
using System.Collections.Generic;
using NAAD.AI;

namespace NAAD.Dialogue
{
    /// <summary>
    /// ScriptableObject for authoring dialogue trees in Unity Editor.
    /// This provides a designer-friendly interface for creating branching dialogue systems.
    /// Export to JSON for version control and runtime loading.
    /// </summary>
    [CreateAssetMenu(fileName = "DialogueGraph", menuName = "NAAD/Dialogue Graph")]
    public class DialogueGraph : ScriptableObject
    {
        [SerializeField] private string _graphId;
        [SerializeField] private string _graphName;
        [SerializeField] private List<DialogueNodeData> _nodes = new List<DialogueNodeData>();

        public string GraphId => _graphId;
        public string GraphName => _graphName;
        public List<DialogueNodeData> Nodes => _nodes;

        /// <summary>
        /// Get a node by ID.
        /// </summary>
        public DialogueNodeData GetNode(string nodeId)
        {
            return _nodes.Find(n => n.nodeId == nodeId);
        }

        /// <summary>
        /// Add a new node to the graph.
        /// </summary>
        public void AddNode(DialogueNodeData node)
        {
            if (node != null && !string.IsNullOrEmpty(node.nodeId))
            {
                _nodes.Add(node);
            }
        }

        /// <summary>
        /// Remove a node from the graph.
        /// </summary>
        public void RemoveNode(string nodeId)
        {
            _nodes.RemoveAll(n => n.nodeId == nodeId);
        }

        /// <summary>
        /// Convert to runtime DialogueNode dictionary.
        /// </summary>
        public Dictionary<string, DialogueNode> ToRuntimeDictionary()
        {
            Dictionary<string, DialogueNode> dict = new Dictionary<string, DialogueNode>();

            foreach (var nodeData in _nodes)
            {
                DialogueNode node = new DialogueNode
                {
                    id = nodeData.nodeId,
                    speakerId = nodeData.speakerId,
                    text = nodeData.text,
                    emotion = nodeData.emotion,
                    nextNodeId = nodeData.nextNodeId,
                    responses = new List<DialogueResponse>()
                };

                foreach (var responseData in nodeData.responses)
                {
                    DialogueResponse response = new DialogueResponse
                    {
                        id = responseData.responseId,
                        text = responseData.text,
                        nextNodeId = responseData.nextNodeId,
                        reputationChange = responseData.reputationChange,
                        relationshipFaction = responseData.relationshipFaction,
                        relationshipChange = responseData.relationshipChange,
                        conditions = new List<DialogueCondition>()
                    };

                    foreach (var conditionData in responseData.conditions)
                    {
                        if (conditionData.type == ConditionType.Reputation)
                        {
                            response.conditions.Add(new ReputationCondition(conditionData.value));
                        }
                        else if (conditionData.type == ConditionType.Seen)
                        {
                            response.conditions.Add(new SeenCondition(conditionData.dialogueId, conditionData.mustHaveSeen));
                        }
                        else if (conditionData.type == ConditionType.Relationship)
                        {
                            response.conditions.Add(new RelationshipCondition(conditionData.faction, conditionData.value));
                        }
                    }

                    node.responses.Add(response);
                }

                dict[node.id] = node;
            }

            return dict;
        }

        /// <summary>
        /// Export graph to JSON for version control.
        /// </summary>
        public string ToJson()
        {
            return JsonUtility.ToJson(this, true);
        }

        /// <summary>
        /// Import graph from JSON.
        /// </summary>
        public void FromJson(string json)
        {
            JsonUtility.FromJsonOverwrite(json, this);
        }
    }

    /// <summary>
    /// Serializable data for dialogue nodes (for ScriptableObject storage).
    /// </summary>
    [System.Serializable]
    public class DialogueNodeData
    {
        public string nodeId;
        public string speakerId;
        [TextArea(3, 5)]
        public string text;
        public DialogueEmotion emotion;
        public string nextNodeId;
        public List<DialogueResponseData> responses = new List<DialogueResponseData>();
    }

    /// <summary>
    /// Serializable data for dialogue responses.
    /// </summary>
    [System.Serializable]
    public class DialogueResponseData
    {
        public string responseId;
        [TextArea(2, 4)]
        public string text;
        public string nextNodeId;
        public int reputationChange;
        public string relationshipFaction;
        public int relationshipChange;
        public List<DialogueConditionData> conditions = new List<DialogueConditionData>();
    }

    /// <summary>
    /// Serializable data for dialogue conditions.
    /// </summary>
    [System.Serializable]
    public class DialogueConditionData
    {
        public ConditionType type;
        public int value;
        public string dialogueId;
        public string faction;
        public bool mustHaveSeen;
    }

    /// <summary>
    /// Condition type enum.
    /// </summary>
    public enum ConditionType
    {
        Reputation,
        Seen,
        Relationship
    }
}
