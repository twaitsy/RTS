using System.Collections.Generic;
using UnityEngine;

public class JobRegistry : DefinitionRegistry<JobDefinition>
{
    private static RegistrySchema<JobDefinition> schema;

    public static JobRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple JobRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }

    protected override RegistrySchema<JobDefinition> GetSchema()
    {
        return schema ??= new RegistrySchema<JobDefinition>()
            .RequireField(nameof(JobDefinition.Id), definition => definition.Id)
            .RequireField(nameof(JobDefinition.DisplayName), definition => definition.DisplayName)
            .RequireField(nameof(JobDefinition.Metadata), definition => definition.Metadata)
            .RequireField(nameof(JobDefinition.Stats), definition => definition.Stats)
            .OptionalField(nameof(JobDefinition.StatModifiers), definition => definition.StatModifiers)
            .OptionalField(nameof(JobDefinition.AllowedActionIds), definition => definition.AllowedActionIds)
            .OptionalField(nameof(JobDefinition.PreferredNeedIds), definition => definition.PreferredNeedIds)
            .AddReference(
                $"{nameof(JobDefinition.Stats)}.{nameof(SerializedStatContainer.Entries)}",
                definition => RegistrySchema<JobDefinition>.ReferenceCollection(definition.Stats.Entries, entry => entry.StatId),
                false,
                new ReferenceTargetRule(nameof(StatRegistry), targetId => StatRegistry.Instance != null && StatRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(
                nameof(JobDefinition.StatModifiers),
                definition => RegistrySchema<JobDefinition>.ReferenceCollection(definition.StatModifiers, modifier => modifier.targetStatId),
                false,
                new ReferenceTargetRule(nameof(StatRegistry), targetId => StatRegistry.Instance != null && StatRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(
                nameof(JobDefinition.AllowedActionIds),
                definition => RegistrySchema<JobDefinition>.ReferenceCollection(definition.AllowedActionIds, id => id),
                false,
                new ReferenceTargetRule(nameof(CommandRegistry), targetId => CommandRegistry.Instance != null && CommandRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(
                nameof(JobDefinition.PreferredNeedIds),
                definition => RegistrySchema<JobDefinition>.ReferenceCollection(definition.PreferredNeedIds, id => id),
                false,
                new ReferenceTargetRule(nameof(NeedRegistry), targetId => NeedRegistry.Instance != null && NeedRegistry.Instance.TryGet(targetId, out _)))
            .AddConstraint(
                "JobConstraints",
                definition =>
                {
                    var errors = new List<string>();
                    if (definition.BaseWorkTime < 0f)
                        errors.Add($"{nameof(JobDefinition.BaseWorkTime)} must be greater than or equal to 0.");
                    if (definition.WorkPriority < 0)
                        errors.Add($"{nameof(JobDefinition.WorkPriority)} must be greater than or equal to 0.");

                    var seenActionIds = new HashSet<string>();
                    var actionIds = definition.AllowedActionIds;
                    if (actionIds == null)
                        return errors;

                    for (var index = 0; index < actionIds.Count; index++)
                    {
                        var actionId = actionIds[index];
                        if (string.IsNullOrWhiteSpace(actionId))
                        {
                            errors.Add($"{nameof(JobDefinition.AllowedActionIds)}[{index}] must not be empty.");
                            continue;
                        }

                        if (!seenActionIds.Add(actionId))
                            errors.Add($"{nameof(JobDefinition.AllowedActionIds)} contains duplicate action id '{actionId}'.");
                    }

                    return errors;
                });
    }

    protected override IEnumerable<string> GetValidationDependencyErrors()
    {
        if (StatRegistry.Instance == null)
            yield return "Missing dependency: StatRegistry.Instance is null.";
        if (CommandRegistry.Instance == null)
            yield return "Missing dependency: CommandRegistry.Instance is null.";
        if (NeedRegistry.Instance == null)
            yield return "Missing dependency: NeedRegistry.Instance is null.";
    }
}
