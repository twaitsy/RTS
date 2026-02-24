using UnityEngine;

public class EffectRegistry : DefinitionRegistry<EffectDefinition>
{
    public static EffectRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple EffectRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}