using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/CivilianNeedsProfile")]
public class CivilianNeedsProfile : ScriptableObject, IIdentifiable
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private string civilianDefinitionId;
    public string CivilianDefinitionId => civilianDefinitionId;

    [SerializeField] private List<CivilianNeedEntry> needs = new();
    public IReadOnlyList<CivilianNeedEntry> Needs => needs;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(id))
            id = name;
    }
#endif
}