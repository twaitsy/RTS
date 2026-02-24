using UnityEngine;

public class TerrainLayerRegistry : DefinitionRegistry<TerrainLayerDefinition>
{
    public static TerrainLayerRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple TerrainLayerRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}