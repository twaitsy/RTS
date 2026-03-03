using System.Collections.Generic;
using UnityEngine;

public static class MovementSystem
{
    private const float DefaultMoveSpeed = 2f;

    public static void MoveTo(
        GameObject actor,
        Vector3 target,
        SerializedStatContainer stats = null,
        IEnumerable<StatModifier> modifiers = null)
    {
        Transform t = actor.transform;

        Vector3 direction = target - t.position;
        float distance = direction.magnitude;

        if (distance <= 0f)
            return;

        float moveSpeed = CanonicalStatResolver.ResolveStatValue(
            stats,
            modifiers,
            CanonicalStatIds.Movement.MoveSpeed,
            DefaultMoveSpeed);

        Vector3 step = direction.normalized * moveSpeed * Time.deltaTime;

        if (step.magnitude > distance)
            step = direction;

        t.position += step;
    }
}
