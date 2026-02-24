using UnityEngine;

public class StatusEffectRegistry : DefinitionRegistry<StatusEffectDefinition>
{
    public static StatusEffectRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple StatusEffectRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}