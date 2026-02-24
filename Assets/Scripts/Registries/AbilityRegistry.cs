using UnityEngine;

public class AbilityRegistry : DefinitionRegistry<AbilityDefinition>
{
    public static AbilityRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple AbilityRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}