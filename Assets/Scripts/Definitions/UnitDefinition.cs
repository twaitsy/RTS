using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/Unit")]
public class UnitDefinition : ScriptableObject, IIdentifiable, IDefinitionMetadataProvider
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private DefinitionMetadata metadata = DefinitionMetadata.Create(DefinitionCategory.Unit);
    public DefinitionMetadata Metadata => metadata;

    [SerializeField, HideInInspector] private bool isIdFinalized;
    [SerializeField, HideInInspector] private string finalizedId;

    [SerializeField] private string displayName;
    public string DisplayName => displayName;

    [SerializeField] private Sprite icon;
    public Sprite Icon => icon;

    [Header("Stats")]
    [FormerlySerializedAs("baseStats")]
    [SerializeField] private SerializedStatContainer stats = new();
    public SerializedStatContainer Stats => stats;
    public IReadOnlyList<StatEntry> BaseStats => stats.Entries;

    [FormerlySerializedAs("equipmentStatModifiers")]
    [SerializeField] private List<StatModifier> statModifiers = new();
    public IReadOnlyList<StatModifier> StatModifiers => statModifiers;

    [Header("Combat")]
    [SerializeField] private List<string> weaponIds = new();
    public IReadOnlyList<string> WeaponIds => weaponIds;

    [SerializeField] private string weaponTypeId;
    public string WeaponTypeId => weaponTypeId;

    [SerializeField] private string armorProfileId;
    public string ArmorProfileId => armorProfileId;

    [SerializeField] private string defenseProfileId;
    public string DefenseProfileId => defenseProfileId;

    [SerializeField] private string armorTypeId;
    public string ArmorTypeId => armorTypeId;

    [Header("Movement")]
    [SerializeField] private string movementProfileId;
    public string MovementProfileId => movementProfileId;

    [SerializeField] private string locomotionProfileId;
    public string LocomotionProfileId => locomotionProfileId;

    [Header("Simulation")]
    [SerializeField] private string schemaModeId = "baseline";
    public string SchemaModeId => schemaModeId;

    [Header("Identity")]
    [SerializeField] private string unitCategoryId;
    public string UnitCategoryId => unitCategoryId;

    [SerializeField] private string roleId;
    public string RoleId => roleId;

    [SerializeField] private List<string> traitIds = new();
    public IReadOnlyList<string> TraitIds => traitIds;

    [Header("Skills")]
    [SerializeField] private List<string> startingSkillIds = new();
    public IReadOnlyList<string> StartingSkillIds => startingSkillIds;

    [SerializeField] private string xpCurveId;
    public string XpCurveId => xpCurveId;

    [SerializeField] private int levelCap = 10;
    public int LevelCap => levelCap;

    [Header("Needs & Mood")]
    [SerializeField] private string needsProfileId;
    public string NeedsProfileId => needsProfileId;

    [SerializeField] private string moodProfileId;
    public string MoodProfileId => moodProfileId;

    [SerializeField] private List<string> moodModifierIds = new();
    public IReadOnlyList<string> MoodModifierIds => moodModifierIds;

    [Header("Inventory & Equipment")]
    [SerializeField] private int inventorySlots = 4;
    public int InventorySlots => inventorySlots;

    [SerializeField] private int equipmentSlots = 2;
    public int EquipmentSlots => equipmentSlots;

    [SerializeField] private List<string> startingItemIds = new();
    public IReadOnlyList<string> StartingItemIds => startingItemIds;

    [Header("Work & Production")]
    [SerializeField] private string productionProfileId;
    public string ProductionProfileId => productionProfileId;

    [SerializeField] private List<string> requiredBuildingIds = new();
    public IReadOnlyList<string> RequiredBuildingIds => requiredBuildingIds;

    [SerializeField] private List<string> requiredTechIds = new();
    public IReadOnlyList<string> RequiredTechIds => requiredTechIds;

    [SerializeField] private List<string> jobProfileIds = new();
    public IReadOnlyList<string> JobProfileIds => jobProfileIds;

    [SerializeField] private List<string> jobIds = new();
    public IReadOnlyList<string> JobIds => jobIds;

    [Header("AI")]
    [SerializeField] private string aiBehaviorProfileId;
    public string AIBehaviorProfileId => aiBehaviorProfileId;

    [SerializeField] private List<string> aiGoalIds = new();
    public IReadOnlyList<string> AIGoalIds => aiGoalIds;

    [SerializeField] private string aiPriorityId;
    public string AIPriorityId => aiPriorityId;

    [SerializeField] private string perceptionProfileId;
    public string PerceptionProfileId => perceptionProfileId;

    [Header("Costs")]
    [SerializeField] private List<ResourceAmount> costs = new();
    public IReadOnlyList<ResourceAmount> Costs => costs;

    [SerializeField] private List<ResourceAmount> upkeepCosts = new();
    public IReadOnlyList<ResourceAmount> UpkeepCosts => upkeepCosts;

    [SerializeField] private int populationCost = 1;
    public int PopulationCost => populationCost;

    [Header("Faction")]
    [SerializeField] private string defaultFactionId;
    public string DefaultFactionId => defaultFactionId;

#if UNITY_EDITOR
    private void OnValidate()
    {
        DefinitionMetadataUtility.EnsureMetadata(ref metadata, DefinitionCategory.Unit);
        DefinitionIdLifecycle.ValidateOnValidate(this, ref id, ref isIdFinalized, ref finalizedId);

        if (string.IsNullOrWhiteSpace(schemaModeId))
            schemaModeId = "baseline";

        stats ??= new();
        statModifiers ??= new();
        weaponIds ??= new();
        traitIds ??= new();
        startingSkillIds ??= new();
        moodModifierIds ??= new();
        startingItemIds ??= new();
        requiredBuildingIds ??= new();
        requiredTechIds ??= new();
        jobProfileIds ??= new();
        jobIds ??= new();
        aiGoalIds ??= new();
        costs ??= new();
        upkeepCosts ??= new();

        UnitRuntimeContextResolver.Invalidate(this, UnitRuntimeInvalidationReason.ProfileChanged);

        foreach (var duplicateStatId in stats.FindDuplicateStatIds())
        {
            Debug.LogError($"[Validation] Asset '{name}' (id: '{id}') has duplicate stat '{duplicateStatId}' in its base stat container.");
        }
    }
#endif
}
