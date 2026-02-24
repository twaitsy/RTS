using UnityEngine;

public class TaskRegistry : DefinitionRegistry<TaskDefinition>
{
    public static TaskRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple TaskRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}