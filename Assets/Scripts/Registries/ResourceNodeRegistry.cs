using UnityEngine;

public class ResourceNodeRegistry : DefinitionRegistry<ResourceNodeDefinition>
{
    public static ResourceNodeRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple ResourceNodeRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}