using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/MovementProfile")]
public class MovementProfileDefinition : ScriptableObject, IIdentifiable, IDefinitionMetadataProvider
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private DefinitionMetadata metadata = DefinitionMetadata.Create(DefinitionCategory.World);
    public DefinitionMetadata Metadata => metadata;

    [SerializeField, HideInInspector] private bool isIdFinalized;
    [SerializeField, HideInInspector] private string finalizedId;

    [SerializeField] private string displayName;
    public string DisplayName => displayName;

    [SerializeField] private SerializedStatContainer stats = new();
    public SerializedStatContainer Stats => stats;

    [SerializeField] private List<StatModifier> statModifiers = new();
    public IReadOnlyList<StatModifier> StatModifiers => statModifiers;

    [SerializeField] private string locomotionProfileId;
    public string LocomotionProfileId => locomotionProfileId;

    [SerializeField] private float moveSpeedMultiplier = 1f;
    public float MoveSpeedMultiplier => moveSpeedMultiplier;

    [SerializeField] private float defenseMultiplier = 1f;
    public float DefenseMultiplier => defenseMultiplier;

    [SerializeField] private float acceleration = 4f;
    public float Acceleration => acceleration;

    [SerializeField] private float turnRate = 180f;
    public float TurnRate => turnRate;

    [SerializeField] private float stoppingDistance = 0.05f;
    public float StoppingDistance => stoppingDistance;

    [SerializeField] private bool canStrafe = true;
    public bool CanStrafe => canStrafe;

    [SerializeField] private bool canReverse;
    public bool CanReverse => canReverse;

#if UNITY_EDITOR
    private void OnValidate()
    {
        DefinitionMetadataUtility.EnsureMetadata(ref metadata, DefinitionCategory.World);
        DefinitionIdLifecycle.ValidateOnValidate(this, ref id, ref isIdFinalized, ref finalizedId);

        stats ??= new();
        statModifiers ??= new();
        moveSpeedMultiplier = Mathf.Max(0f, moveSpeedMultiplier);
        defenseMultiplier = Mathf.Max(0f, defenseMultiplier);
        acceleration = Mathf.Max(0f, acceleration);
        turnRate = Mathf.Max(0f, turnRate);
        stoppingDistance = Mathf.Max(0f, stoppingDistance);
        UnitRuntimeContextResolver.ClearCache();

        foreach (var duplicateStatId in stats.FindDuplicateStatIds())
            Debug.LogError($"[Validation] Asset '{name}' (id: '{id}') has duplicate stat '{duplicateStatId}' in its base stat container.");
    }
#endif
}
