using System;
using System.Collections.Generic;
using UnityEngine;

public enum RequirementOperator
{
    Leaf,
    And,
    Or,
    // Not is rarely needed for requirements, so I left it out. Add it if you want.
}

public enum RequirementLeafType
{
    // Tech / Research
    HasTech,
    ResearchCompleted,
    ResearchInProgress,
    ResearchLevelAtLeast,

    // Buildings
    HasBuilding,
    BuildingCountAtLeast,
    BuildingCountAtMost,
    BuildingUpgradedTo,

    // Units
    HasUnitOfType,
    UnitCountAtLeast,
    UnitCountAtMost,
    HasUnitWithTrait,
    HasUnitWithRole,

    // Traits / Policies / Tags
    HasTrait,
    HasPolicy,
    HasTagOnFaction,

    // Resources & Economy
    HasResourceAmount,
    ResourceRateAbove,
    ResourceRateBelow,
    StorageCapacityAvailable,
    StorageCapacityUsedAbove,

    // Factions / Diplomacy
    FactionRelationAbove,
    FactionRelationBelow,
    HasAlliance,
    HasWar,

    // Eras / Seasons / Time
    ReachedEra,
    CurrentSeasonIs,
    TimeSinceStartGreaterThan,
    TimeSinceStartLessThan,
    TimeOfDayIs,

    // Score / Population
    PlayerHasScoreAbove,
    PopulationAbove,
    PopulationBelow,

    // Zones / Territory
    ControlsZone,
    ControlsTerritory,
    ZoneLevelAtLeast,

    // Environment
    WeatherIs,
    TemperatureAbove,
    TemperatureBelow,

    // Meta
    AlwaysTrue,
    AlwaysFalse
}

[Serializable]
public class RequirementLeafData
{
    [Tooltip("Tech ID, Building ID, Faction ID, Resource ID, etc.")]
    public string targetId = "";

    [Tooltip("Amount, relation level (0-100), time in seconds, etc.")]
    public float value = 0f;

    public ComparisonOperator comparison = ComparisonOperator.GreaterThanOrEqual;
}

[Serializable]
public class RequirementNode
{
    public RequirementOperator op = RequirementOperator.Leaf;

    [Header("Composite - And / Or")]
    [SerializeReference]
    public List<RequirementNode> children = new();

    [Header("Leaf")]
    public RequirementLeafType leafType;
    public RequirementLeafData leafData = new RequirementLeafData();

    [Header("Reuse another RequirementDefinition")]
    public RequirementDefinition referencedRequirement;
}

[CreateAssetMenu(menuName = "DataDrivenRTS/Logic/Requirement")]
public class RequirementDefinition : ScriptableObject, IIdentifiable
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField, HideInInspector] private bool isIdFinalized;
    [SerializeField, HideInInspector] private string finalizedId;

    [SerializeField] private string displayName;
    public string DisplayName => displayName;

    [SerializeField] private Sprite icon;
    public Sprite Icon => icon;

    [SerializeReference]
    public RequirementNode root = new RequirementNode();

#if UNITY_EDITOR
    private void OnValidate()
    {
        DefinitionIdLifecycle.ValidateOnValidate(this, ref id, ref isIdFinalized, ref finalizedId);
    }
#endif

    public bool IsSatisfied(IRequirementContext context)
    {
        if (context == null)
        {
            Debug.LogWarning($"Requirement '{Id}' evaluated with null context.");
            return false;
        }

        if (root == null)
        {
            Debug.LogWarning($"Requirement '{Id}' has no root node.");
            return false;
        }
        return EvaluateInternal(context, new HashSet<RequirementDefinition>(), 0);
    }

    private bool EvaluateInternal(IRequirementContext context, HashSet<RequirementDefinition> visited, int depth)
    {
        if (depth > 32) // consistent with Condition
        {
            Debug.LogError($"Requirement '{Id}' exceeded max recursion depth.");
            return false;
        }

        if (!visited.Add(this))
        {
            Debug.LogError($"Circular Requirement reference detected in '{Id}'.");
            return false;
        }

        bool result = EvaluateNode(root, context, visited, depth + 1);
        visited.Remove(this);
        return result;
    }

    private bool EvaluateNode(RequirementNode node, IRequirementContext context, HashSet<RequirementDefinition> visited, int depth)
    {
        switch (node.op)
        {
            case RequirementOperator.Leaf:
                if (node.referencedRequirement != null)
                    return node.referencedRequirement.EvaluateInternal(context, visited, depth);

                return context.EvaluateRequirementLeaf(node);

            case RequirementOperator.And:
                foreach (var child in node.children)
                {
                    if (child == null) continue;
                    if (!EvaluateNode(child, context, visited, depth))
                        return false;
                }
                return true;

            case RequirementOperator.Or:
                foreach (var child in node.children)
                {
                    if (child == null) continue;
                    if (EvaluateNode(child, context, visited, depth))
                        return true;
                }
                return false;

            default:
                return false;
        }
    }
}

public interface IRequirementContext
{
    bool EvaluateRequirementLeaf(RequirementNode node);
}