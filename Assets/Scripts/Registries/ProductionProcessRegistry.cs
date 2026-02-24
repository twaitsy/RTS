using UnityEngine;

public class ProductionProcessRegistry : DefinitionRegistry<ProductionProcessDefinition>
{
    public static ProductionProcessRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple ProductionProcessRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}