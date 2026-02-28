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

    // ----------------------------------------------------------------------
    // Canonical Stats
    // ----------------------------------------------------------------------

    [Header("Stats")]
    [FormerlySerializedAs("baseStats")]
    [SerializeField] private SerializedStatContainer stats = new();
    public SerializedStatContainer Stats => stats;
    public IReadOnlyList<StatEntry> BaseStats => stats.Entries;

    [FormerlySerializedAs("equipmentStatModifiers")]
    [SerializeField] private List<StatModifier> statModifiers = new();
    public IReadOnlyList<StatModifier> StatModifiers => statModifiers;

    // ----------------------------------------------------------------------
    // Combat (non-numeric links only)
    // ----------------------------------------------------------------------

    [Header("Combat")]
    [SerializeField] private string weaponTypeId;
    public string WeaponTypeId => weaponTypeId;

    [SerializeField] private string armorTypeId;
    public string ArmorTypeId => armorTypeId;

    // ----------------------------------------------------------------------
    // Identity
    // ----------------------------------------------------------------------

    [Header("Identity")]
    [SerializeField] private string roleId;
    public string RoleId => roleId;

    [SerializeField] private List<string> traitIds = new();
    public IReadOnlyList<string> TraitIds => traitIds;

    // ----------------------------------------------------------------------
    // Skills (IDs only  numeric skill levels now live in stats)
    // ----------------------------------------------------------------------

    [Header("Skills")]
    [SerializeField] private List<string> startingSkillIds = new();
    public IReadOnlyList<string> StartingSkillIds => startingSkillIds;

    [SerializeField] private string xpCurveId;
    public string XpCurveId => xpCurveId;

    [SerializeField] private int levelCap = 10;
    public int LevelCap => levelCap;

    // ----------------------------------------------------------------------
    // Needs & Mood (IDs only  numeric values now live in stats)
    // ----------------------------------------------------------------------

    [Header("Needs & Mood")]
    [SerializeField] private string needsProfileId;
    public string NeedsProfileId => needsProfileId;

    [SerializeField] private List<string> moodModifierIds = new();
    public IReadOnlyList<string> MoodModifierIds => moodModifierIds;

    // ----------------------------------------------------------------------
    // Inventory & Equipment
    // ----------------------------------------------------------------------

    [Header("Inventory & Equipment")]
    [SerializeField] private int inventorySlots = 4;
    public int InventorySlots => inventorySlots;

    [SerializeField] private int equipmentSlots = 2;
    public int EquipmentSlots => equipmentSlots;

    [SerializeField] private List<string> startingItemIds = new();
    public IReadOnlyList<string> StartingItemIds => startingItemIds;

    // ----------------------------------------------------------------------
    // Work & Production (IDs only  numeric values now live in stats)
    // ----------------------------------------------------------------------

    [Header("Work & Production")]
    [SerializeField] private List<string> jobIds = new();
    public IReadOnlyList<string> JobIds => jobIds;

    // ----------------------------------------------------------------------
    // AI
    // ----------------------------------------------------------------------

    [Header("AI")]
    [SerializeField] private List<string> aiGoalIds = new();
    public IReadOnlyList<string> AIGoalIds => aiGoalIds;

    [SerializeField] private string aiPriorityId;
    public string AIPriorityId => aiPriorityId;

    [SerializeField] private string perceptionProfileId;
    public string PerceptionProfileId => perceptionProfileId;

    // ----------------------------------------------------------------------
    // Costs & Upkeep
    // ----------------------------------------------------------------------

    [Header("Costs")]
    [SerializeField] private List<ResourceAmount> costs = new();
    public IReadOnlyList<ResourceAmount> Costs => costs;

    [SerializeField] private List<ResourceAmount> upkeepCosts = new();
    public IReadOnlyList<ResourceAmount> UpkeepCosts => upkeepCosts;

    [SerializeField] private int populationCost = 1;
    public int PopulationCost => populationCost;

    // ----------------------------------------------------------------------
    // Faction
    // ----------------------------------------------------------------------

    [Header("Faction")]
    [SerializeField] private string defaultFactionId;
    public string DefaultFactionId => defaultFactionId;

    // ----------------------------------------------------------------------
    // Editor Validation
    // ----------------------------------------------------------------------

#if UNITY_EDITOR
    private void OnValidate()
    {
        DefinitionMetadataUtility.EnsureMetadata(ref metadata, DefinitionCategory.Unit);
        DefinitionIdLifecycle.ValidateOnValidate(this, ref id, ref isIdFinalized, ref finalizedId);

        stats ??= new();
        statModifiers ??= new();

        foreach (var duplicateStatId in stats.FindDuplicateStatIds())
        {
            Debug.LogError($"[Validation] Asset '{name}' (id: '{id}') has duplicate stat '{duplicateStatId}' in its base stat container.");
        }
    }
#endif
}