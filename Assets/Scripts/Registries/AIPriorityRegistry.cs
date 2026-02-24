using UnityEngine;

public class AIPriorityRegistry : DefinitionRegistry<AIPriorityDefinition>
{
    public static AIPriorityRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple AIPriorityRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}