using UnityEngine;

public class StorageRuleRegistry : DefinitionRegistry<StorageRuleDefinition>
{
    public static StorageRuleRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple StorageRuleRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}