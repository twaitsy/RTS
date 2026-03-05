using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class DerivedComputationModule
{
    public static float ComputeDps(UnitDefinition unitDefinition)
    {
        var context = UnitRuntimeContextResolver.Resolve(unitDefinition, definitionResolver: null);
        return ComputeDps(context);
    }

    public static float ComputeDps(UnitRuntimeContext context)
    {
        if (context?.Unit == null)
            return 0f;

        if (context.Weapons == null || context.Weapons.Count == 0)
            return ComputeWeaponDps(context, weapon: null);

        float totalDps = 0f;
        for (int i = 0; i < context.Weapons.Count; i++)
            totalDps += ComputeWeaponDps(context, context.Weapons[i]);

        return Mathf.Max(0f, totalDps);
    }

    public static float ComputeEffectiveHp(UnitRuntimeContext context)
    {
        if (context?.Unit == null)
            return 0f;

        float health = Mathf.Max(0f, context.ResolveStat(CanonicalStatIds.Combat.Health, 0f));
        float armor = Mathf.Max(0f, context.ResolveStat(CanonicalStatIds.Combat.Armor, 0f));
        float blockChance = Mathf.Clamp01(context.ResolveStat(CanonicalStatIds.Combat.BlockChance, 0f));
        float dodgeChance = Mathf.Clamp01(context.ResolveStat(CanonicalStatIds.Combat.DodgeChance, 0f));

        float armorMitigation = armor / (100f + armor);
        float totalMitigation = Mathf.Clamp(armorMitigation + (blockChance * 0.5f) + (dodgeChance * 0.5f), 0f, 0.9f);

        return health / Mathf.Max(0.0001f, 1f - totalMitigation);
    }

    public static float ComputeThreat(UnitRuntimeContext context)
    {
        if (context?.Unit == null)
            return 0f;

        float dps = ComputeDps(context);
        float aggression = Mathf.Max(0f, context.ResolveStat(CanonicalStatIds.AI.Aggression, 0f));
        float range = Mathf.Max(0f, context.ResolveStat(CanonicalStatIds.Combat.AttackRange, 0f));

        return dps * (1f + aggression) * (1f + (range * 0.05f));
    }

    public static float ComputeMoraleStability(UnitRuntimeContext context)
    {
        if (context?.Unit == null)
            return 0f;

        float moraleDecay = Mathf.Max(0f, context.ResolveStat(CanonicalStatIds.Needs.MoraleDecayRate, 0f));
        float stressGain = Mathf.Max(0f, context.ResolveStat(CanonicalStatIds.Needs.StressGainRate, 0f));
        float decayFactor = context.NeedsProfile?.MoraleCurve ?? 1f;

        return 1f / (1f + (moraleDecay + stressGain) * decayFactor);
    }

    public static float ComputeJobProficiency(UnitRuntimeContext context)
    {
        if (context?.Unit == null)
            return 0f;

        float productionWorkSpeed = Mathf.Max(0f, context.ResolveStat(CanonicalStatIds.Production.WorkSpeed, 1f));
        float buildSpeed = Mathf.Max(0f, context.ResolveStat(CanonicalStatIds.Production.BuildSpeed, 1f));

        float profileThroughput = ComputeProductionThroughput(context);

        return (productionWorkSpeed + buildSpeed + profileThroughput) / 3f;
    }

    public static float ComputeProductionThroughput(ProductionDefinition productionDefinition)
    {
        if (productionDefinition == null || productionDefinition.ProductionTime <= 0f)
            return 0f;

        var workSpeed = GetBaseStatValue(productionDefinition.Stats, CanonicalStatIds.Production.WorkSpeed, 1f);
        return Mathf.Max(0f, workSpeed / productionDefinition.ProductionTime);
    }

    public static float ComputeProductionThroughput(UnitRuntimeContext context)
    {
        if (context?.Unit == null)
            return 0f;

        float profileProductionTime = context.ProductionProfile?.ProductionTime ?? 0f;
        if (profileProductionTime <= 0f)
            return 0f;

        float workSpeed = Mathf.Max(0f, context.ResolveStat(CanonicalStatIds.Production.WorkSpeed, 1f));
        return workSpeed / profileProductionTime;
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
        return CanonicalStatResolver.BuildRollups(modifiers);
    }

    private static float ComputeWeaponDps(UnitRuntimeContext context, WeaponDefinition weapon)
    {
        float baseDamage = context.ResolveStat(CanonicalStatIds.Combat.BaseDamage, 0f);
        float attackDamage = context.ResolveStat(CanonicalStatIds.Combat.AttackDamage, baseDamage);
        float attackSpeed = Mathf.Max(0f, context.ResolveStat(CanonicalStatIds.Combat.AttackSpeed, 0f));
        float critChance = Mathf.Clamp01(context.ResolveStat(CanonicalStatIds.Combat.CritChance, 0f));
        float critMultiplier = Mathf.Max(1f, context.ResolveStat(CanonicalStatIds.Combat.CritMultiplier, 1f));

        if (weapon != null)
        {
            baseDamage = GetBaseStatValue(weapon.Stats, CanonicalStatIds.Combat.BaseDamage, baseDamage);
            attackDamage = GetBaseStatValue(weapon.Stats, CanonicalStatIds.Combat.AttackDamage, attackDamage);
            attackSpeed = GetBaseStatValue(weapon.Stats, CanonicalStatIds.Combat.AttackSpeed, attackSpeed);
            critChance = Mathf.Clamp01(GetBaseStatValue(weapon.Stats, CanonicalStatIds.Combat.CritChance, critChance));
            critMultiplier = Mathf.Max(1f, GetBaseStatValue(weapon.Stats, CanonicalStatIds.Combat.CritMultiplier, critMultiplier));
        }

        float expectedDamage = Mathf.Max(0f, attackDamage) * (1f + critChance * (critMultiplier - 1f));
        return expectedDamage * Mathf.Max(0f, attackSpeed);
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
