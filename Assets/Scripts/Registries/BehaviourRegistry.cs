using System.Collections.Generic;
using UnityEngine;

public class BehaviourRegistry : DefinitionRegistry<BehaviourDefinition>
{
    private static RegistrySchema<BehaviourDefinition> schema;

    public static BehaviourRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple BehaviourRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }

    protected override RegistrySchema<BehaviourDefinition> GetSchema()
    {
        return schema ??= new RegistrySchema<BehaviourDefinition>()
            .RequireField(nameof(BehaviourDefinition.Id), definition => definition.Id)
            .RequireField(nameof(BehaviourDefinition.DisplayName), definition => definition.DisplayName)
            .RequireField(nameof(BehaviourDefinition.Metadata), definition => definition.Metadata)
            .RequireField(nameof(BehaviourDefinition.Stats), definition => definition.Stats)
            .OptionalField(nameof(BehaviourDefinition.JobIds), definition => definition.JobIds)
            .AddReference(
                $"{nameof(BehaviourDefinition.Stats)}.{nameof(SerializedStatContainer.Entries)}",
                definition => RegistrySchema<BehaviourDefinition>.ReferenceCollection(definition.Stats.Entries, entry => entry.StatId),
                false,
                new ReferenceTargetRule(nameof(StatRegistry), targetId => StatRegistry.Instance != null && StatRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(
                nameof(BehaviourDefinition.JobIds),
                definition => RegistrySchema<BehaviourDefinition>.ReferenceCollection(definition.JobIds, id => id),
                false,
                new ReferenceTargetRule(nameof(JobRegistry), targetId => JobRegistry.Instance != null && JobRegistry.Instance.TryGet(targetId, out _)))
            .AddConstraint(
                "BehaviourConstraints",
                definition =>
                {
                    var errors = new List<string>();
                    if (definition.Priority < 0)
                        errors.Add($"{nameof(BehaviourDefinition.Priority)} must be greater than or equal to 0.");

                    return errors;
                });
    }

    protected override IEnumerable<string> GetValidationDependencyErrors()
    {
        if (StatRegistry.Instance == null)
            yield return "Missing dependency: StatRegistry.Instance is null.";
        if (JobRegistry.Instance == null)
            yield return "Missing dependency: JobRegistry.Instance is null.";
    }
}
