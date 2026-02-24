using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/UIIcon")]
public class UIIconDefinition : ScriptableObject, IIdentifiable
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private Sprite sprite;
    public Sprite Sprite => sprite;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(id))
            id = name;
    }
#endif
}