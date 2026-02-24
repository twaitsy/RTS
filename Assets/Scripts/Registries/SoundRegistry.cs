using UnityEngine;

public class SoundRegistry : DefinitionRegistry<SoundDefinition>
{
    public static SoundRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple SoundRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}