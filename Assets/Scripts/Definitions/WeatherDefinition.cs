using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/Weather")]
public class WeatherDefinition : ScriptableObject, IIdentifiable
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private string displayName;
    public string DisplayName => displayName;

    [SerializeField] private float temperature;
    public float Temperature => temperature;

    [SerializeField] private float precipitation;
    public float Precipitation => precipitation;

    [SerializeField] private float windStrength;
    public float WindStrength => windStrength;

    [SerializeField] private List<NeedModifier> needModifiers = new();
    public IReadOnlyList<NeedModifier> NeedModifiers => needModifiers;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(id))
            id = name;
    }
#endif
}