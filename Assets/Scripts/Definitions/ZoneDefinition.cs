using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/Zone")]
public class ZoneDefinition : ScriptableObject, IIdentifiable
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private string displayName;
    public string DisplayName => displayName;

    [SerializeField] private ZoneType zoneType;
    public ZoneType ZoneType => zoneType;

    [SerializeField] private List<NeedModifier> needModifiers = new();
    public IReadOnlyList<NeedModifier> NeedModifiers => needModifiers;

    [SerializeField] private bool isSafe;
    public bool IsSafe => isSafe;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(id))
            id = name;
    }
#endif
}