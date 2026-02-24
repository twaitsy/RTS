using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/TerrainType")]
public class TerrainTypeDefinition : ScriptableObject, IIdentifiable
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private SerializedStatContainer stats = new();
    public SerializedStatContainer Stats => stats;

    [SerializeField] private float moveSpeedMultiplier = 1f;
    public float MoveSpeedMultiplier => moveSpeedMultiplier;

    [SerializeField] private float defenseMultiplier = 1f;
    public float DefenseMultiplier => defenseMultiplier;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(id))
            id = name;

        stats ??= new();

        foreach (var duplicateStatId in stats.FindDuplicateStatIds())
        {
            Debug.LogError($"[Validation] Asset '{name}' (id: '{id}') has duplicate stat '{duplicateStatId}' in its base stat container.");
        }
    }
#endif
}
