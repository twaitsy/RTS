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

    public static IReadOnlyCollection<string> GetReferenceFieldPaths()
    {
        return GetOrCreateSchema().GetReferenceFieldNames();
    }

    protected override RegistrySchema<UnitDefinition> GetSchema()
    {
        return GetOrCreateSchema();
    }

    private static RegistrySchema<UnitDefinition> GetOrCreateSchema()
    {
        if (schema != null)
            return schema;

        schema = new RegistrySchema<UnitDefinition>()
            .RequireField(nameof(UnitDefinition.Id), definition => definition.Id)
            .RequireField(nameof(UnitDefinition.DisplayName), definition => definition.DisplayName)
            .RequireField(nameof(UnitDefinition.Stats), definition => definition.Stats)
            .OptionalField(nameof(UnitDefinition.WeaponIds), definition => definition.WeaponIds)
            .OptionalField(nameof(UnitDefinition.ArmorProfileId), definition => definition.ArmorProfileId)
            .OptionalField(nameof(UnitDefinition.DefenseProfileId), definition => definition.DefenseProfileId)
            .OptionalField(nameof(UnitDefinition.WeaponTypeId), definition => definition.WeaponTypeId)
            .OptionalField(nameof(UnitDefinition.ArmorTypeId), definition => definition.ArmorTypeId)
            .OptionalField(nameof(UnitDefinition.MovementProfileId), definition => definition.MovementProfileId)
            .OptionalField(nameof(UnitDefinition.LocomotionProfileId), definition => definition.LocomotionProfileId)
            .OptionalField(nameof(UnitDefinition.ProductionProfileId), definition => definition.ProductionProfileId)
            .OptionalField(nameof(UnitDefinition.RequiredBuildingIds), definition => definition.RequiredBuildingIds)
            .OptionalField(nameof(UnitDefinition.RequiredTechIds), definition => definition.RequiredTechIds)
            .OptionalField(nameof(UnitDefinition.MoodProfileId), definition => definition.MoodProfileId)
            .OptionalField(nameof(UnitDefinition.JobProfileIds), definition => definition.JobProfileIds)
            .OptionalField(nameof(UnitDefinition.AIBehaviorProfileId), definition => definition.AIBehaviorProfileId)
            .OptionalField(nameof(UnitDefinition.UnitCategoryId), definition => definition.UnitCategoryId)
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
            .AddReference(nameof(UnitDefinition.WeaponIds), definition => definition.WeaponIds, false, new ReferenceTargetRule(nameof(WeaponRegistry), targetId => WeaponRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.ArmorProfileId), definition => RegistrySchema<UnitDefinition>.SingleReference(definition.ArmorProfileId), false, new ReferenceTargetRule(nameof(ArmorProfileRegistry), targetId => ArmorProfileRegistry.Instance != null && ArmorProfileRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.DefenseProfileId), definition => RegistrySchema<UnitDefinition>.SingleReference(definition.DefenseProfileId), false, new ReferenceTargetRule(nameof(DefenseProfileRegistry), targetId => DefenseProfileRegistry.Instance != null && DefenseProfileRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.WeaponTypeId), definition => RegistrySchema<UnitDefinition>.SingleReference(definition.WeaponTypeId), false, new ReferenceTargetRule(nameof(WeaponTypeRegistry), targetId => WeaponTypeRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.ArmorTypeId), definition => RegistrySchema<UnitDefinition>.SingleReference(definition.ArmorTypeId), false, new ReferenceTargetRule(nameof(ArmorTypeRegistry), targetId => ArmorTypeRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.MovementProfileId), definition => RegistrySchema<UnitDefinition>.SingleReference(definition.MovementProfileId), false, new ReferenceTargetRule(nameof(MovementProfileRegistry), targetId => MovementProfileRegistry.Instance != null && MovementProfileRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.LocomotionProfileId), definition => RegistrySchema<UnitDefinition>.SingleReference(definition.LocomotionProfileId), false, new ReferenceTargetRule(nameof(LocomotionProfileRegistry), targetId => LocomotionProfileRegistry.Instance != null && LocomotionProfileRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.ProductionProfileId), definition => RegistrySchema<UnitDefinition>.SingleReference(definition.ProductionProfileId), false, new ReferenceTargetRule(nameof(ProductionProfileRegistry), targetId => ProductionProfileRegistry.Instance != null && ProductionProfileRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.RequiredBuildingIds), definition => definition.RequiredBuildingIds, false, new ReferenceTargetRule(nameof(BuildingRegistry), targetId => BuildingRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.RequiredTechIds), definition => definition.RequiredTechIds, false, new ReferenceTargetRule(nameof(TechRegistry), targetId => TechRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.RoleId), definition => RegistrySchema<UnitDefinition>.SingleReference(definition.RoleId), false, new ReferenceTargetRule(nameof(RoleRegistry), targetId => RoleRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.TraitIds), definition => definition.TraitIds, false, new ReferenceTargetRule(nameof(TraitRegistry), targetId => TraitRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.StartingSkillIds), definition => definition.StartingSkillIds, false, new ReferenceTargetRule(nameof(SkillRegistry), targetId => SkillRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.NeedsProfileId), definition => RegistrySchema<UnitDefinition>.SingleReference(definition.NeedsProfileId), false, new ReferenceTargetRule(nameof(NeedsProfileRegistry), targetId => NeedsProfileRegistry.Instance != null && NeedsProfileRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.MoodProfileId), definition => RegistrySchema<UnitDefinition>.SingleReference(definition.MoodProfileId), false, new ReferenceTargetRule(nameof(MoodRegistry), targetId => MoodRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.MoodModifierIds), definition => definition.MoodModifierIds, false, new ReferenceTargetRule(nameof(MoodRegistry), targetId => MoodRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.StartingItemIds), definition => definition.StartingItemIds, false, new ReferenceTargetRule(nameof(ItemRegistry), targetId => ItemRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.JobProfileIds), definition => definition.JobProfileIds, false, new ReferenceTargetRule(nameof(JobRegistry), targetId => JobRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.JobIds), definition => definition.JobIds, false, new ReferenceTargetRule(nameof(JobRegistry), targetId => JobRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.AIBehaviorProfileId), definition => RegistrySchema<UnitDefinition>.SingleReference(definition.AIBehaviorProfileId), false, new ReferenceTargetRule(nameof(BehaviourRegistry), targetId => BehaviourRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.AIGoalIds), definition => definition.AIGoalIds, false, new ReferenceTargetRule(nameof(AIGoalRegistry), targetId => AIGoalRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.AIPriorityId), definition => RegistrySchema<UnitDefinition>.SingleReference(definition.AIPriorityId), false, new ReferenceTargetRule(nameof(AIPriorityRegistry), targetId => AIPriorityRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.PerceptionProfileId), definition => RegistrySchema<UnitDefinition>.SingleReference(definition.PerceptionProfileId), false, new ReferenceTargetRule(nameof(AIPerceptionRegistry), targetId => AIPerceptionRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.UnitCategoryId), definition => RegistrySchema<UnitDefinition>.SingleReference(definition.UnitCategoryId), false, new ReferenceTargetRule(nameof(UnitCategoryRegistry), targetId => UnitCategoryRegistry.Instance != null && UnitCategoryRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.DefaultFactionId), definition => RegistrySchema<UnitDefinition>.SingleReference(definition.DefaultFactionId), false, new ReferenceTargetRule(nameof(FactionRegistry), targetId => FactionRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.Costs), definition => RegistrySchema<UnitDefinition>.ReferenceCollection(definition.Costs, amount => amount.ResourceId), false, new ReferenceTargetRule(nameof(ResourceRegistry), targetId => ResourceRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.UpkeepCosts), definition => RegistrySchema<UnitDefinition>.ReferenceCollection(definition.UpkeepCosts, amount => amount.ResourceId), false, new ReferenceTargetRule(nameof(ResourceRegistry), targetId => ResourceRegistry.Instance.TryGet(targetId, out _)))
            .AddConstraint("UnitSimulationRelationalConstraints", ValidateSimulationRelationalConstraints);

        return schema;
    }

    protected override void ValidateDefinitions(List<UnitDefinition> defs, System.Action<string> reportError)
    {
        // Validation handled by schema constraints.
    }

    private static IEnumerable<string> ValidateSimulationRelationalConstraints(UnitDefinition definition)
    {
        if (definition == null)
            yield break;

        if (HasStat(definition, CanonicalStatIds.Combat.AttackSpeed) && (definition.WeaponIds == null || definition.WeaponIds.Count == 0))
            yield return $"{nameof(UnitDefinition.WeaponIds)} is required when '{CanonicalStatIds.Combat.AttackSpeed}' is present.";

        if (HasStat(definition, CanonicalStatIds.Movement.MoveSpeed) && string.IsNullOrWhiteSpace(definition.MovementProfileId))
            yield return $"{nameof(UnitDefinition.MovementProfileId)} is required when '{CanonicalStatIds.Movement.MoveSpeed}' is present.";

        if (HasStat(definition, CanonicalStatIds.Needs.HungerRate) && string.IsNullOrWhiteSpace(definition.NeedsProfileId))
            yield return $"{nameof(UnitDefinition.NeedsProfileId)} is required when '{CanonicalStatIds.Needs.HungerRate}' is present.";

        if (HasStat(definition, CanonicalStatIds.Perception.PerceptionRadius) && string.IsNullOrWhiteSpace(definition.PerceptionProfileId))
            yield return $"{nameof(UnitDefinition.PerceptionProfileId)} is required when '{CanonicalStatIds.Perception.PerceptionRadius}' is present.";

        if (HasCosts(definition) && string.IsNullOrWhiteSpace(definition.ProductionProfileId))
            yield return $"{nameof(UnitDefinition.ProductionProfileId)} is required when {nameof(UnitDefinition.Costs)} contains resource costs.";
    }

    private static bool HasStat(UnitDefinition definition, string statId)
    {
        return definition?.Stats != null && definition.Stats.TryGetValue(statId, out _);
    }

    private static bool HasCosts(UnitDefinition definition)
    {
        if (definition?.Costs == null)
            return false;

        foreach (var cost in definition.Costs)
        {
            if (cost.Amount > 0)
                return true;
        }

        return false;
    }

    protected override IEnumerable<string> GetValidationDependencyErrors()
    {
        if (StatRegistry.Instance == null) yield return "Missing dependency: StatRegistry.Instance is null.";
        if (WeaponRegistry.Instance == null) yield return "Missing dependency: WeaponRegistry.Instance is null.";
        if (DefenseProfileRegistry.Instance == null) yield return "Missing dependency: DefenseProfileRegistry.Instance is null.";
        if (WeaponTypeRegistry.Instance == null) yield return "Missing dependency: WeaponTypeRegistry.Instance is null.";
        if (ArmorProfileRegistry.Instance == null) yield return "Missing dependency: ArmorProfileRegistry.Instance is null.";
        if (ArmorTypeRegistry.Instance == null) yield return "Missing dependency: ArmorTypeRegistry.Instance is null.";
        if (MovementProfileRegistry.Instance == null) yield return "Missing dependency: MovementProfileRegistry.Instance is null.";
        if (LocomotionProfileRegistry.Instance == null) yield return "Missing dependency: LocomotionProfileRegistry.Instance is null.";
        if (ProductionProfileRegistry.Instance == null) yield return "Missing dependency: ProductionProfileRegistry.Instance is null.";
        if (BuildingRegistry.Instance == null) yield return "Missing dependency: BuildingRegistry.Instance is null.";
        if (TechRegistry.Instance == null) yield return "Missing dependency: TechRegistry.Instance is null.";
        if (UnitCategoryRegistry.Instance == null) yield return "Missing dependency: UnitCategoryRegistry.Instance is null.";
        if (RoleRegistry.Instance == null) yield return "Missing dependency: RoleRegistry.Instance is null.";
        if (TraitRegistry.Instance == null) yield return "Missing dependency: TraitRegistry.Instance is null.";
        if (SkillRegistry.Instance == null) yield return "Missing dependency: SkillRegistry.Instance is null.";
        if (NeedsProfileRegistry.Instance == null) yield return "Missing dependency: NeedsProfileRegistry.Instance is null.";
        if (MoodRegistry.Instance == null) yield return "Missing dependency: MoodRegistry.Instance is null.";
        if (ItemRegistry.Instance == null) yield return "Missing dependency: ItemRegistry.Instance is null.";
        if (JobRegistry.Instance == null) yield return "Missing dependency: JobRegistry.Instance is null.";
        if (BehaviourRegistry.Instance == null) yield return "Missing dependency: BehaviourRegistry.Instance is null.";
        if (AIGoalRegistry.Instance == null) yield return "Missing dependency: AIGoalRegistry.Instance is null.";
        if (AIPriorityRegistry.Instance == null) yield return "Missing dependency: AIPriorityRegistry.Instance is null.";
        if (AIPerceptionRegistry.Instance == null) yield return "Missing dependency: AIPerceptionRegistry.Instance is null.";
        if (FactionRegistry.Instance == null) yield return "Missing dependency: FactionRegistry.Instance is null.";
        if (ResourceRegistry.Instance == null) yield return "Missing dependency: ResourceRegistry.Instance is null.";
    }
}
