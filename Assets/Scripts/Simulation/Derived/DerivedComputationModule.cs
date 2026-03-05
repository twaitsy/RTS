using System.Collections.Generic;
using UnityEngine;

public readonly struct DerivedRuntimeSnapshot
{
    public DerivedRuntimeSnapshot(
        float dps,
        float effectiveHp,
        float threatRating,
        float moraleStability,
        float fatigueDecay,
        float jobProficiency,
        float movementEfficiency)
    {
        Dps = dps;
        EffectiveHp = effectiveHp;
        ThreatRating = threatRating;
        MoraleStability = moraleStability;
        FatigueDecay = fatigueDecay;
        JobProficiency = jobProficiency;
        MovementEfficiency = movementEfficiency;
    }

    public float Dps { get; }
    public float EffectiveHp { get; }
    public float ThreatRating { get; }
    public float MoraleStability { get; }
    public float FatigueDecay { get; }
    public float JobProficiency { get; }
    public float MovementEfficiency { get; }
}

public static class DerivedComputationModule
{
    public static float ComputeDps(UnitDefinition unitDefinition)
    {
        var context = UnitRuntimeContextResolver.Resolve(unitDefinition, definitionResolver: null);
        return ComputeDps(context);
    }

    public static DerivedRuntimeSnapshot ComputeSnapshot(UnitRuntimeContext context)
    {
        if (context?.Unit == null)
            return default;

        var dps = ComputeDps(context);
        var effectiveHp = ComputeEffectiveHp(context);
        var threat = ComputeThreat(context);
        var morale = ComputeMoraleStability(context);
        var fatigueDecay = ComputeFatigueDecay(context);
        var jobProficiency = ComputeJobProficiency(context);
        var movementEfficiency = ComputeMovementEfficiency(context);

        return new DerivedRuntimeSnapshot(dps, effectiveHp, threat, morale, fatigueDecay, jobProficiency, movementEfficiency);
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
        float moodStability = context.MoodProfile?.MoraleStability ?? 1f;
        float recovery = context.MoodProfile?.StressRecoveryRate ?? 0f;

        return moodStability / Mathf.Max(0.0001f, 1f + ((moraleDecay + stressGain - recovery) * decayFactor));
    }

    public static float ComputeFatigueDecay(UnitRuntimeContext context)
    {
        if (context?.Unit == null)
            return 0f;

        float fatigueRate = Mathf.Max(0f, context.ResolveStat(CanonicalStatIds.Needs.FatigueRate, 0f));
        float fatigueCurve = Mathf.Max(0f, context.NeedsProfile?.FatigueCurve ?? 1f);
        float moraleStability = Mathf.Max(0.1f, ComputeMoraleStability(context));
        return fatigueRate * fatigueCurve / moraleStability;
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

    public static float ComputeMovementEfficiency(UnitRuntimeContext context)
    {
        if (context?.Unit == null)
            return 0f;

        float speed = Mathf.Max(0f, context.ResolveStat(CanonicalStatIds.Movement.MoveSpeed, context.MovementProfile?.MoveSpeedMultiplier ?? 1f));
        float acceleration = Mathf.Max(0f, context.ResolveStat(CanonicalStatIds.Movement.Acceleration, context.MovementProfile?.Acceleration ?? 1f));
        float turnRate = Mathf.Max(1f, context.ResolveStat(CanonicalStatIds.Movement.TurnRate, context.MovementProfile?.TurnRate ?? 1f));

        float locomotionFactor = context.LocomotionProfile switch
        {
            null => 1f,
            var p when p.CanTraverseAir => 1.15f,
            var p when p.CanTraverseWater => 1.05f,
            _ => 1f
        };

        return ((speed * 0.6f) + (acceleration * 0.25f) + (turnRate * 0.15f / 180f)) * locomotionFactor;
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

    public static IReadOnlyDictionary<string, StatModifierRollup> ComputeStatModifierRollups(IEnumerable<StatModifier> modifiers)
    {
        return CanonicalStatResolver.BuildRollups(modifiers);
    }

    private static float ComputeWeaponDps(UnitRuntimeContext context, WeaponDefinition weapon)
    {
        float baseDamage = context.ResolveStat(CanonicalStatIds.Combat.BaseDamage, weapon?.Damage ?? 0f);
        float attackDamage = context.ResolveStat(CanonicalStatIds.Combat.AttackDamage, baseDamage);
        float attackSpeed = Mathf.Max(0f, context.ResolveStat(CanonicalStatIds.Combat.AttackSpeed, weapon?.AttackSpeed ?? 0f));
        float critChance = Mathf.Clamp01(context.ResolveStat(CanonicalStatIds.Combat.CritChance, 0f));
        float critMultiplier = Mathf.Max(1f, context.ResolveStat(CanonicalStatIds.Combat.CritMultiplier, 1f));
        float range = Mathf.Max(0f, context.ResolveStat(CanonicalStatIds.Combat.AttackRange, weapon?.Range ?? 0f));

        if (weapon != null)
        {
            baseDamage = Mathf.Max(baseDamage, weapon.Damage);
            attackDamage = Mathf.Max(attackDamage, weapon.Damage);
            attackSpeed = Mathf.Max(attackSpeed, weapon.AttackSpeed);
            range = Mathf.Max(range, weapon.Range);
        }

        float expectedDamage = Mathf.Max(0f, attackDamage) * (1f + critChance * (critMultiplier - 1f));
        float rangeFactor = 1f + Mathf.Min(range, 10f) * 0.02f;
        return expectedDamage * Mathf.Max(0f, attackSpeed) * rangeFactor;
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
