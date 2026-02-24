using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/Need")]
public class NeedDefinition : ScriptableObject, IIdentifiable
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private string displayName;
    public string DisplayName => displayName;

    [SerializeField] private SerializedStatContainer stats = new();
    public SerializedStatContainer Stats => stats;

    [SerializeField] private float maxValue = 100f;
    public float MaxValue => maxValue;

    [SerializeField] private float baseDecayPerSecond = 1f;
    public float BaseDecayPerSecond => baseDecayPerSecond;

    [SerializeField] private float criticalThreshold = 20f;
    public float CriticalThreshold => criticalThreshold;

    [SerializeField] private List<string> statModifierIds = new();
    public IReadOnlyList<string> StatModifierIds => statModifierIds;

    [SerializeField] private bool allowAnyModifierDomain;
    public bool AllowAnyModifierDomain => allowAnyModifierDomain;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(id))
            id = name;

        stats ??= new();

        foreach (var duplicateStatId in stats.FindDuplicateStatIds())
        {
            Debug.LogError($"[Validation] Asset '{name}' (id: '{id}') has duplicate stat '{duplicateStatId}' in its base stat container.");
        }
    }
#endif
}
