using UnityEngine;

public static class AISensingSystem
{
    private const float DefaultPerceptionRadius = 0f;

    public static bool IsTargetPerceivable(
        Vector3 observerPosition,
        Vector3 targetPosition,
        UnitRuntimeContext context,
        float targetStealth = 0f,
        float emittedNoise = 1f,
        Vector3? observerForward = null)
    {
        var profile = context?.PerceptionProfile;

        float profileRadius = profile?.HearingRadius ?? DefaultPerceptionRadius;
        float perceptionRadius = context?.ResolveStat(CanonicalStatIds.Perception.PerceptionRadius, profileRadius) ?? profileRadius;
        float hearingRadius = context?.ResolveStat(CanonicalStatIds.Perception.HearingRadius, profile?.HearingRadius ?? 0f) ?? 0f;
        float visionArc = context?.ResolveStat(CanonicalStatIds.Perception.VisionArc, profile?.VisionArc ?? 360f) ?? 360f;
        float detection = Mathf.Clamp01(profile?.StealthDetection ?? 0f);

        float sqrDistance = (targetPosition - observerPosition).sqrMagnitude;
        if (sqrDistance <= (hearingRadius * hearingRadius * Mathf.Max(0f, emittedNoise)))
            return true;

        if (sqrDistance > (perceptionRadius * perceptionRadius))
            return false;

        if (targetStealth > detection)
            return false;

        if (!observerForward.HasValue || visionArc >= 360f)
            return true;

        var toTarget = targetPosition - observerPosition;
        if (toTarget.sqrMagnitude < 0.0001f)
            return true;

        var angle = Vector3.Angle(observerForward.Value.normalized, toTarget.normalized);
        return angle <= (visionArc * 0.5f);
    }
}
