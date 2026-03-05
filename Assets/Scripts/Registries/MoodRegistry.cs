using System.Collections.Generic;
using UnityEngine;

public class MoodRegistry : DefinitionRegistry<MoodDefinition>
{
    private static RegistrySchema<MoodDefinition> schema;

    public static MoodRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple MoodRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }

    protected override RegistrySchema<MoodDefinition> GetSchema()
    {
        return schema ??= new RegistrySchema<MoodDefinition>()
            .RequireField(nameof(MoodDefinition.Id), definition => definition.Id)
            .RequireField(nameof(MoodDefinition.DisplayName), definition => definition.DisplayName)
            .RequireField(nameof(MoodDefinition.Metadata), definition => definition.Metadata)
            .RequireField(nameof(MoodDefinition.Stats), definition => definition.Stats)
            .OptionalField(nameof(MoodDefinition.StatModifiers), definition => definition.StatModifiers)
            .OptionalField(nameof(MoodDefinition.PersonalityTraits), definition => definition.PersonalityTraits)
            .AddReference(
                $"{nameof(MoodDefinition.Stats)}.{nameof(SerializedStatContainer.Entries)}",
                definition => RegistrySchema<MoodDefinition>.ReferenceCollection(definition.Stats.Entries, entry => entry.StatId),
                false,
                new ReferenceTargetRule(nameof(StatRegistry), targetId => StatRegistry.Instance != null && StatRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(
                nameof(MoodDefinition.StatModifiers),
                definition => RegistrySchema<MoodDefinition>.ReferenceCollection(definition.StatModifiers, modifier => modifier.targetStatId),
                false,
                new ReferenceTargetRule(nameof(StatRegistry), targetId => StatRegistry.Instance != null && StatRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(
                nameof(MoodDefinition.PersonalityTraits),
                definition => RegistrySchema<MoodDefinition>.ReferenceCollection(definition.PersonalityTraits, id => id),
                false,
                new ReferenceTargetRule(nameof(TraitRegistry), targetId => TraitRegistry.Instance != null && TraitRegistry.Instance.TryGet(targetId, out _)))
            .AddConstraint(
                "MoodConstraints",
                definition =>
                {
                    var errors = new List<string>();
                    if (definition.MoraleStability < 0f)
                        errors.Add($"{nameof(MoodDefinition.MoraleStability)} must be greater than or equal to 0.");
                    if (definition.StressRecoveryRate < 0f)
                        errors.Add($"{nameof(MoodDefinition.StressRecoveryRate)} must be greater than or equal to 0.");
                    if (definition.PanicThreshold < 0f || definition.PanicThreshold > 1f)
                        errors.Add($"{nameof(MoodDefinition.PanicThreshold)} must be between 0 and 1.");

                    var seenTraitIds = new HashSet<string>();
                    var traitIds = definition.PersonalityTraits;
                    if (traitIds == null)
                        return errors;

                    for (var index = 0; index < traitIds.Count; index++)
                    {
                        var traitId = traitIds[index];
                        if (string.IsNullOrWhiteSpace(traitId))
                        {
                            errors.Add($"{nameof(MoodDefinition.PersonalityTraits)}[{index}] must not be empty.");
                            continue;
                        }

                        if (!seenTraitIds.Add(traitId))
                            errors.Add($"{nameof(MoodDefinition.PersonalityTraits)} contains duplicate trait id '{traitId}'.");
                    }

                    return errors;
                });
    }

    protected override IEnumerable<string> GetValidationDependencyErrors()
    {
        if (StatRegistry.Instance == null)
            yield return "Missing dependency: StatRegistry.Instance is null.";
        if (TraitRegistry.Instance == null)
            yield return "Missing dependency: TraitRegistry.Instance is null.";
    }
}
