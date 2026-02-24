using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/WeaponType")]
public class WeaponTypeDefinition : ScriptableObject, IIdentifiable
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private List<StatModifier> statModifiers = new();
    public IReadOnlyList<StatModifier> StatModifiers => statModifiers;

    // Legacy (deprecated - use StatModifiers)
    [SerializeField] private float baseDamage;
    [SerializeField] private float attackSpeed;

    public float BaseDamage => GetOverride(CanonicalStatIds.BaseDamage, baseDamage);
    public float AttackSpeed => GetOverride(CanonicalStatIds.AttackSpeed, attackSpeed);

    private float GetOverride(string statId, float fallback)
    {
        foreach (var modifier in statModifiers)
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
