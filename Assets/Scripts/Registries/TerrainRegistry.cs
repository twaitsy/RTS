using UnityEngine;

public class TerrainRegistry : DefinitionRegistry<TerrainTypeDefinition>
{
    public static TerrainRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple TerrainRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}