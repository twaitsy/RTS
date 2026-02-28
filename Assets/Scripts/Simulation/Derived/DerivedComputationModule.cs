using System;
using System.Collections.Generic;
using System.Linq;

public static class DerivedComputationModule
{
    public static float ComputeDps(UnitDefinition unitDefinition)
    {
        if (unitDefinition == null)
            return 0f;

        var rollups = BuildRollups(unitDefinition.StatModifiers);
        var baseDamage = ResolveStatValue(unitDefinition.Stats, rollups, CanonicalStatIds.Combat.BaseDamage, 0f);
        var attackSpeed = ResolveStatValue(unitDefinition.Stats, rollups, CanonicalStatIds.Combat.AttackSpeed, 0f);

        return Math.Max(0f, baseDamage * attackSpeed);
    }

    public static float ComputeProductionThroughput(ProductionDefinition productionDefinition)
    {
        if (productionDefinition == null || productionDefinition.ProductionTime <= 0f)
            return 0f;

        var workSpeed = GetBaseStatValue(productionDefinition.Stats, CanonicalStatIds.Production.WorkSpeed, 1f);
        return Math.Max(0f, workSpeed / productionDefinition.ProductionTime);
    }

    public static float ComputeResourceEfficiency(ProductionDefinition productionDefinition, string resourceId)
    {
        if (productionDefinition == null || string.IsNullOrWhiteSpace(resourceId))
            return 0f;

        var targetCost = productionDefinition.Costs?
            .Where(cost => StringComparer.Ordinal.Equals(cost.ResourceId, resourceId))
            .Sum(cost => Math.Max(0, cost.Amount)) ?? 0;

        if (targetCost <= 0)
            return 0f;

        return ComputeProductionThroughput(productionDefinition) / targetCost;
    }

    public static IReadOnlyDictionary<string, StatModifierRollup> ComputeStatModifierRollups(IEnumerable<StatModifier> modifiers)
    {
        return BuildRollups(modifiers);
    }

    private static Dictionary<string, StatModifierRollup> BuildRollups(IEnumerable<StatModifier> modifiers)
    {
        var rollups = new Dictionary<string, StatModifierRollup>(StringComparer.Ordinal);
        if (modifiers == null)
            return rollups;

        foreach (var modifier in modifiers)
        {
            if (string.IsNullOrWhiteSpace(modifier.targetStatId))
                continue;

            if (!rollups.TryGetValue(modifier.targetStatId, out var rollup))
                rollup = StatModifierRollup.Identity(modifier.targetStatId);

            rollup = rollup.Apply(modifier);
            rollups[modifier.targetStatId] = rollup;
        }

        return rollups;
    }

    private static float ResolveStatValue(
        SerializedStatContainer stats,
        IReadOnlyDictionary<string, StatModifierRollup> rollups,
        string statId,
        float defaultValue)
    {
        var baseValue = GetBaseStatValue(stats, statId, defaultValue);

        if (rollups != null && rollups.TryGetValue(statId, out var rollup))
            return rollup.ApplyTo(baseValue);

        return baseValue;
    }

    private static float GetBaseStatValue(SerializedStatContainer stats, string statId, float defaultValue)
    {
        if (stats?.Entries == null || string.IsNullOrWhiteSpace(statId))
            return defaultValue;

        foreach (var stat in stats.Entries)
        {
            if (StringComparer.Ordinal.Equals(stat.StatId, statId))
                return stat.Value;
        }

        return defaultValue;
    }
}

public readonly struct StatModifierRollup
{
    public StatModifierRollup(string statId, float additive, float multiplicative, bool hasOverride, float overrideValue)
    {
        StatId = statId;
        Additive = additive;
        Multiplicative = multiplicative;
        HasOverride = hasOverride;
        OverrideValue = overrideValue;
    }

    public string StatId { get; }
    public float Additive { get; }
    public float Multiplicative { get; }
    public bool HasOverride { get; }
    public float OverrideValue { get; }

    public static StatModifierRollup Identity(string statId)
    {
        return new StatModifierRollup(statId, additive: 0f, multiplicative: 1f, hasOverride: false, overrideValue: 0f);
    }

    public StatModifierRollup Apply(StatModifier modifier)
    {
        return modifier.operation switch
        {
            StatOperation.Add => new StatModifierRollup(StatId, Additive + modifier.value, Multiplicative, HasOverride, OverrideValue),
            StatOperation.Multiply => new StatModifierRollup(StatId, Additive, Multiplicative * modifier.value, HasOverride, OverrideValue),
            StatOperation.Override => new StatModifierRollup(StatId, Additive, Multiplicative, hasOverride: true, overrideValue: modifier.value),
            _ => this
        };
    }

    public float ApplyTo(float baseValue)
    {
        if (HasOverride)
            return OverrideValue;

        return (baseValue + Additive) * Multiplicative;
    }
}
