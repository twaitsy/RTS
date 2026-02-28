using System;
using System.Collections.Generic;
using UnityEngine;

public enum ConditionOperator
{
    Leaf,
    And,
    Or,
    Not
}

public enum ConditionLeafType
{
    // Health / Status
    HealthBelow,
    HealthAbove,
    IsDead,
    IsInjured,
    IsUnderAttack,
    HasStatusEffect,

    // Tags / Traits / Roles / Skills
    HasTag,
    HasTrait,
    HasRole,
    HasSkill,
    CompareSkillLevel,

    // Resources & Economy
    HasResource,
    ResourceInStorage,
    ResourceRateAbove,
    ResourceRateBelow,
    ResourceCapacityAvailable,

    // Environment
    IsNight,
    IsSeason,
    IsWeatherActive,
    TemperatureAbove,
    TemperatureBelow,
    IsInBiome,

    // Spatial / Perception
    DistanceToEntityLessThan,
    HasLineOfSightTo,
    EnemyInRange,
    AllyInRange,
    IsInZone,
    IsNearZone,
    IsInsideBuilding,

    // Events / Timers
    TimeSinceLastEvent,
    TimeOfDayBetween,
    CooldownReady,

    // Squads / Groups
    HasUnitInSquad,
    SquadSizeAbove,
    SquadSizeBelow,
    SquadHasRole,
    SquadHasTrait,

    // Meta
    AlwaysTrue,
    AlwaysFalse
}

[Serializable]
public class ConditionNode
{
    public ConditionOperator op = ConditionOperator.Leaf;

    [Header("Composite")]
    [SerializeReference]
    public List<ConditionNode> children = new();

    [Header("Leaf")]
    public ConditionLeafType leafType;
    public float floatValue;
    public string stringValue;

    [Header("Reuse")]
    public ConditionDefinition referencedCondition;
}

[CreateAssetMenu(menuName = "DataDrivenRTS/Logic/Condition")]
public class ConditionDefinition : ScriptableObject, IIdentifiable
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
    public ConditionNode root = new ConditionNode();

#if UNITY_EDITOR
    private void OnValidate()
    {
        DefinitionIdLifecycle.ValidateOnValidate(this, ref id, ref isIdFinalized, ref finalizedId);
    }
#endif

    public bool Evaluate(IConditionContext context)
    {
        return Evaluate(context, new HashSet<ConditionDefinition>(), 0);
    }

    private bool Evaluate(IConditionContext context, HashSet<ConditionDefinition> visited, int depth)
    {
        if (depth > 32)
        {
            Debug.LogError($"Condition '{Id}' exceeded max depth.");
            return false;
        }

        if (!visited.Add(this))
        {
            Debug.LogError($"Circular Condition reference detected at '{Id}'.");
            return false;
        }

        bool result = EvaluateNode(root, context, visited, depth + 1);
        visited.Remove(this);
        return result;
    }

    private bool EvaluateNode(ConditionNode node, IConditionContext context, HashSet<ConditionDefinition> visited, int depth)
    {
        switch (node.op)
        {
            case ConditionOperator.Leaf:
                if (node.referencedCondition != null)
                    return node.referencedCondition.Evaluate(context, visited, depth);

                return context.EvaluateLeaf(node);

            case ConditionOperator.And:
                foreach (var child in node.children)
                    if (!EvaluateNode(child, context, visited, depth))
                        return false;
                return true;

            case ConditionOperator.Or:
                foreach (var child in node.children)
                    if (EvaluateNode(child, context, visited, depth))
                        return true;
                return false;

            case ConditionOperator.Not:
                if (node.children.Count == 0)
                    return true;
                return !EvaluateNode(node.children[0], context, visited, depth);

            default:
                return false;
        }
    }
}

public interface IConditionContext
{
    bool EvaluateLeaf(ConditionNode node);
}