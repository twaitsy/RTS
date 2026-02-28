using System;
using System.Collections.Generic;
using UnityEngine;

public static class LogicMath
{
    public static bool Compare(float left, float right, ComparisonOperator op)
    {
        switch (op)
        {
            case ComparisonOperator.LessThan: return left < right;
            case ComparisonOperator.GreaterThan: return left > right;
            case ComparisonOperator.Equal: return Mathf.Approximately(left, right);
            case ComparisonOperator.NotEqual: return !Mathf.Approximately(left, right);
            case ComparisonOperator.LessThanOrEqual: return left <= right || Mathf.Approximately(left, right);
            case ComparisonOperator.GreaterThanOrEqual: return left >= right || Mathf.Approximately(left, right);
            default: return false;
        }
    }
}

public interface IStandardConditionData
{
    bool GetFlag(string key);
    float GetNumber(string key);
    string GetText(string key);
    bool HasTag(string tag);
}

public interface IStandardRequirementData
{
    bool HasId(string key);
    float GetValue(string key);
}

public static class StandardConditionFunctions
{
    public static bool Evaluate(ConditionNode node, IStandardConditionData data)
    {
        switch (node.leafType)
        {
            case ConditionLeafType.AlwaysTrue:
                return true;
            case ConditionLeafType.AlwaysFalse:
                return false;
            case ConditionLeafType.HasTag:
                return data.HasTag(node.stringValue);
            case ConditionLeafType.HealthBelow:
                return data.GetNumber("health") < node.floatValue;
            case ConditionLeafType.HealthAbove:
                return data.GetNumber("health") > node.floatValue;
            case ConditionLeafType.IsDead:
                return data.GetFlag("isDead");
            case ConditionLeafType.CooldownReady:
                return data.GetFlag("cooldownReady");
            case ConditionLeafType.IsNight:
                return data.GetFlag("isNight");
            case ConditionLeafType.IsSeason:
                return string.Equals(data.GetText("season"), node.stringValue, StringComparison.OrdinalIgnoreCase);
            case ConditionLeafType.TimeSinceLastEvent:
                return data.GetNumber("timeSinceLastEvent") >= node.floatValue;
            default:
                Debug.LogWarning($"No standard evaluator implemented for Condition leaf '{node.leafType}'.");
                return false;
        }
    }
}

public static class StandardRequirementFunctions
{
    public static bool Evaluate(RequirementNode node, IStandardRequirementData data)
    {
        switch (node.leafType)
        {
            case RequirementLeafType.AlwaysTrue:
                return true;
            case RequirementLeafType.AlwaysFalse:
                return false;
            case RequirementLeafType.HasTech:
            case RequirementLeafType.HasTrait:
            case RequirementLeafType.HasPolicy:
                return data.HasId(node.leafData.targetId);
            case RequirementLeafType.HasResourceAmount:
            case RequirementLeafType.PlayerHasScoreAbove:
            case RequirementLeafType.PopulationAbove:
            case RequirementLeafType.PopulationBelow:
            {
                var current = data.GetValue(node.leafData.targetId);
                return LogicMath.Compare(current, node.leafData.value, node.leafData.comparison);
            }
            default:
                Debug.LogWarning($"No standard evaluator implemented for Requirement leaf '{node.leafType}'.");
                return false;
        }
    }
}
