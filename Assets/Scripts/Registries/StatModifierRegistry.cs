using UnityEngine;

public class StatModifierRegistry : DefinitionRegistry<StatModifierDefinition>
{
    public static StatModifierRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple StatModifierRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}