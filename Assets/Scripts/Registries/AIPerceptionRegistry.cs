using System.Collections.Generic;
using UnityEngine;

public class AIPerceptionRegistry : DefinitionRegistry<AIPerceptionDefinition>
{
    private static RegistrySchema<AIPerceptionDefinition> schema;

    public static AIPerceptionRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple AIPerceptionRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }

    protected override RegistrySchema<AIPerceptionDefinition> GetSchema()
    {
        return schema ??= new RegistrySchema<AIPerceptionDefinition>()
            .RequireField(nameof(AIPerceptionDefinition.Id), definition => definition.Id)
            .RequireField(nameof(AIPerceptionDefinition.DisplayName), definition => definition.DisplayName)
            .RequireField(nameof(AIPerceptionDefinition.Metadata), definition => definition.Metadata)
            .RequireField(nameof(AIPerceptionDefinition.Stats), definition => definition.Stats)
            .AddReference(
                $"{nameof(AIPerceptionDefinition.Stats)}.{nameof(SerializedStatContainer.Entries)}",
                definition => RegistrySchema<AIPerceptionDefinition>.ReferenceCollection(definition.Stats.Entries, entry => entry.StatId),
                false,
                new ReferenceTargetRule(nameof(StatRegistry), targetId => StatRegistry.Instance != null && StatRegistry.Instance.TryGet(targetId, out _)))
            .AddConstraint(
                "AIPerceptionConstraints",
                definition =>
                {
                    var errors = new List<string>();

                    if (definition.VisionArc <= 0f || definition.VisionArc > 360f)
                        errors.Add($"{nameof(AIPerceptionDefinition.VisionArc)} must be greater than 0 and at most 360.");
                    if (definition.HearingRadius < 0f)
                        errors.Add($"{nameof(AIPerceptionDefinition.HearingRadius)} must be greater than or equal to 0.");
                    if (definition.StealthDetection < 0f || definition.StealthDetection > 1f)
                        errors.Add($"{nameof(AIPerceptionDefinition.StealthDetection)} must be between 0 and 1.");
                    if (definition.AlertnessDecay < 0f)
                        errors.Add($"{nameof(AIPerceptionDefinition.AlertnessDecay)} must be greater than or equal to 0.");
                    if (definition.MemoryDuration < 0f)
                        errors.Add($"{nameof(AIPerceptionDefinition.MemoryDuration)} must be greater than or equal to 0.");

                    return errors;
                });
    }

    protected override IEnumerable<string> GetValidationDependencyErrors()
    {
        if (StatRegistry.Instance == null)
            yield return "Missing dependency: StatRegistry.Instance is null.";
    }
}
