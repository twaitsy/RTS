using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/Faction")]
public class FactionDefinition : ScriptableObject, IIdentifiable
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private string displayName;
    public string DisplayName => displayName;

    [SerializeField] private List<string> startingUnitIds = new();
    public IReadOnlyList<string> StartingUnitIds => startingUnitIds;

    [SerializeField] private List<string> startingBuildingIds = new();
    public IReadOnlyList<string> StartingBuildingIds => startingBuildingIds;

    [SerializeField] private string techTreeId;
    public string TechTreeId => techTreeId;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(id))
            id = name;
    }
#endif
}