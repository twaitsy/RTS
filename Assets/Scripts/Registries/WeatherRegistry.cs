using UnityEngine;

public class WeatherRegistry : DefinitionRegistry<WeatherDefinition>
{
    public static WeatherRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple WeatherRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}