using UnityEngine;

public static class MovementSystem
{
    private const float DefaultMoveSpeed = 2f;
    private const float DefaultAcceleration = 8f;
    private const float DefaultTurnRate = 360f;
    private static readonly System.Collections.Generic.Dictionary<int, float> velocityByActorId = new();

    public static void MoveTo(GameObject actor, Vector3 target, UnitRuntimeContext context)
    {
        if (actor == null)
            return;

        if (UnitInterpreterRegistry.TryGet(context, out var interpreters) && interpreters.Movement != null)
        {
            interpreters.Movement.MoveTo(actor, target);
            return;
        }

        float fallbackSpeed = context?.MovementProfile?.MoveSpeedMultiplier ?? context?.LocomotionProfile?.Speed ?? DefaultMoveSpeed;
        float moveSpeed = context?.ResolveStat(CanonicalStatIds.Movement.MoveSpeed, fallbackSpeed) ?? fallbackSpeed;

        float fallbackAcceleration = context?.MovementProfile?.Acceleration ?? DefaultAcceleration;
        float acceleration = context?.ResolveStat(CanonicalStatIds.Movement.Acceleration, fallbackAcceleration) ?? fallbackAcceleration;

        float fallbackTurnRate = context?.MovementProfile?.TurnRate ?? DefaultTurnRate;
        float turnRate = context?.ResolveStat(CanonicalStatIds.Movement.TurnRate, fallbackTurnRate) ?? fallbackTurnRate;

        float stoppingDistance = context?.MovementProfile?.StoppingDistance ?? 0f;
        MoveTo(actor, target, moveSpeed, acceleration, turnRate, stoppingDistance);
    }

    private static void MoveTo(GameObject actor, Vector3 target, float moveSpeed, float acceleration, float turnRate, float stoppingDistance)
    {
        Transform t = actor.transform;

        Vector3 direction = target - t.position;
        float distance = direction.magnitude;

        if (distance <= Mathf.Max(0f, stoppingDistance))
            return;

        if (direction.sqrMagnitude > 0.0001f)
        {
            Vector3 forward = Vector3.RotateTowards(t.forward, direction.normalized, Mathf.Deg2Rad * turnRate * Time.deltaTime, 0f);
            if (forward.sqrMagnitude > 0.0001f)
                t.forward = forward;
        }

        var actorId = actor.GetInstanceID();
        velocityByActorId.TryGetValue(actorId, out var currentSpeed);
        var nextSpeed = Mathf.MoveTowards(currentSpeed, moveSpeed, Mathf.Max(0f, acceleration) * Time.deltaTime);
        velocityByActorId[actorId] = nextSpeed;

        Vector3 step = direction.normalized * nextSpeed * Time.deltaTime;

        if (step.magnitude > distance)
            step = direction;

        t.position += step;
    }
}
