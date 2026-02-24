using UnityEngine;

public class StorageTypeRegistry : DefinitionRegistry<StorageTypeDefinition>
{
    public static StorageTypeRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple StorageTypeRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}