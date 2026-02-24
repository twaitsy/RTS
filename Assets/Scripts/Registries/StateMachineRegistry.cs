using UnityEngine;

public class StateMachineRegistry : DefinitionRegistry<StateMachineDefinition>
{
    public static StateMachineRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple StateMachineRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}