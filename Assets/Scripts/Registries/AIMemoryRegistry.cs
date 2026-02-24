using UnityEngine;

public class AIMemoryRegistry : DefinitionRegistry<AIMemoryDefinition>
{
    public static AIMemoryRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple AIMemoryRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}