using UnityEngine;

public class WeatherEffectRegistry : DefinitionRegistry<WeatherEffectDefinition>
{
    public static WeatherEffectRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple WeatherEffectRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}