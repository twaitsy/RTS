using UnityEngine;

public class SeasonRegistry : DefinitionRegistry<SeasonDefinition>
{
    public static SeasonRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple SeasonRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}