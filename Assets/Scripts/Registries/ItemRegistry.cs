using UnityEngine;

public class ItemRegistry : DefinitionRegistry<ItemDefinition>
{
    public static ItemRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple ItemRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}