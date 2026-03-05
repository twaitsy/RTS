using System.Collections.Generic;
using UnityEngine;

public class NeedsProfileRegistry : DefinitionRegistry<NeedsProfileDefinition>
{
    private static RegistrySchema<NeedsProfileDefinition> schema;

    public static NeedsProfileRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple NeedsProfileRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }

    protected override RegistrySchema<NeedsProfileDefinition> GetSchema()
    {
        return schema ??= new RegistrySchema<NeedsProfileDefinition>()
            .RequireField(nameof(NeedsProfileDefinition.Id), definition => definition.Id)
            .RequireField(nameof(NeedsProfileDefinition.Metadata), definition => definition.Metadata)
            .OptionalField(nameof(NeedsProfileDefinition.DisplayName), definition => definition.DisplayName)
            .RequireField(nameof(NeedsProfileDefinition.Stats), definition => definition.Stats)
            .OptionalField(nameof(NeedsProfileDefinition.StatModifiers), definition => definition.StatModifiers)
            .OptionalField(nameof(NeedsProfileDefinition.CivilianDefinitionId), definition => definition.CivilianDefinitionId)
            .OptionalField(nameof(NeedsProfileDefinition.Needs), definition => definition.Needs)
            .AddReference(
                $"{nameof(NeedsProfileDefinition.Stats)}.{nameof(SerializedStatContainer.Entries)}",
                definition => RegistrySchema<NeedsProfileDefinition>.ReferenceCollection(definition.Stats.Entries, entry => entry.StatId),
                false,
                new ReferenceTargetRule(nameof(StatRegistry), targetId => StatRegistry.Instance != null && StatRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(
                nameof(NeedsProfileDefinition.StatModifiers),
                definition => RegistrySchema<NeedsProfileDefinition>.ReferenceCollection(definition.StatModifiers, modifier => modifier.targetStatId),
                false,
                new ReferenceTargetRule(nameof(StatRegistry), targetId => StatRegistry.Instance != null && StatRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(
                nameof(NeedsProfileDefinition.CivilianDefinitionId),
                definition => RegistrySchema<NeedsProfileDefinition>.SingleReference(definition.CivilianDefinitionId),
                false,
                new ReferenceTargetRule(nameof(CivilianRegistry), targetId => CivilianRegistry.Instance != null && CivilianRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(
                nameof(NeedsProfileDefinition.Needs) + ".needId",
                definition => RegistrySchema<NeedsProfileDefinition>.ReferenceCollection(definition.Needs, need => need.needId),
                false,
                new ReferenceTargetRule(nameof(NeedRegistry), targetId => NeedRegistry.Instance != null && NeedRegistry.Instance.TryGet(targetId, out _)))
            .AddConstraint("NeedsProfileConstraints", ValidateConstraints);
    }

    protected override void ValidateDefinitions(List<NeedsProfileDefinition> defs, System.Action<string> reportError)
    {
    }

    private static IEnumerable<string> ValidateConstraints(NeedsProfileDefinition definition)
    {
        if (definition.HungerCurve <= 0f)
            yield return $"{nameof(NeedsProfileDefinition.HungerCurve)} must be greater than 0.";
        if (definition.ThirstCurve <= 0f)
            yield return $"{nameof(NeedsProfileDefinition.ThirstCurve)} must be greater than 0.";
        if (definition.FatigueCurve <= 0f)
            yield return $"{nameof(NeedsProfileDefinition.FatigueCurve)} must be greater than 0.";
        if (definition.MoraleCurve <= 0f)
            yield return $"{nameof(NeedsProfileDefinition.MoraleCurve)} must be greater than 0.";
        if (definition.StressCurve <= 0f)
            yield return $"{nameof(NeedsProfileDefinition.StressCurve)} must be greater than 0.";
        if (definition.SocialNeedCurve <= 0f)
            yield return $"{nameof(NeedsProfileDefinition.SocialNeedCurve)} must be greater than 0.";
        if (definition.CriticalNeedThreshold < 0f || definition.CriticalNeedThreshold > 1f)
            yield return $"{nameof(NeedsProfileDefinition.CriticalNeedThreshold)} must be between 0 and 1.";

        var seenNeedIds = new HashSet<string>();
        var needs = definition.Needs;

        if (needs == null)
            yield break;

        for (var index = 0; index < needs.Count; index++)
        {
            var entry = needs[index];
            if (string.IsNullOrWhiteSpace(entry.needId))
            {
                yield return $"{nameof(NeedsProfileDefinition.Needs)}[{index}].needId must not be empty.";
                continue;
            }

            if (!seenNeedIds.Add(entry.needId))
                yield return $"{nameof(NeedsProfileDefinition.Needs)} contains duplicate need id '{entry.needId}'.";

            if (entry.maxValue < 0f)
                yield return $"{nameof(NeedsProfileDefinition.Needs)}[{index}].{nameof(CivilianNeedEntry.maxValue)} must be greater than or equal to 0.";
            if (entry.startValue < 0f)
                yield return $"{nameof(NeedsProfileDefinition.Needs)}[{index}].{nameof(CivilianNeedEntry.startValue)} must be greater than or equal to 0.";
            if (entry.startValue > entry.maxValue)
                yield return $"{nameof(NeedsProfileDefinition.Needs)}[{index}].{nameof(CivilianNeedEntry.startValue)} must be less than or equal to {nameof(CivilianNeedEntry.maxValue)}.";
            if (entry.decayMultiplier < 0f)
                yield return $"{nameof(NeedsProfileDefinition.Needs)}[{index}].{nameof(CivilianNeedEntry.decayMultiplier)} must be greater than or equal to 0.";
        }
    }

    protected override IEnumerable<string> GetValidationDependencyErrors()
    {
        if (StatRegistry.Instance == null)
            yield return "Missing dependency: StatRegistry.Instance is null.";
        if (CivilianRegistry.Instance == null)
            yield return "Missing dependency: CivilianRegistry.Instance is null.";
        if (NeedRegistry.Instance == null)
            yield return "Missing dependency: NeedRegistry.Instance is null.";
    }
}
