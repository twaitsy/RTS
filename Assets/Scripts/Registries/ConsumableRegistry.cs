using UnityEngine;

public class ConsumableRegistry : DefinitionRegistry<ConsumableDefinition>
{
    public static ConsumableRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple ConsumableRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}