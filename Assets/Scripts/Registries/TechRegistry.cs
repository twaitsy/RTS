using System.Collections.Generic;
using UnityEngine;

public class TechRegistry : DefinitionRegistry<TechDefinition>
{
    private static readonly HashSet<StatDomain> AnyDomain = new();
    private static RegistrySchema<TechDefinition> schema;

    public static TechRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple TechRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }

    public static IReadOnlyCollection<string> GetReferenceFieldPaths()
    {
        return GetOrCreateSchema().GetReferenceFieldNames();
    }

    protected override RegistrySchema<TechDefinition> GetSchema()
    {
        return GetOrCreateSchema();
    }

    private static RegistrySchema<TechDefinition> GetOrCreateSchema()
    {
        if (schema != null)
            return schema;

        schema = new RegistrySchema<TechDefinition>()
            .RequireField(nameof(TechDefinition.Id), definition => definition.Id)
            .RequireField(nameof(TechDefinition.Stats), definition => definition.Stats)
            .OptionalField(nameof(TechDefinition.StatModifiers), definition => definition.StatModifiers)
            .OptionalField(nameof(TechDefinition.RequiredTechIds), definition => definition.RequiredTechIds)
            .OptionalField(nameof(TechDefinition.Costs), definition => definition.Costs)
            .AddReference(
                $"{nameof(TechDefinition.Stats)}.{nameof(SerializedStatContainer.Entries)}",
                definition => RegistrySchema<TechDefinition>.ReferenceCollection(definition.Stats.Entries, stat => stat.StatId),
                false,
                new ReferenceTargetRule(nameof(StatRegistry), targetId => StatRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(
                nameof(TechDefinition.StatModifiers),
                definition => RegistrySchema<TechDefinition>.ReferenceCollection(definition.StatModifiers, modifier => modifier.targetStatId),
                false,
                new ReferenceTargetRule(nameof(StatRegistry), targetId => StatRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(
                nameof(TechDefinition.RequiredTechIds),
                definition => definition.RequiredTechIds,
                false,
                new ReferenceTargetRule(nameof(TechRegistry), targetId => TechRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(
                nameof(TechDefinition.Costs),
                definition => RegistrySchema<TechDefinition>.ReferenceCollection(definition.Costs, amount => amount.ResourceId),
                false,
                new ReferenceTargetRule(nameof(ResourceRegistry), targetId => ResourceRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(
                nameof(TechDefinition.StatModifierIds),
                definition => definition.StatModifierIds,
                false,
                new ReferenceTargetRule(nameof(StatModifierRegistry), targetId => StatModifierRegistry.Instance.TryGet(targetId, out _)));

        return schema;
    }

    protected override void ValidateDefinitions(List<TechDefinition> defs, System.Action<string> reportError)
    {
        StatModifierLinkValidator.ValidateHostStatModifierLinks(
            defs,
            definition => definition.Id,
            definition => definition.StatModifierIds,
            definition => definition.name,
            _ => true,
            modifierId => StatModifierRegistry.Instance.TryGet(modifierId, out _),
            modifierId => StatModifierRegistry.Instance.Get(modifierId),
            statId => StatRegistry.Instance.TryGet(statId, out _),
            statId => StatRegistry.Instance.Get(statId),
            AnyDomain,
            "any domain",
            reportError);
    }

    protected override IEnumerable<string> GetValidationDependencyErrors()
    {
        if (StatModifierRegistry.Instance == null)
            yield return "Missing dependency: StatModifierRegistry.Instance is null.";
        if (StatRegistry.Instance == null)
            yield return "Missing dependency: StatRegistry.Instance is null.";
        if (ResourceRegistry.Instance == null)
            yield return "Missing dependency: ResourceRegistry.Instance is null.";
    }
}
