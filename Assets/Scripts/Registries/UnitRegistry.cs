using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitRegistry : DefinitionRegistry<UnitDefinition>
{
    private static readonly string[] MovementStatSet =
    {
        CanonicalStatIds.Movement.MoveSpeed,
        CanonicalStatIds.Movement.Acceleration,
        CanonicalStatIds.Movement.TurnRate,
    };

    private static readonly string[] CombatStatSet =
    {
        CanonicalStatIds.Combat.Health,
        CanonicalStatIds.Combat.AttackDamage,
        CanonicalStatIds.Combat.AttackSpeed,
        CanonicalStatIds.Combat.AttackRange,
    };

    private static readonly string[] NeedsStatSet =
    {
        CanonicalStatIds.Needs.HungerRate,
        CanonicalStatIds.Needs.ThirstRate,
    };

    private static readonly string[] PerceptionStatSet =
    {
        CanonicalStatIds.Perception.PerceptionRadius,
        CanonicalStatIds.Perception.HearingRadius,
    };

    private static readonly string[] ProductionWorkStatSet =
    {
        CanonicalStatIds.Production.WorkSpeed,
        CanonicalStatIds.Production.CarryCapacity,
    };

    private static readonly string[] AiStatSet =
    {
        CanonicalStatIds.AI.Aggression,
        CanonicalStatIds.AI.Courage,
        CanonicalStatIds.AI.Obedience,
    };

    private static readonly string[] EconomyStatSet =
    {
        CanonicalStatIds.Economy.UpkeepRate,
        CanonicalStatIds.Economy.PopulationCost,
    };

    private static readonly string[] LifecycleStatSet =
    {
        CanonicalStatIds.Lifecycle.XPPerKill,
        CanonicalStatIds.Lifecycle.MaxLevel,
    };

    private static readonly Dictionary<string, string[]> RequiredStatSetsByName = new(StringComparer.OrdinalIgnoreCase)
    {
        ["movement"] = MovementStatSet,
        ["combat"] = CombatStatSet,
        ["needs"] = NeedsStatSet,
        ["perception"] = PerceptionStatSet,
        ["production/work"] = ProductionWorkStatSet,
        ["ai"] = AiStatSet,
        ["economy"] = EconomyStatSet,
        ["lifecycle"] = LifecycleStatSet,
    };

    private static readonly Dictionary<string, string[]> RequiredSetNamesByMode = new(StringComparer.OrdinalIgnoreCase)
    {
        ["baseline"] = new[] { "movement", "ai", "economy", "lifecycle" },
        ["combatant"] = new[] { "movement", "combat", "perception", "ai", "economy", "lifecycle" },
        ["worker"] = new[] { "movement", "needs", "perception", "production/work", "ai", "economy", "lifecycle" },
        ["civilian"] = new[] { "movement", "needs", "perception", "ai", "economy", "lifecycle" },
        ["scout"] = new[] { "movement", "perception", "ai", "economy", "lifecycle" },
    };

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
            .RequireField(nameof(UnitDefinition.SchemaModeId), definition => definition.SchemaModeId)
            .OptionalField(nameof(UnitDefinition.WeaponIds), definition => definition.WeaponIds)
            .OptionalField(nameof(UnitDefinition.ArmorProfileId), definition => definition.ArmorProfileId)
            .OptionalField(nameof(UnitDefinition.DefenseProfileId), definition => definition.DefenseProfileId)
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
                new ReferenceTargetRule(nameof(StatRegistry), targetId => StatRegistry.Instance != null && StatRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(
                nameof(UnitDefinition.StatModifiers),
                definition => RegistrySchema<UnitDefinition>.ReferenceCollection(definition.StatModifiers, modifier => modifier.targetStatId),
                false,
                new ReferenceTargetRule(nameof(StatRegistry), targetId => StatRegistry.Instance != null && StatRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.WeaponIds), definition => definition.WeaponIds, false, new ReferenceTargetRule(nameof(WeaponRegistry), targetId => WeaponRegistry.Instance != null && WeaponRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.ArmorProfileId), definition => RegistrySchema<UnitDefinition>.SingleReference(definition.ArmorProfileId), false, new ReferenceTargetRule(nameof(ArmorProfileRegistry), targetId => ArmorProfileRegistry.Instance != null && ArmorProfileRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.DefenseProfileId), definition => RegistrySchema<UnitDefinition>.SingleReference(definition.DefenseProfileId), false, new ReferenceTargetRule(nameof(DefenseProfileRegistry), targetId => DefenseProfileRegistry.Instance != null && DefenseProfileRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.MovementProfileId), definition => RegistrySchema<UnitDefinition>.SingleReference(definition.MovementProfileId), false, new ReferenceTargetRule(nameof(MovementProfileRegistry), targetId => MovementProfileRegistry.Instance != null && MovementProfileRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.LocomotionProfileId), definition => RegistrySchema<UnitDefinition>.SingleReference(definition.LocomotionProfileId), false, new ReferenceTargetRule(nameof(LocomotionProfileRegistry), targetId => LocomotionProfileRegistry.Instance != null && LocomotionProfileRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.ProductionProfileId), definition => RegistrySchema<UnitDefinition>.SingleReference(definition.ProductionProfileId), false, new ReferenceTargetRule(nameof(ProductionProfileRegistry), targetId => ProductionProfileRegistry.Instance != null && ProductionProfileRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.RequiredBuildingIds), definition => definition.RequiredBuildingIds, false, new ReferenceTargetRule(nameof(BuildingRegistry), targetId => BuildingRegistry.Instance != null && BuildingRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.RequiredTechIds), definition => definition.RequiredTechIds, false, new ReferenceTargetRule(nameof(TechRegistry), targetId => TechRegistry.Instance != null && TechRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.RoleId), definition => RegistrySchema<UnitDefinition>.SingleReference(definition.RoleId), false, new ReferenceTargetRule(nameof(RoleRegistry), targetId => RoleRegistry.Instance != null && RoleRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.TraitIds), definition => definition.TraitIds, false, new ReferenceTargetRule(nameof(TraitRegistry), targetId => TraitRegistry.Instance != null && TraitRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.StartingSkillIds), definition => definition.StartingSkillIds, false, new ReferenceTargetRule(nameof(SkillRegistry), targetId => SkillRegistry.Instance != null && SkillRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.NeedsProfileId), definition => RegistrySchema<UnitDefinition>.SingleReference(definition.NeedsProfileId), false, new ReferenceTargetRule(nameof(NeedsProfileRegistry), targetId => NeedsProfileRegistry.Instance != null && NeedsProfileRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.MoodProfileId), definition => RegistrySchema<UnitDefinition>.SingleReference(definition.MoodProfileId), false, new ReferenceTargetRule(nameof(MoodRegistry), targetId => MoodRegistry.Instance != null && MoodRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.MoodModifierIds), definition => definition.MoodModifierIds, false, new ReferenceTargetRule(nameof(MoodRegistry), targetId => MoodRegistry.Instance != null && MoodRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.StartingItemIds), definition => definition.StartingItemIds, false, new ReferenceTargetRule(nameof(ItemRegistry), targetId => ItemRegistry.Instance != null && ItemRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.JobProfileIds), definition => definition.JobProfileIds, false, new ReferenceTargetRule(nameof(JobRegistry), targetId => JobRegistry.Instance != null && JobRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.JobIds), definition => definition.JobIds, false, new ReferenceTargetRule(nameof(JobRegistry), targetId => JobRegistry.Instance != null && JobRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.AIBehaviorProfileId), definition => RegistrySchema<UnitDefinition>.SingleReference(definition.AIBehaviorProfileId), false, new ReferenceTargetRule(nameof(BehaviourRegistry), targetId => BehaviourRegistry.Instance != null && BehaviourRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.AIGoalIds), definition => definition.AIGoalIds, false, new ReferenceTargetRule(nameof(AIGoalRegistry), targetId => AIGoalRegistry.Instance != null && AIGoalRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.AIPriorityId), definition => RegistrySchema<UnitDefinition>.SingleReference(definition.AIPriorityId), false, new ReferenceTargetRule(nameof(AIPriorityRegistry), targetId => AIPriorityRegistry.Instance != null && AIPriorityRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.PerceptionProfileId), definition => RegistrySchema<UnitDefinition>.SingleReference(definition.PerceptionProfileId), false, new ReferenceTargetRule(nameof(AIPerceptionRegistry), targetId => AIPerceptionRegistry.Instance != null && AIPerceptionRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.UnitCategoryId), definition => RegistrySchema<UnitDefinition>.SingleReference(definition.UnitCategoryId), false, new ReferenceTargetRule(nameof(UnitCategoryRegistry), targetId => UnitCategoryRegistry.Instance != null && UnitCategoryRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.DefaultFactionId), definition => RegistrySchema<UnitDefinition>.SingleReference(definition.DefaultFactionId), false, new ReferenceTargetRule(nameof(FactionRegistry), targetId => FactionRegistry.Instance != null && FactionRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.Costs), definition => RegistrySchema<UnitDefinition>.ReferenceCollection(definition.Costs, amount => amount.ResourceId), false, new ReferenceTargetRule(nameof(ResourceRegistry), targetId => ResourceRegistry.Instance != null && ResourceRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(nameof(UnitDefinition.UpkeepCosts), definition => RegistrySchema<UnitDefinition>.ReferenceCollection(definition.UpkeepCosts, amount => amount.ResourceId), false, new ReferenceTargetRule(nameof(ResourceRegistry), targetId => ResourceRegistry.Instance != null && ResourceRegistry.Instance.TryGet(targetId, out _)))
            .AddConstraint("UnitSchemaRequirements", ValidateRequiredStatSets)
            .AddConstraint("UnitSimulationRelationalConstraints", ValidateSimulationRelationalConstraints);

        return schema;
    }

    protected override void ValidateDefinitions(List<UnitDefinition> defs, Action<string> reportError)
    {
        // Validation handled by schema constraints.
    }

    private static IEnumerable<string> ValidateRequiredStatSets(UnitDefinition definition)
    {
        if (definition == null)
            yield break;

        var schemaMode = ResolveSchemaMode(definition, out var modeSource);
        if (!RequiredSetNamesByMode.TryGetValue(schemaMode, out var requiredSetNames))
        {
            yield return $"Unknown schema mode '{definition.SchemaModeId}'. Allowed values: {string.Join(", ", RequiredSetNamesByMode.Keys.OrderBy(value => value, StringComparer.OrdinalIgnoreCase))}.";
            yield break;
        }

        var missingStatsBySet = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var setName in requiredSetNames)
        {
            if (!RequiredStatSetsByName.TryGetValue(setName, out var requiredStats))
                continue;

            foreach (var missingStatId in requiredStats.Where(statId => !HasStat(definition, statId)))
            {
                if (!missingStatsBySet.TryGetValue(setName, out var missingInSet))
                {
                    missingInSet = new List<string>();
                    missingStatsBySet[setName] = missingInSet;
                }

                missingInSet.Add(missingStatId);
            }
        }

        if (missingStatsBySet.Count > 0)
        {
            var setSummaries = missingStatsBySet
                .Select(pair => $"{pair.Key} => [{string.Join(", ", pair.Value.Distinct(StringComparer.Ordinal))}]")
                .OrderBy(summary => summary, StringComparer.OrdinalIgnoreCase);

            yield return $"Schema mode '{schemaMode}' ({modeSource}) is missing required stat IDs by set: {string.Join("; ", setSummaries)}.";
        }

        var missingProfiles = new List<string>();
        if (requiredSetNames.Contains("movement") && string.IsNullOrWhiteSpace(definition.MovementProfileId))
            missingProfiles.Add(nameof(UnitDefinition.MovementProfileId));
        if (requiredSetNames.Contains("needs") && string.IsNullOrWhiteSpace(definition.NeedsProfileId))
            missingProfiles.Add(nameof(UnitDefinition.NeedsProfileId));
        if (requiredSetNames.Contains("perception") && string.IsNullOrWhiteSpace(definition.PerceptionProfileId))
            missingProfiles.Add(nameof(UnitDefinition.PerceptionProfileId));
        if (requiredSetNames.Contains("production/work") && string.IsNullOrWhiteSpace(definition.ProductionProfileId))
            missingProfiles.Add(nameof(UnitDefinition.ProductionProfileId));

        if (missingProfiles.Count > 0)
        {
            yield return $"Schema mode '{schemaMode}' ({modeSource}) is missing required profile links: {string.Join(", ", missingProfiles)}.";
        }

        const string legacyPerceptionId = "ai.perceptionRadius";
        if (HasStat(definition, legacyPerceptionId))
            yield return $"Legacy stat id '{legacyPerceptionId}' is not allowed. Use '{CanonicalStatIds.Perception.PerceptionRadius}'.";
    }

    private static IEnumerable<string> ValidateSimulationRelationalConstraints(UnitDefinition definition)
    {
        if (definition == null)
            yield break;

        if (HasStatInRelationalGraph(definition, CanonicalStatIds.Combat.AttackSpeed) && (definition.WeaponIds == null || definition.WeaponIds.Count == 0))
            yield return $"{nameof(UnitDefinition.WeaponIds)} is required when '{CanonicalStatIds.Combat.AttackSpeed}' is present.";

        if (HasStatInRelationalGraph(definition, CanonicalStatIds.Movement.MoveSpeed) && string.IsNullOrWhiteSpace(definition.MovementProfileId))
            yield return $"{nameof(UnitDefinition.MovementProfileId)} is required when '{CanonicalStatIds.Movement.MoveSpeed}' is present.";

        if (HasStatInRelationalGraph(definition, CanonicalStatIds.Needs.HungerRate) && string.IsNullOrWhiteSpace(definition.NeedsProfileId))
            yield return $"{nameof(UnitDefinition.NeedsProfileId)} is required when '{CanonicalStatIds.Needs.HungerRate}' is present.";

        if (HasStatInRelationalGraph(definition, CanonicalStatIds.Perception.PerceptionRadius) && string.IsNullOrWhiteSpace(definition.PerceptionProfileId))
            yield return $"{nameof(UnitDefinition.PerceptionProfileId)} is required when '{CanonicalStatIds.Perception.PerceptionRadius}' is present.";

        if (HasCosts(definition) && string.IsNullOrWhiteSpace(definition.ProductionProfileId))
            yield return $"{nameof(UnitDefinition.ProductionProfileId)} is required when {nameof(UnitDefinition.Costs)} contains resource costs.";

        if (!string.IsNullOrWhiteSpace(definition.WeaponTypeId))
            yield return $"{nameof(UnitDefinition.WeaponTypeId)} is legacy and forbidden. Use {nameof(UnitDefinition.WeaponIds)} with canonical {nameof(WeaponRegistry)} links.";

        if (!string.IsNullOrWhiteSpace(definition.ArmorTypeId))
            yield return $"{nameof(UnitDefinition.ArmorTypeId)} is legacy and forbidden. Use {nameof(UnitDefinition.ArmorProfileId)} with canonical {nameof(ArmorProfileRegistry)} links.";

        foreach (var requiredStatId in GetRoleRequiredStats(definition))
        {
            if (!HasStatInRelationalGraph(definition, requiredStatId))
                yield return $"Missing role-required stat '{requiredStatId}' for role '{definition.RoleId}'.";
        }

        foreach (var requiredStatId in GetCategoryRequiredStats(definition))
        {
            if (!HasStatInRelationalGraph(definition, requiredStatId))
                yield return $"Missing archetype-required stat '{requiredStatId}' for category '{definition.UnitCategoryId}'.";
        }
    }

    private static string ResolveSchemaMode(UnitDefinition definition, out string modeSource)
    {
        var schemaMode = definition?.SchemaModeId?.Trim();
        if (string.IsNullOrWhiteSpace(schemaMode))
        {
            modeSource = "missing";
            return string.Empty;
        }

        modeSource = "unit.schemaModeId";
        return schemaMode;
    }

    private static bool HasStatInRelationalGraph(UnitDefinition definition, string statId)
    {
        if (definition == null || string.IsNullOrWhiteSpace(statId))
            return false;

        var allStats = CollectRelationalStatIds(definition);
        return allStats.Contains(statId);
    }

    private static bool HasStat(UnitDefinition definition, string statId)
    {
        if (definition?.Stats?.Entries == null || string.IsNullOrWhiteSpace(statId))
            return false;

        for (int i = 0; i < definition.Stats.Entries.Count; i++)
        {
            if (string.Equals(definition.Stats.Entries[i].StatId, statId, StringComparison.Ordinal))
                return true;
        }

        return false;
    }

    private static HashSet<string> CollectRelationalStatIds(UnitDefinition definition)
    {
        var statIds = new HashSet<string>(StringComparer.Ordinal);
        if (definition == null)
            return statIds;

        AddStats(statIds, definition.Stats);

        if (definition.WeaponIds != null && WeaponRegistry.Instance != null)
        {
            for (int i = 0; i < definition.WeaponIds.Count; i++)
            {
                var weaponId = definition.WeaponIds[i];
                if (string.IsNullOrWhiteSpace(weaponId))
                    continue;

                if (WeaponRegistry.Instance.TryGet(weaponId, out var weapon))
                    AddStats(statIds, weapon?.Stats);
            }
        }

        if (ArmorProfileRegistry.Instance != null && ArmorProfileRegistry.Instance.TryGet(definition.ArmorProfileId, out var armorProfile))
            AddStats(statIds, armorProfile?.Stats);
        if (DefenseProfileRegistry.Instance != null && DefenseProfileRegistry.Instance.TryGet(definition.DefenseProfileId, out var defenseProfile))
            AddStats(statIds, defenseProfile?.Stats);
        if (MovementProfileRegistry.Instance != null && MovementProfileRegistry.Instance.TryGet(definition.MovementProfileId, out var movementProfile))
            AddStats(statIds, movementProfile?.Stats);
        if (LocomotionProfileRegistry.Instance != null && LocomotionProfileRegistry.Instance.TryGet(definition.LocomotionProfileId, out var locomotionProfile))
            AddStats(statIds, locomotionProfile?.Stats);
        if (NeedsProfileRegistry.Instance != null && NeedsProfileRegistry.Instance.TryGet(definition.NeedsProfileId, out var needsProfile))
            AddStats(statIds, needsProfile?.Stats);
        if (MoodRegistry.Instance != null && MoodRegistry.Instance.TryGet(definition.MoodProfileId, out var moodProfile))
            AddStats(statIds, moodProfile?.Stats);
        if (BehaviourRegistry.Instance != null && BehaviourRegistry.Instance.TryGet(definition.AIBehaviorProfileId, out var behaviourProfile))
            AddStats(statIds, behaviourProfile?.Stats);
        if (AIPerceptionRegistry.Instance != null && AIPerceptionRegistry.Instance.TryGet(definition.PerceptionProfileId, out var perceptionProfile))
            AddStats(statIds, perceptionProfile?.Stats);
        if (ProductionProfileRegistry.Instance != null && ProductionProfileRegistry.Instance.TryGet(definition.ProductionProfileId, out var productionProfile))
            AddStats(statIds, productionProfile?.Stats);

        AddJobStats(definition.JobProfileIds, statIds);
        AddJobStats(definition.JobIds, statIds);
        return statIds;
    }

    private static void AddJobStats(IReadOnlyList<string> jobIds, HashSet<string> statIds)
    {
        if (jobIds == null || JobRegistry.Instance == null)
            return;

        for (int i = 0; i < jobIds.Count; i++)
        {
            var jobId = jobIds[i];
            if (string.IsNullOrWhiteSpace(jobId))
                continue;

            if (JobRegistry.Instance.TryGet(jobId, out var job))
                AddStats(statIds, job?.Stats);
        }
    }

    private static void AddStats(HashSet<string> statIds, SerializedStatContainer stats)
    {
        if (stats?.Entries == null)
            return;

        for (int i = 0; i < stats.Entries.Count; i++)
        {
            var statId = stats.Entries[i].StatId;
            if (!string.IsNullOrWhiteSpace(statId))
                statIds.Add(statId);
        }
    }

    private static IEnumerable<string> GetRoleRequiredStats(UnitDefinition definition)
    {
        if (definition == null || string.IsNullOrWhiteSpace(definition.RoleId) || RoleRegistry.Instance == null)
            yield break;

        if (!RoleRegistry.Instance.TryGet(definition.RoleId, out var role) || role?.RequiredStatIds == null)
            yield break;

        for (int i = 0; i < role.RequiredStatIds.Count; i++)
        {
            var statId = role.RequiredStatIds[i];
            if (!string.IsNullOrWhiteSpace(statId))
                yield return statId;
        }
    }

    private static IEnumerable<string> GetCategoryRequiredStats(UnitDefinition definition)
    {
        if (definition == null || string.IsNullOrWhiteSpace(definition.UnitCategoryId) || UnitCategoryRegistry.Instance == null)
            yield break;

        if (!UnitCategoryRegistry.Instance.TryGet(definition.UnitCategoryId, out var category) || category?.RequiredStatIds == null)
            yield break;

        for (int i = 0; i < category.RequiredStatIds.Count; i++)
        {
            var statId = category.RequiredStatIds[i];
            if (!string.IsNullOrWhiteSpace(statId))
                yield return statId;
        }
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
        if (ArmorProfileRegistry.Instance == null) yield return "Missing dependency: ArmorProfileRegistry.Instance is null.";
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



