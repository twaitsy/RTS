using UnityEngine;

public class ToolRegistry : DefinitionRegistry<ToolDefinition>
{
    public static ToolRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple ToolRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}