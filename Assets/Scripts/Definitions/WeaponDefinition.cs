using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/Weapon")]
public class WeaponDefinition : ScriptableObject, IIdentifiable, IDefinitionMetadataProvider
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private DefinitionMetadata metadata = DefinitionMetadata.Create(DefinitionCategory.Combat);
    public DefinitionMetadata Metadata => metadata;

    [SerializeField, HideInInspector] private bool isIdFinalized;
    [SerializeField, HideInInspector] private string finalizedId;

    [SerializeField] private string displayName;
    public string DisplayName => displayName;

    [SerializeField] private Sprite icon;
    public Sprite Icon => icon;

    [Header("Weapon Profile")]
    [SerializeField] private float damage = 10f;
    public float Damage => damage;

    [SerializeField] private float attackSpeed = 1f;
    public float AttackSpeed => attackSpeed;

    [SerializeField] private float range = 1f;
    public float Range => range;

    [SerializeField] private string projectileId;
    public string ProjectileId => projectileId;

    [FormerlySerializedAs("baseStats")]
    [SerializeField] private SerializedStatContainer stats = new();
    public SerializedStatContainer Stats => stats;
    public IReadOnlyList<StatEntry> BaseStats => stats.Entries;

    [FormerlySerializedAs("equipmentStatModifiers")]
    [SerializeField] private List<StatModifier> statModifiers = new();
    public IReadOnlyList<StatModifier> StatModifiers => statModifiers;

#if UNITY_EDITOR
    private void OnValidate()
    {
        DefinitionMetadataUtility.EnsureMetadata(ref metadata, DefinitionCategory.Combat);
        DefinitionIdLifecycle.ValidateOnValidate(this, ref id, ref isIdFinalized, ref finalizedId);

        stats ??= new();
        statModifiers ??= new();
        damage = Mathf.Max(0f, damage);
        attackSpeed = Mathf.Max(0f, attackSpeed);
        range = Mathf.Max(0f, range);
        UnitRuntimeContextResolver.ClearCache();

        foreach (var duplicateStatId in stats.FindDuplicateStatIds())
        {
            Debug.LogError($"[Validation] Asset '{name}' (id: '{id}') has duplicate stat '{duplicateStatId}' in its base stat container.");
        }
    }
#endif
}
