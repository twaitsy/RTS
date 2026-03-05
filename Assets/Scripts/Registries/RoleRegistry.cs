using System.Collections.Generic;
using UnityEngine;

public class RoleRegistry : DefinitionRegistry<RoleDefinition>
{
    private static RegistrySchema<RoleDefinition> schema;

    public static RoleRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple RoleRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }

    protected override RegistrySchema<RoleDefinition> GetSchema()
    {
        return schema ??= new RegistrySchema<RoleDefinition>()
            .RequireField(nameof(RoleDefinition.Id), definition => definition.Id)
            .RequireField(nameof(RoleDefinition.Metadata), definition => definition.Metadata)
            .RequireField(nameof(RoleDefinition.DisplayName), definition => definition.DisplayName)
            .OptionalField(nameof(RoleDefinition.StatModifiers), definition => definition.StatModifiers)
            .OptionalField(nameof(RoleDefinition.BehaviourIds), definition => definition.BehaviourIds)
            .OptionalField(nameof(RoleDefinition.JobIds), definition => definition.JobIds)
            .OptionalField(nameof(RoleDefinition.NeedMultipliers), definition => definition.NeedMultipliers)
            .OptionalField(nameof(RoleDefinition.RequiredStatIds), definition => definition.RequiredStatIds)
            .AddReference(
                nameof(RoleDefinition.StatModifiers),
                definition => RegistrySchema<RoleDefinition>.ReferenceCollection(definition.StatModifiers, modifier => modifier.targetStatId),
                false,
                new ReferenceTargetRule(nameof(StatRegistry), targetId => StatRegistry.Instance != null && StatRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(
                nameof(RoleDefinition.BehaviourIds),
                definition => RegistrySchema<RoleDefinition>.ReferenceCollection(definition.BehaviourIds, id => id),
                false,
                new ReferenceTargetRule(nameof(BehaviourRegistry), targetId => BehaviourRegistry.Instance != null && BehaviourRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(
                nameof(RoleDefinition.JobIds),
                definition => RegistrySchema<RoleDefinition>.ReferenceCollection(definition.JobIds, id => id),
                false,
                new ReferenceTargetRule(nameof(JobRegistry), targetId => JobRegistry.Instance != null && JobRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(
                nameof(RoleDefinition.NeedMultipliers) + ".needId",
                definition => RegistrySchema<RoleDefinition>.ReferenceCollection(definition.NeedMultipliers, entry => entry.needId),
                false,
                new ReferenceTargetRule(nameof(NeedRegistry), targetId => NeedRegistry.Instance != null && NeedRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(
                nameof(RoleDefinition.RequiredStatIds),
                definition => RegistrySchema<RoleDefinition>.ReferenceCollection(definition.RequiredStatIds, id => id),
                false,
                new ReferenceTargetRule(nameof(StatRegistry), targetId => StatRegistry.Instance != null && StatRegistry.Instance.TryGet(targetId, out _)))
            .AddConstraint("RoleConstraints", ValidateConstraints);
    }

    private static IEnumerable<string> ValidateConstraints(RoleDefinition definition)
    {
        var required = definition.RequiredStatIds;
        if (required != null)
        {
            var seenRequired = new HashSet<string>(System.StringComparer.Ordinal);
            for (var i = 0; i < required.Count; i++)
            {
                var statId = required[i];
                if (string.IsNullOrWhiteSpace(statId))
                {
                    yield return $"{nameof(RoleDefinition.RequiredStatIds)}[{i}] must not be empty.";
                    continue;
                }

                if (!seenRequired.Add(statId))
                    yield return $"{nameof(RoleDefinition.RequiredStatIds)} contains duplicate stat id '{statId}'.";
            }
        }

        var multipliers = definition.NeedMultipliers;
        if (multipliers == null)
            yield break;

        var seenNeedIds = new HashSet<string>(System.StringComparer.Ordinal);
        for (var i = 0; i < multipliers.Count; i++)
        {
            var entry = multipliers[i];
            if (string.IsNullOrWhiteSpace(entry.needId))
            {
                yield return $"{nameof(RoleDefinition.NeedMultipliers)}[{i}].needId must not be empty.";
                continue;
            }

            if (!seenNeedIds.Add(entry.needId))
                yield return $"{nameof(RoleDefinition.NeedMultipliers)} contains duplicate need id '{entry.needId}'.";

            if (entry.multiplier < 0f)
                yield return $"{nameof(RoleDefinition.NeedMultipliers)}[{i}].multiplier must be greater than or equal to 0.";
        }
    }

    protected override IEnumerable<string> GetValidationDependencyErrors()
    {
        if (StatRegistry.Instance == null)
            yield return "Missing dependency: StatRegistry.Instance is null.";
        if (BehaviourRegistry.Instance == null)
            yield return "Missing dependency: BehaviourRegistry.Instance is null.";
        if (JobRegistry.Instance == null)
            yield return "Missing dependency: JobRegistry.Instance is null.";
        if (NeedRegistry.Instance == null)
            yield return "Missing dependency: NeedRegistry.Instance is null.";
    }
}
