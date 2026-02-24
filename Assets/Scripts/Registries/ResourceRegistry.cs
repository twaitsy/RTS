using UnityEngine;

public class ResourceRegistry : DefinitionRegistry<ResourceDefinition>
{
    public static ResourceRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple ResourceRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}