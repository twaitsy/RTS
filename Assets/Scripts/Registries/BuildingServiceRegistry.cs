using UnityEngine;

public class BuildingServiceRegistry : DefinitionRegistry<BuildingServiceDefinition>
{
    public static BuildingServiceRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple BuildingServiceRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}