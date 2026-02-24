using UnityEngine;

public class TerrainMaterialRegistry : DefinitionRegistry<TerrainMaterialDefinition>
{
    public static TerrainMaterialRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple TerrainMaterialRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}