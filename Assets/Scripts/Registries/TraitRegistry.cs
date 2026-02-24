using UnityEngine;

public class TraitRegistry : DefinitionRegistry<TraitDefinition>
{
    public static TraitRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple TraitRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}