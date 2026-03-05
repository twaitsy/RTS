using UnityEngine;

public static class NeedsTickSystem
{
    private const float DefaultHungerRate = 0f;
    private const float DefaultThirstRate = 0f;
    private const float DefaultFatigueRate = 0f;
    private const float DefaultMoraleDecayRate = 0f;

    public static float TickHunger(float currentHunger, float minHunger, UnitRuntimeContext context)
    {
        float baseRate = context?.ResolveStat(CanonicalStatIds.Needs.HungerRate, DefaultHungerRate) ?? DefaultHungerRate;
        float profileMultiplier = context?.NeedsProfile?.HungerCurve ?? 1f;
        float hungerRate = Mathf.Max(0f, baseRate * profileMultiplier);

        return Mathf.Max(minHunger, currentHunger - (hungerRate * Time.deltaTime));
    }

    public static float TickThirst(float currentThirst, float minThirst, UnitRuntimeContext context)
    {
        float baseRate = context?.ResolveStat(CanonicalStatIds.Needs.ThirstRate, DefaultThirstRate) ?? DefaultThirstRate;
        float profileMultiplier = context?.NeedsProfile?.ThirstCurve ?? 1f;
        float thirstRate = Mathf.Max(0f, baseRate * profileMultiplier);

        return Mathf.Max(minThirst, currentThirst - (thirstRate * Time.deltaTime));
    }

    public static float TickFatigue(float currentFatigue, float minFatigue, UnitRuntimeContext context)
    {
        float baseRate = context?.ResolveStat(CanonicalStatIds.Needs.FatigueRate, DefaultFatigueRate) ?? DefaultFatigueRate;
        float profileMultiplier = context?.NeedsProfile?.FatigueCurve ?? 1f;
        float fatigueRate = Mathf.Max(0f, baseRate * profileMultiplier);

        return Mathf.Max(minFatigue, currentFatigue - (fatigueRate * Time.deltaTime));
    }

    public static float TickMorale(float currentMorale, float minMorale, UnitRuntimeContext context)
    {
        float baseRate = context?.ResolveStat(CanonicalStatIds.Needs.MoraleDecayRate, DefaultMoraleDecayRate) ?? DefaultMoraleDecayRate;
        float profileMultiplier = context?.NeedsProfile?.MoraleCurve ?? 1f;
        float moraleDecay = Mathf.Max(0f, baseRate * profileMultiplier);

        return Mathf.Max(minMorale, currentMorale - (moraleDecay * Time.deltaTime));
    }
}
