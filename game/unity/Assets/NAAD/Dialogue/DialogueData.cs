using UnityEngine;
using System.Collections.Generic;
using System;
using NAAD.AI;

namespace NAAD.Dialogue
{
    /// <summary>
    /// Represents a single node in a dialogue tree
    /// </summary>
    [Serializable]
    public class DialogueNode
    {
        public string id;
        public string speakerId; // NPC ID or "player"
        public string text;
        public DialogueEmotion emotion;
        public List<DialogueResponse> responses;
        public string nextNodeId; // For linear progression without responses

        public DialogueNode()
        {
            responses = new List<DialogueResponse>();
            emotion = DialogueEmotion.Neutral;
        }
    }

    /// <summary>
    /// Represents a player response option in dialogue
    /// </summary>
    [Serializable]
    public class DialogueResponse
    {
        public string id;
        public string text;
        public string nextNodeId;
        public List<DialogueCondition> conditions;
        public int reputationChange;
        public string relationshipFaction; // Optional faction for relationship changes
        public int relationshipChange;

        public DialogueResponse()
        {
            conditions = new List<DialogueCondition>();
            reputationChange = 0;
            relationshipChange = 0;
        }

        /// <summary>
        /// Check if this response is available based on conditions
        /// </summary>
        public bool IsAvailable(IDialogueConditionContext context)
        {
            foreach (var condition in conditions)
            {
                if (!condition.Evaluate(context))
                {
                    return false;
                }
            }
            return true;
        }
    }

    /// <summary>
    /// Base class for dialogue conditions
    /// </summary>
    [Serializable]
    public abstract class DialogueCondition
    {
        public abstract bool Evaluate(IDialogueConditionContext context);
    }

    /// <summary>
    /// Context interface for evaluating dialogue conditions
    /// </summary>
    public interface IDialogueConditionContext
    {
        int GetReputation();
        bool HasSeenDialogue(string dialogueId);
        int GetRelationship(string faction);
    }

    /// <summary>
    /// Condition requiring minimum reputation
    /// </summary>
    [Serializable]
    public class ReputationCondition : DialogueCondition
    {
        public int minReputation;

        public ReputationCondition(int minReputation = 0)
        {
            this.minReputation = minReputation;
        }

        public override bool Evaluate(IDialogueConditionContext context)
        {
            return context.GetReputation() >= minReputation;
        }
    }

    /// <summary>
    /// Condition preventing repeat dialogues
    /// </summary>
    [Serializable]
    public class SeenCondition : DialogueCondition
    {
        public string dialogueId;
        public bool mustHaveSeen;

        public SeenCondition(string dialogueId, bool mustHaveSeen = false)
        {
            this.dialogueId = dialogueId;
            this.mustHaveSeen = mustHaveSeen;
        }

        public override bool Evaluate(IDialogueConditionContext context)
        {
            bool hasSeen = context.HasSeenDialogue(dialogueId);
            return mustHaveSeen ? hasSeen : !hasSeen;
        }
    }

    /// <summary>
    /// Condition requiring minimum relationship with a faction
    /// </summary>
    [Serializable]
    public class RelationshipCondition : DialogueCondition
    {
        public string faction;
        public int minRelationship;

        public RelationshipCondition(string faction, int minRelationship = 0)
        {
            this.faction = faction;
            this.minRelationship = minRelationship;
        }

        public override bool Evaluate(IDialogueConditionContext context)
        {
            return context.GetRelationship(faction) >= minRelationship;
        }
    }
}
