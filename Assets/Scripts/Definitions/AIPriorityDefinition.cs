using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/AIPriority")]
public class AIPriorityDefinition : ScriptableObject, IIdentifiable
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private string displayName;
    public string DisplayName => displayName;

    [SerializeField] private Sprite icon;
    public Sprite Icon => icon;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(id))
            id = name;
    }
#endif
}