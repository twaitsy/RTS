using UnityEngine;

public class ScheduleRegistry : DefinitionRegistry<ScheduleDefinition>
{
    public static ScheduleRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple ScheduleRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}