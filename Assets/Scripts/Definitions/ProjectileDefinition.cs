using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/Projectile")]
public class ProjectileDefinition : ScriptableObject, IIdentifiable, IDefinitionMetadataProvider
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private DefinitionMetadata metadata = DefinitionMetadata.Create(DefinitionCategory.Combat);
    public DefinitionMetadata Metadata => metadata;

    [SerializeField, HideInInspector] private bool isIdFinalized;
    [SerializeField, HideInInspector] private string finalizedId;

    [SerializeField] private float speed = 10f;
    public float Speed => speed;

    [SerializeField] private float lifetime = 5f;
    public float Lifetime => lifetime;

    [SerializeField] private float impactRadius = 0f;
    public float ImpactRadius => impactRadius;

    [SerializeField] private string weaponTypeId;
    public string WeaponTypeId => weaponTypeId;

    [SerializeField] private string visualPrefabId;
    public string VisualPrefabId => visualPrefabId;

#if UNITY_EDITOR
    private void OnValidate()
    {
        DefinitionMetadataUtility.EnsureMetadata(ref metadata, DefinitionCategory.Combat);
        DefinitionIdLifecycle.ValidateOnValidate(this, ref id, ref isIdFinalized, ref finalizedId);
    }
#endif
}