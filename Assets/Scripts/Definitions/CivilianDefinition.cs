using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/Civilian")]
public class CivilianDefinition : ScriptableObject, IIdentifiable
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private string displayName;
    public string DisplayName => displayName;

    [FormerlySerializedAs("baseStats")]
    [SerializeField] private SerializedStatContainer stats = new();
    public IReadOnlyList<StatEntry> BaseStats => stats.Entries;
    public SerializedStatContainer Stats => stats;

    // Legacy (deprecated - use BaseStats)
    [SerializeField] private float moveSpeed;
    [SerializeField] private float workSpeed;

    public float MoveSpeed => GetBaseStat(CanonicalStatIds.MoveSpeed, moveSpeed);
    public float WorkSpeed => GetBaseStat(CanonicalStatIds.WorkSpeed, workSpeed);

    private float GetBaseStat(string statId, float fallback)
    {
        return stats.TryGetValue(statId, out var value) ? value : fallback;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(id))
            id = name;
    }
#endif
}
