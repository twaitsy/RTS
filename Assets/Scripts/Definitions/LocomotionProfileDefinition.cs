using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/LocomotionProfile")]
public class LocomotionProfileDefinition : ScriptableObject, IIdentifiable, IDefinitionMetadataProvider
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private DefinitionMetadata metadata = DefinitionMetadata.Create(DefinitionCategory.Unit);
    public DefinitionMetadata Metadata => metadata;

    [SerializeField, HideInInspector] private bool isIdFinalized;
    [SerializeField, HideInInspector] private string finalizedId;

    [SerializeField] private string displayName;
    public string DisplayName => displayName;

    [SerializeField] private SerializedStatContainer stats = new();
    public SerializedStatContainer Stats => stats;

    [SerializeField] private List<StatModifier> statModifiers = new();
    public IReadOnlyList<StatModifier> StatModifiers => statModifiers;

    [SerializeField] private string clipName;
    public string ClipName => clipName;

    [SerializeField] private float speed = 1f;
    public float Speed => speed;

    [SerializeField] private bool useRootMotion;
    public bool UseRootMotion => useRootMotion;

    [SerializeField] private bool canTraverseGround = true;
    public bool CanTraverseGround => canTraverseGround;

    [SerializeField] private bool canTraverseWater;
    public bool CanTraverseWater => canTraverseWater;

    [SerializeField] private bool canTraverseAir;
    public bool CanTraverseAir => canTraverseAir;

#if UNITY_EDITOR
    private void OnValidate()
    {
        DefinitionMetadataUtility.EnsureMetadata(ref metadata, DefinitionCategory.Unit);
        DefinitionIdLifecycle.ValidateOnValidate(this, ref id, ref isIdFinalized, ref finalizedId);
        stats ??= new();
        statModifiers ??= new();
        speed = Mathf.Max(0f, speed);
        UnitRuntimeContextResolver.ClearCache();

        foreach (var duplicateStatId in stats.FindDuplicateStatIds())
            Debug.LogError($"[Validation] Asset '{name}' (id: '{id}') has duplicate stat '{duplicateStatId}' in its base stat container.");
    }
#endif
}
