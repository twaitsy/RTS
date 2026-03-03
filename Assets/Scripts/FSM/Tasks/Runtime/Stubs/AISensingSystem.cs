using System.Collections.Generic;
using UnityEngine;

public static class AISensingSystem
{
    private const float DefaultPerceptionRadius = 0f;

    public static bool IsTargetPerceivable(
        Vector3 observerPosition,
        Vector3 targetPosition,
        SerializedStatContainer stats,
        IEnumerable<StatModifier> modifiers = null)
    {
        float perceptionRadius = CanonicalStatResolver.ResolveStatValue(
            stats,
            modifiers,
            CanonicalStatIds.AI.PerceptionRadius,
            DefaultPerceptionRadius);

        float sqrDistance = (targetPosition - observerPosition).sqrMagnitude;
        return sqrDistance <= (perceptionRadius * perceptionRadius);
    }
}
