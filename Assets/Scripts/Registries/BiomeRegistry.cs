using UnityEngine;

public class BiomeRegistry : DefinitionRegistry<BiomeDefinition>
{
    public static BiomeRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple BiomeRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}