using System;
using UnityEngine;

// ====================== SHARED LOGIC CORE (one file for everything) ======================

public enum ComparisonOperator
{
    LessThan,
    GreaterThan,
    Equal,
    NotEqual,
    LessThanOrEqual,
    GreaterThanOrEqual
}

public interface IEffectContext { }   // expand later if needed

public interface ITagHolder
{
    bool HasTag(string tag);
}

public interface IEffectReceiver
{
    /// <summary>
    /// Called when any Trigger wants to apply an EffectDefinition to this entity
    /// </summary>
    void ReceiveEffect(EffectDefinition effect, IEffectContext context);
}