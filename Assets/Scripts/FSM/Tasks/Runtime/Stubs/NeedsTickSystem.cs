using System.Collections.Generic;
using UnityEngine;

public static class NeedsTickSystem
{
    private const float DefaultHungerRate = 0f;

    public static float TickHunger(
        float currentHunger,
        float minHunger,
        SerializedStatContainer stats,
        IEnumerable<StatModifier> modifiers = null)
    {
        float hungerRate = CanonicalStatResolver.ResolveStatValue(
            stats,
            modifiers,
            CanonicalStatIds.Needs.HungerRate,
            DefaultHungerRate);

        return Mathf.Max(minHunger, currentHunger - (hungerRate * Time.deltaTime));
    }
}
