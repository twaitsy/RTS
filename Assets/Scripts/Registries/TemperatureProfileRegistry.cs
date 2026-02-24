using UnityEngine;

public class TemperatureProfileRegistry : DefinitionRegistry<TemperatureProfileDefinition>
{
    public static TemperatureProfileRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple TemperatureProfileRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}