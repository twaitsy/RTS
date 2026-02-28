using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/CivilianNeedsProfile")]
public class CivilianNeedsProfile : ScriptableObject, IIdentifiable
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField, HideInInspector] private bool isIdFinalized;
    [SerializeField, HideInInspector] private string finalizedId;

    [SerializeField] private string civilianDefinitionId;
    public string CivilianDefinitionId => civilianDefinitionId;

    [SerializeField] private List<CivilianNeedEntry> needs = new();
    public IReadOnlyList<CivilianNeedEntry> Needs => needs;

#if UNITY_EDITOR
    private void OnValidate()
    {
        DefinitionIdLifecycle.ValidateOnValidate(this, ref id, ref isIdFinalized, ref finalizedId);
    }
#endif
}