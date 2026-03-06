using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/TaskSteps/MoveTo")]
public class MoveToStep : TaskStepDefinition
{
    private const float ARRIVAL_THRESHOLD = 0.1f;

    [SerializeField] private TaskTargetType targetType = TaskTargetType.None;
    [SerializeField] private string arrivedEventId;
    [SerializeField] private string failedEventId;

    public override TaskStepResult Execute(TaskContext context)
    {
        // --- BASIC CONTEXT CHECKS ---
        if (context == null)
        {
            TaskDebug.Error(TaskDebug.MoveTo, "MoveToStep: Context is null.");
            return TaskStepResult.FailTask("MoveToStep: Context is null.", failedEventId);
        }

        if (context.Actor == null)
        {
            TaskDebug.Error(TaskDebug.MoveTo, "MoveToStep: Context.Actor is null.");
            return TaskStepResult.FailTask("MoveToStep: Context.Actor is null.", failedEventId);
        }

        TaskDebug.Log(TaskDebug.MoveTo,
            $"[MoveToStep] Executing for actor '{context.Actor.name}' (targetType={targetType})");

        // --- RESOLVE TARGET ---
        var comp = ResolveTarget(context);

        if (comp == null)
        {
            TaskDebug.Warn(TaskDebug.MoveTo,
                $"[MoveToStep] ResolveTarget returned NULL for actor '{context.Actor.name}'.");
            return TaskStepResult.FailTask(
                $"MoveToStep: No target set for actor '{context.Actor.name}'.",
                failedEventId);
        }

        Vector3 actorPos = context.Actor.transform.position;
        Vector3 rawTargetPos = comp.transform.position;
        Vector3 targetPos = ResolveInteractionPosition(comp, actorPos);

        TaskDebug.Log(TaskDebug.MoveTo,
            $"[MoveToStep] Resolved target component '{comp.name}' at raw={rawTargetPos}, interaction={targetPos}");

        // --- MOVEMENT SERVICE CHECK ---
        var movement = context.Services?.Movement;
        if (movement == null)
        {
            TaskDebug.Error(TaskDebug.MoveTo, "[MoveToStep] Movement service is NULL.");
            return TaskStepResult.FailTask("MoveToStep: Movement service is unavailable.", failedEventId);
        }

        TaskDebug.Log(TaskDebug.MoveTo,
            $"[MoveToStep] ActorPos={actorPos}, TargetPos={targetPos}");

        // --- ARRIVAL CHECK ---
        bool arrived = HasArrived(context, targetPos);

        TaskDebug.Log(TaskDebug.MoveTo,
            $"[MoveToStep] HasArrived={arrived}, Threshold={ARRIVAL_THRESHOLD}");

        if (arrived)
        {
            TaskDebug.Log(TaskDebug.MoveTo,
                $"[MoveToStep] Actor '{context.Actor.name}' ARRIVED at target. Advancing step.");
            return TaskStepResult.AdvanceStep(arrivedEventId);
        }

        // --- ATTEMPT MOVEMENT ---
        TaskDebug.Log(TaskDebug.MoveTo,
            $"[MoveToStep] Calling Movement.MoveTo for actor '{context.Actor.name}'...");

        if (!movement.MoveTo(context.Actor, targetPos, context.RuntimeContext, out string failureReason))
        {
            TaskDebug.Warn(TaskDebug.MoveTo,
                $"[MoveToStep] Movement.MoveTo FAILED for actor '{context.Actor.name}'. Reason: {failureReason}");

            return TaskStepResult.FailTask($"MoveToStep: {failureReason}", failedEventId);
        }

        TaskDebug.Log(TaskDebug.MoveTo,
            $"[MoveToStep] Movement.MoveTo succeeded. Actor '{context.Actor.name}' moving toward target.");

        return TaskStepResult.StayOnStep();
    }

    private Component ResolveTarget(TaskContext context)
    {
        Component result = targetType switch
        {
            TaskTargetType.ResourceNode => context.ResourceTarget,
            TaskTargetType.Dropoff => context.DropoffTarget,
            _ => context.DropoffTarget != null ? context.DropoffTarget : context.ResourceTarget,
        };

        TaskDebug.Log(TaskDebug.MoveTo,
            $"[MoveToStep] ResolveTarget: targetType={targetType}, " +
            $"ResourceTarget={context.ResourceTarget}, DropoffTarget={context.DropoffTarget}, Result={result}");

        return result;
    }

    private Vector3 ResolveInteractionPosition(Component comp, Vector3 actorPos)
    {
        if (comp is ResourceNodeRuntime resourceNode)
            return resourceNode.GetBestInteractionWorldPosition(actorPos);

        if (comp is BuildingRuntime building)
            return building.GetBestInteractionWorldPosition(actorPos);

        return comp.transform.position;
    }

    private bool HasArrived(TaskContext context, Vector3 targetPos)
    {
        // Interpreter-based distance (preferred)
        if (UnitInterpreterRegistry.TryGet(context.RuntimeContext, out var interpreters) &&
            interpreters?.Movement != null &&
            interpreters.Movement.TryGetRemainingDistance(context.Actor, out float remainingDistance))
        {
            TaskDebug.Log(TaskDebug.MoveTo,
                $"[MoveToStep] RemainingDistance={remainingDistance} (interpreter)");

            return remainingDistance <= ARRIVAL_THRESHOLD;
        }

        // Fallback distance
        Vector3 currentPos = context.Actor.transform.position;
        float sqrDist = (targetPos - currentPos).sqrMagnitude;

        TaskDebug.Log(TaskDebug.MoveTo,
            $"[MoveToStep] Fallback sqrDist={sqrDist}, thresholdSqr={ARRIVAL_THRESHOLD * ARRIVAL_THRESHOLD}");

        return sqrDist < ARRIVAL_THRESHOLD * ARRIVAL_THRESHOLD;
    }
}
