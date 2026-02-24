using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/Unit")]
public class UnitDefinition : ScriptableObject, IIdentifiable
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private string displayName;
    public string DisplayName => displayName;

    [SerializeField] private Sprite icon;
    public Sprite Icon => icon;

    // Canonical stats
    [Header("Canonical Stats")]
    [SerializeField] private List<StatEntry> baseStats = new();
    public IReadOnlyList<StatEntry> BaseStats => baseStats;

    [SerializeField] private List<StatModifier> equipmentStatModifiers = new();
    public IReadOnlyList<StatModifier> EquipmentStatModifiers => equipmentStatModifiers;

    // Legacy core stats (deprecated - use BaseStats)
    [Header("Legacy Core Stats (Deprecated)")]
    [SerializeField] private float maxHealth = 100f;

    [SerializeField] private float moveSpeed = 3f;

    [SerializeField] private float turnSpeed = 360f;

    [SerializeField] private float visionRange = 10f;

    public float MaxHealth => GetBaseStat(CanonicalStatIds.MaxHealth, maxHealth);
    public float MoveSpeed => GetBaseStat(CanonicalStatIds.MoveSpeed, moveSpeed);
    public float TurnSpeed => GetBaseStat(CanonicalStatIds.TurnSpeed, turnSpeed);
    public float VisionRange => GetBaseStat(CanonicalStatIds.VisionRange, visionRange);

    // Combat
    [Header("Combat")]
    [SerializeField] private string weaponTypeId;
    public string WeaponTypeId => weaponTypeId;

    [SerializeField] private string armorTypeId;
    public string ArmorTypeId => armorTypeId;

    [SerializeField] private float baseDamage;
    [SerializeField] private float attackSpeed;
    [SerializeField] private float attackRange;

    public float BaseDamage => GetEquipmentStat(CanonicalStatIds.BaseDamage, baseDamage);
    public float AttackSpeed => GetEquipmentStat(CanonicalStatIds.AttackSpeed, attackSpeed);
    public float AttackRange => GetEquipmentStat(CanonicalStatIds.AttackRange, attackRange);

    [SerializeField] private List<DamageResistance> resistances = new();
    public IReadOnlyList<DamageResistance> Resistances => resistances;

    // Roles & traits
    [Header("Identity")]
    [SerializeField] private string roleId;
    public string RoleId => roleId;

    [SerializeField] private List<string> traitIds = new();
    public IReadOnlyList<string> TraitIds => traitIds;

    // Skills
    [Header("Skills")]
    [SerializeField] private List<SkillLevel> startingSkills = new();
    public IReadOnlyList<SkillLevel> StartingSkills => startingSkills;

    [SerializeField] private string xpCurveId;
    public string XpCurveId => xpCurveId;

    [SerializeField] private int levelCap = 10;
    public int LevelCap => levelCap;

    // Needs & mood
    [Header("Needs & Mood")]
    [SerializeField] private string needsProfileId;
    public string NeedsProfileId => needsProfileId;

    [SerializeField] private List<MoodModifier> moodModifiers = new();
    public IReadOnlyList<MoodModifier> MoodModifiers => moodModifiers;

    // Inventory & equipment
    [Header("Inventory & Equipment")]
    [SerializeField] private int inventorySlots = 4;
    public int InventorySlots => inventorySlots;

    [SerializeField] private int equipmentSlots = 2;
    public int EquipmentSlots => equipmentSlots;

    [SerializeField] private List<string> startingItemIds = new();
    public IReadOnlyList<string> StartingItemIds => startingItemIds;

    // Work & production
    [Header("Work & Production")]
    [SerializeField] private List<string> jobIds = new();
    public IReadOnlyList<string> JobIds => jobIds;

    [SerializeField] private float workSpeed = 1f;

    [SerializeField] private float carryCapacity = 10f;

    public float WorkSpeed => GetBaseStat(CanonicalStatIds.WorkSpeed, workSpeed);
    public float CarryCapacity => GetBaseStat(CanonicalStatIds.CarryCapacity, carryCapacity);

    // AI
    [Header("AI")]
    [SerializeField] private List<string> aiGoalIds = new();
    public IReadOnlyList<string> AIGoalIds => aiGoalIds;

    [SerializeField] private string aiPriorityId;
    public string AIPriorityId => aiPriorityId;

    [SerializeField] private string perceptionProfileId;
    public string PerceptionProfileId => perceptionProfileId;

    // Costs & upkeep
    [Header("Costs")]
    [SerializeField] private List<ResourceAmount> costs = new();
    public IReadOnlyList<ResourceAmount> Costs => costs;

    [SerializeField] private List<ResourceAmount> upkeepCosts = new();
    public IReadOnlyList<ResourceAmount> UpkeepCosts => upkeepCosts;

    [SerializeField] private int populationCost = 1;
    public int PopulationCost => populationCost;

    // Faction
    [Header("Faction")]
    [SerializeField] private string defaultFactionId;
    public string DefaultFactionId => defaultFactionId;

    private float GetBaseStat(string statId, float fallback)
    {
        foreach (var stat in baseStats)
        {
            if (string.Equals(stat.StatId, statId, StringComparison.Ordinal))
                return stat.Value;
        }

        return fallback;
    }

    private float GetEquipmentStat(string statId, float fallback)
    {
        foreach (var modifier in equipmentStatModifiers)
        {
            if (string.Equals(modifier.targetStatId, statId, StringComparison.Ordinal) && modifier.operation == StatOperation.Override)
                return modifier.value;
        }

        return fallback;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(id))
            id = name;
    }
#endif
}
