using System.Collections.Generic;
using UnityEngine;

public static class AISensingSystem
{
    private const float DefaultPerceptionRadius = 0f;

    public static bool IsTargetPerceivable(Vector3 observerPosition, Vector3 targetPosition, UnitRuntimeContext context)
    {
        var profile = context?.PerceptionProfile;

        float profileRadius = profile?.HearingRadius ?? DefaultPerceptionRadius;
        float perceptionRadius = context?.ResolveStat(CanonicalStatIds.Perception.PerceptionRadius, profileRadius) ?? profileRadius;

        float sqrDistance = (targetPosition - observerPosition).sqrMagnitude;
        return sqrDistance <= (perceptionRadius * perceptionRadius);
    }

    public static bool IsTargetPerceivable(
        Vector3 observerPosition,
        Vector3 targetPosition,
        SerializedStatContainer stats,
        IEnumerable<StatModifier> modifiers = null)
    {
        float perceptionRadius = CanonicalStatResolver.ResolveStatValue(
            stats,
            modifiers,
            CanonicalStatIds.Perception.PerceptionRadius,
            DefaultPerceptionRadius);

        float sqrDistance = (targetPosition - observerPosition).sqrMagnitude;
        return sqrDistance <= (perceptionRadius * perceptionRadius);
    }
}
