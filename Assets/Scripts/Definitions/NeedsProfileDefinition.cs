using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/NeedsProfile")]
public class NeedsProfileDefinition : ScriptableObject, IIdentifiable, IDefinitionMetadataProvider
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private DefinitionMetadata metadata = DefinitionMetadata.Create(DefinitionCategory.Social);
    public DefinitionMetadata Metadata => metadata;

    [SerializeField, HideInInspector] private bool isIdFinalized;
    [SerializeField, HideInInspector] private string finalizedId;

    [SerializeField] private string civilianDefinitionId;
    public string CivilianDefinitionId => civilianDefinitionId;

    [SerializeField] private List<CivilianNeedEntry> needs = new();
    public IReadOnlyList<CivilianNeedEntry> Needs => needs;

    [SerializeField] private float hungerCurve = 1f;
    public float HungerCurve => hungerCurve;

    [SerializeField] private float thirstCurve = 1f;
    public float ThirstCurve => thirstCurve;

    [SerializeField] private float fatigueCurve = 1f;
    public float FatigueCurve => fatigueCurve;

    [SerializeField] private float moraleCurve = 1f;
    public float MoraleCurve => moraleCurve;

    [SerializeField] private float stressCurve = 1f;
    public float StressCurve => stressCurve;

    [SerializeField] private float socialNeedCurve = 1f;
    public float SocialNeedCurve => socialNeedCurve;

#if UNITY_EDITOR
    private void OnValidate()
    {
        DefinitionMetadataUtility.EnsureMetadata(ref metadata, DefinitionCategory.Social);
        DefinitionIdLifecycle.ValidateOnValidate(this, ref id, ref isIdFinalized, ref finalizedId);
        needs ??= new();
    }
#endif
}
