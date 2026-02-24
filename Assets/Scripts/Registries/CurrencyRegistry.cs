using UnityEngine;

public class CurrencyRegistry : DefinitionRegistry<CurrencyDefinition>
{
    public static CurrencyRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple CurrencyRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}