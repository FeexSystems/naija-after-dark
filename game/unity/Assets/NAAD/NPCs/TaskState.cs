using UnityEngine;

[CreateAssetMenu(fileName = "TaskState", menu = "NAAD/Behavior/Task")]
public class TaskState : BehaviorState
{
    [Header("Task State Settings")]
    [Tooltip("Name of the task this NPC is performing")]
    [SerializeField] protected string taskName = "Generic Task";

    [Tooltip("Duration to complete this task (seconds)")]
    [SerializeField] protected float taskDuration = 10f;

    [Tooltip("Success chance for the task")]
    [SerializeField] protected float successChance = 0.8f;

    private float taskTimer;
    private bool taskCompleted;

    public override void Enter(BehaviorStateMachine stateMachine)
    {
        base.Enter(stateMachine);
        taskTimer = 0f;
        taskCompleted = false;
        Debug.Log($"[BehaviorState] {stateName}: Starting task '{taskName}'.");
    }

    public override void UpdateState(BehaviorStateMachine stateMachine)
    {
        base.UpdateState(stateMachine);
        if (!taskCompleted)
        {
            taskTimer += Time.deltaTime;
            if (taskTimer >= taskDuration)
            {
                taskCompleted = Random.value <= successChance;
                Debug.Log($"[BehaviorState] {stateName}: Task completed. Success: {taskCompleted}.");

                if (taskCompleted)
                {
                    stateMachine.RequestStateTransition("Conversation");
                }
                else
                {
                    stateMachine.RequestStateTransition("Idle");
                }
            }
        }
    }

    public override void Exit(BehaviorStateMachine stateMachine)
    {
        base.Exit(stateMachine);
        Debug.Log($"[BehaviorState] {stateName}: Task exited.");
    }
}