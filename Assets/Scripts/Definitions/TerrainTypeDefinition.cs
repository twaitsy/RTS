using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/TerrainType")]
public class TerrainTypeDefinition : ScriptableObject, IIdentifiable
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private float moveSpeedMultiplier = 1f;
    public float MoveSpeedMultiplier => moveSpeedMultiplier;

    [SerializeField] private float defenseMultiplier = 1f;
    public float DefenseMultiplier => defenseMultiplier;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(id))
            id = name;
    }
#endif
}