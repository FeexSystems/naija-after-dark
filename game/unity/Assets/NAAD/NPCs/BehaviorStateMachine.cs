using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// State machine for NPC behaviors, managing transitions between different behavior states.
/// </summary>
public class BehaviorStateMachine : MonoBehaviour
{
    [Header("State Machine Settings")]
    [Tooltip("Current active state of the NPC")]
    [SerializeField] protected BehaviorState currentState;

    [Tooltip("All possible states the NPC can transition between")]
    [SerializeField] protected List<BehaviorState> availableStates = new List<BehaviorState>();

    [Tooltip("Current state timer for time-based state transitions")]
    [SerializeField] protected float stateTimer;

    [Tooltip("Time in current state before considering transition")]
    [SerializeField] protected float stateDuration = 5f;

    protected Dictionary<string, BehaviorState> stateLookup = new Dictionary<string, BehaviorState>();

    protected virtual void Awake()
    {
        foreach (var state in availableStates)
        {
            if (state != null)
            {
                stateLookup[state.GetType().Name] = state;
            }
        }

        if (currentState == null && availableStates.Count > 0)
        {
            currentState = availableStates[0];
            currentState?.Enter(this);
        }
    }

    protected virtual void Update()
    {
        if (currentState == null) return;
        stateTimer += Time.deltaTime;

        if (stateTimer >= stateDuration)
        {
            stateTimer = 0f;
            RequestStateTransition();
        }

        currentState?.UpdateState(this);
    }

    public void RequestStateTransition(string stateTypeName)
    {
        if (stateLookup.TryGetValue(stateTypeName, out var newState))
        {
            TransitionToState(newState);
        }
        else
        {
            Debug.LogWarning($"State type '{stateTypeName}' not found.");
        }
    }

    protected void TransitionToState(BehaviorState newState)
    {
        currentState?.Exit(this);
        currentState = newState;
        stateTimer = 0f;
        currentState?.Enter(this);
    }

    public void SetStateDuration(float duration)
    {
        stateDuration = duration;
    }

    public BehaviorState GetCurrentState() => currentState;

    public bool IsInState(string stateType)
    {
        return currentState != null && currentState.GetType().Name == stateType;
    }

    public void AddState(BehaviorState state)
    {
        if (state != null && !availableStates.Contains(state))
        {
            availableStates.Add(state);
            stateLookup[state.GetType().Name] = state;
        }
    }

    public void RemoveState(string stateTypeName)
    {
        if (stateLookup.Remove(stateTypeName))
        {
            availableStates.RemoveAll(s => s.GetType().Name == stateTypeName);
        }
    }
}