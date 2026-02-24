using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/Civilian")]
public class CivilianDefinition : ScriptableObject, IIdentifiable
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private string displayName;
    public string DisplayName => displayName;

    [SerializeField] private List<StatEntry> baseStats = new();
    public IReadOnlyList<StatEntry> BaseStats => baseStats;

    // Legacy (deprecated - use BaseStats)
    [SerializeField] private float moveSpeed;
    [SerializeField] private float workSpeed;

    public float MoveSpeed => GetBaseStat(CanonicalStatIds.MoveSpeed, moveSpeed);
    public float WorkSpeed => GetBaseStat(CanonicalStatIds.WorkSpeed, workSpeed);

    private float GetBaseStat(string statId, float fallback)
    {
        foreach (var stat in baseStats)
        {
            if (string.Equals(stat.StatId, statId, StringComparison.Ordinal))
                return stat.Value;
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
