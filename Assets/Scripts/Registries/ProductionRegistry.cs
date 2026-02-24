using UnityEngine;

public class ProductionRegistry : DefinitionRegistry<ProductionDefinition>
{
    public static ProductionRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple ProductionRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}