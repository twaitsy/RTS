using UnityEngine;

public class MarketRegistry : DefinitionRegistry<MarketDefinition>
{
    public static MarketRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple MarketRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}