using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct NeedModifier
{
    public string needId;
    public float deltaPerSecond;
}
[Serializable]
public struct StatModifier
{
    public string targetStatId;
    public float value;
    public StatOperation operation;
}

public enum StatOperation
{
    Add,
    Multiply,
    Override
}

[Serializable]
public struct ResourceAmount
{
    [SerializeField] private string resourceId;
    public string ResourceId => resourceId;

    [SerializeField] private int amount;
    public int Amount => amount;
}

[Serializable]
public struct DamageModifier
{
    public string weaponTypeId;
    public string armorTypeId;
    public float multiplier;
}

[Serializable]
public struct NeedAmount
{
    public string needId;
    public float amount;
}

[Serializable]
public struct ItemAmount
{
    public string itemId;
    public int amount;
}

[Serializable]
public struct CivilianNeedEntry
{
    public string needId;
    public float maxValue;
    public float startValue;
    public float decayMultiplier;
}

[Serializable]
public struct ScheduleBlock
{
    public int startHour;
    public int endHour;
    public string jobId;
    public string zoneId;
    public int priority;
}

[Serializable]
public struct RoleNeedMultiplier
{
    public string needId;
    public float multiplier;
}

public enum ItemCategory
{
    Consumable,
    Tool,
    Equipment,
    Material,
    Misc
}

public enum AIGoalTargetType
{
    None,
    Resource,
    Building,
    Enemy,
    Zone,
    Position
}

public enum ZoneType
{
    None,
    Housing,
    Work,
    Storage,
    Leisure,
    Danger,
    Gathering
}
public enum CommandActionType
{
    None,
    Move,
    Attack,
    Stop,
    HoldPosition,
    UseAbility,
    Build,
    Patrol
}