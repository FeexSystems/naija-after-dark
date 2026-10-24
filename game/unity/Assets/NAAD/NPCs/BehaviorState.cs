using UnityEngine;

[CreateAssetMenu(fileName = "NewBehaviorState", menuName = "NAAD/Behavior/State")]
public class BehaviorState : ScriptableObject
{
    [Header("State Settings")]
    [Tooltip("Name identifier for this state")]
    [SerializeField] protected string stateName = "New State";

    [Tooltip("Duration this state should run before auto-transition (seconds). Set to 0 for manual transitions only.")]
    [SerializeField] protected float defaultDuration = 5f;

    [Tooltip("Gizmos color for editor visualization")]
    [SerializeField] protected Color gizmosColor = Color.cyan;

    public string StateName => stateName;
    public float DefaultDuration => defaultDuration;
    public Color GizmosColor => gizmosColor;

    public virtual void Enter(BehaviorStateMachine stateMachine) { }

    public virtual void UpdateState(BehaviorStateMachine stateMachine) { }

    public virtual void Exit(BehaviorStateMachine stateMachine) { }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = gizmosColor;
        Gizmos.DrawSphere(Vector3.zero, 0.5f);
        Handles.Label(Vector3.zero, stateName);
    }
}