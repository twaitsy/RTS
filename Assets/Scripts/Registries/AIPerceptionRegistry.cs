using UnityEngine;

public class AIPerceptionRegistry : DefinitionRegistry<AIPerceptionDefinition>
{
    public static AIPerceptionRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple AIPerceptionRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}