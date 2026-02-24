using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/Civilian")]
public class CivilianDefinition : ScriptableObject, IIdentifiable
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private string displayName;
    public string DisplayName => displayName;

    [SerializeField] private float moveSpeed;
    public float MoveSpeed => moveSpeed;

    [SerializeField] private float workSpeed;
    public float WorkSpeed => workSpeed;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(id))
            id = name;
    }
#endif
}