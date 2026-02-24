using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/Job")]
public class JobDefinition : ScriptableObject, IIdentifiable
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private string displayName;
    public string DisplayName => displayName;

    [SerializeField] private float baseWorkTime;
    public float BaseWorkTime => baseWorkTime;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(id))
            id = name;
    }
#endif
}