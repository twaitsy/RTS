using System.Collections.Generic;
using UnityEngine;

public class UnitRegistry : DefinitionRegistry<UnitDefinition>
{
    private static RegistrySchema<UnitDefinition> schema;

    public static UnitRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple UnitRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }

    protected override RegistrySchema<UnitDefinition> GetSchema()
    {
        if (schema != null)
            return schema;

        schema = new RegistrySchema<UnitDefinition>()
            .RequireField(nameof(UnitDefinition.Id), definition => definition.Id)
            .RequireField(nameof(UnitDefinition.DisplayName), definition => definition.DisplayName)
            .RequireField(nameof(UnitDefinition.Stats), definition => definition.Stats)
            .OptionalField(nameof(UnitDefinition.WeaponTypeId), definition => definition.WeaponTypeId)
            .OptionalField(nameof(UnitDefinition.ArmorTypeId), definition => definition.ArmorTypeId)
            .OptionalField(nameof(UnitDefinition.RoleId), definition => definition.RoleId)
            .AddReference(
                $"{nameof(UnitDefinition.Stats)}.{nameof(SerializedStatContainer.Entries)}",
                definition => RegistrySchema<UnitDefinition>.ReferenceCollection(definition.Stats.Entries, stat => stat.StatId),
                false,
                new ReferenceTargetRule(nameof(StatRegistry), targetId => StatRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(
                nameof(UnitDefinition.StatModifiers),
                definition => RegistrySchema<UnitDefinition>.ReferenceCollection(definition.StatModifiers, modifier => modifier.targetStatId),
                false,
                new ReferenceTargetRule(nameof(StatRegistry), targetId => StatRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.WeaponTypeId), definition => RegistrySchema<UnitDefinition>.SingleReference(definition.WeaponTypeId), false, new ReferenceTargetRule(nameof(WeaponTypeRegistry), targetId => WeaponTypeRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.ArmorTypeId), definition => RegistrySchema<UnitDefinition>.SingleReference(definition.ArmorTypeId), false, new ReferenceTargetRule(nameof(ArmorTypeRegistry), targetId => ArmorTypeRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.RoleId), definition => RegistrySchema<UnitDefinition>.SingleReference(definition.RoleId), false, new ReferenceTargetRule(nameof(RoleRegistry), targetId => RoleRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.TraitIds), definition => definition.TraitIds, false, new ReferenceTargetRule(nameof(TraitRegistry), targetId => TraitRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.StartingSkillIds), definition => definition.StartingSkillIds, false, new ReferenceTargetRule(nameof(SkillRegistry), targetId => SkillRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.NeedsProfileId), definition => RegistrySchema<UnitDefinition>.SingleReference(definition.NeedsProfileId), false, new ReferenceTargetRule(nameof(CivilianNeedsProfileRegistry), targetId => CivilianNeedsProfileRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.MoodModifierIds), definition => definition.MoodModifierIds, false, new ReferenceTargetRule(nameof(MoodRegistry), targetId => MoodRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.StartingItemIds), definition => definition.StartingItemIds, false, new ReferenceTargetRule(nameof(ItemRegistry), targetId => ItemRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.JobIds), definition => definition.JobIds, false, new ReferenceTargetRule(nameof(JobRegistry), targetId => JobRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.AIGoalIds), definition => definition.AIGoalIds, false, new ReferenceTargetRule(nameof(AIGoalRegistry), targetId => AIGoalRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.AIPriorityId), definition => RegistrySchema<UnitDefinition>.SingleReference(definition.AIPriorityId), false, new ReferenceTargetRule(nameof(AIPriorityRegistry), targetId => AIPriorityRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.PerceptionProfileId), definition => RegistrySchema<UnitDefinition>.SingleReference(definition.PerceptionProfileId), false, new ReferenceTargetRule(nameof(AIPerceptionRegistry), targetId => AIPerceptionRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.DefaultFactionId), definition => RegistrySchema<UnitDefinition>.SingleReference(definition.DefaultFactionId), false, new ReferenceTargetRule(nameof(FactionRegistry), targetId => FactionRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.Costs), definition => RegistrySchema<UnitDefinition>.ReferenceCollection(definition.Costs, amount => amount.ResourceId), false, new ReferenceTargetRule(nameof(ResourceRegistry), targetId => ResourceRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.UpkeepCosts), definition => RegistrySchema<UnitDefinition>.ReferenceCollection(definition.UpkeepCosts, amount => amount.ResourceId), false, new ReferenceTargetRule(nameof(ResourceRegistry), targetId => ResourceRegistry.Instance.TryGet(targetId, out _)));

        return schema;
    }

    protected override void ValidateDefinitions(List<UnitDefinition> defs, System.Action<string> reportError)
    {
        // Intentionally reserved for bespoke Unit validation rules.
    }

    protected override IEnumerable<string> GetValidationDependencyErrors()
    {
        if (StatRegistry.Instance == null) yield return "Missing dependency: StatRegistry.Instance is null.";
        if (WeaponTypeRegistry.Instance == null) yield return "Missing dependency: WeaponTypeRegistry.Instance is null.";
        if (ArmorTypeRegistry.Instance == null) yield return "Missing dependency: ArmorTypeRegistry.Instance is null.";
        if (RoleRegistry.Instance == null) yield return "Missing dependency: RoleRegistry.Instance is null.";
        if (TraitRegistry.Instance == null) yield return "Missing dependency: TraitRegistry.Instance is null.";
        if (SkillRegistry.Instance == null) yield return "Missing dependency: SkillRegistry.Instance is null.";
        if (CivilianNeedsProfileRegistry.Instance == null) yield return "Missing dependency: CivilianNeedsProfileRegistry.Instance is null.";
        if (MoodRegistry.Instance == null) yield return "Missing dependency: MoodRegistry.Instance is null.";
        if (ItemRegistry.Instance == null) yield return "Missing dependency: ItemRegistry.Instance is null.";
        if (JobRegistry.Instance == null) yield return "Missing dependency: JobRegistry.Instance is null.";
        if (AIGoalRegistry.Instance == null) yield return "Missing dependency: AIGoalRegistry.Instance is null.";
        if (AIPriorityRegistry.Instance == null) yield return "Missing dependency: AIPriorityRegistry.Instance is null.";
        if (AIPerceptionRegistry.Instance == null) yield return "Missing dependency: AIPerceptionRegistry.Instance is null.";
        if (FactionRegistry.Instance == null) yield return "Missing dependency: FactionRegistry.Instance is null.";
        if (ResourceRegistry.Instance == null) yield return "Missing dependency: ResourceRegistry.Instance is null.";
    }
}
