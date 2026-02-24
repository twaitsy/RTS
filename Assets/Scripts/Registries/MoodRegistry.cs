using UnityEngine;

public class MoodRegistry : DefinitionRegistry<MoodDefinition>
{
    public static MoodRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple MoodRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}