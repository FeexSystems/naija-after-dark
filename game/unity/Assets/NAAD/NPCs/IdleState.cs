using UnityEngine;

[CreateAssetMenu(fileName = "IdleState", menu = "NAAD/Behavior/Idle")]
public class IdleState : BehaviorState
{
    [Header("Idle State Settings")]
    [Tooltip("Speed at which NPC wanders")]
    [SerializeField] protected float wanderSpeed = 2f;

    [Tooltip("Radius of wander area around NPC")]
    [SerializeField] protected float wanderRadius = 5f;

    [Tooltip("Minimum time between direction changes")]
    [SerializeField] protected float minDirectionChangeTime = 1f;

    [Tooltip("Maximum time between direction changes")]
    [SerializeField] protected float maxDirectionChangeTime = 3f;

    private float directionChangeTimer;

    public override void Enter(BehaviorStateMachine stateMachine)
    {
        base.Enter(stateMachine);
        directionChangeTimer = Random.Range(minDirectionChangeTime, maxDirectionChangeTime);
        Debug.Log($"[BehaviorState] {stateName} entered. NPC will wander.");
    }

    public override void UpdateState(BehaviorStateMachine stateMachine)
    {
        base.UpdateState(stateMachine);
        directionChangeTimer -= Time.deltaTime;

        if (directionChangeTimer <= 0f)
        {
            directionChangeTimer = Random.Range(minDirectionChangeTime, maxDirectionChangeTime);
            Debug.Log($"[BehaviorState] {stateName}: Changing direction.");
        }
    }

    public override void Exit(BehaviorStateMachine stateMachine)
    {
        base.Exit(stateMachine);
        Debug.Log($"[BehaviorState] {stateName} exited.");
    }
}