using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/Building")]
public class BuildingDefinition : ScriptableObject, IIdentifiable
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private string displayName;
    public string DisplayName => displayName;

    [SerializeField] private Sprite icon;
    public Sprite Icon => icon;

    [SerializeField] private List<ResourceAmount> buildCosts = new();
    public IReadOnlyList<ResourceAmount> BuildCosts => buildCosts;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(id))
            id = name;
    }
#endif
}