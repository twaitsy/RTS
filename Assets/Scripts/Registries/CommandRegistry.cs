using UnityEngine;

public class CommandRegistry : DefinitionRegistry<CommandDefinition>
{
    public static CommandRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple CommandRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}