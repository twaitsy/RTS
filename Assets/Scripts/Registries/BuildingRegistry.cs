using UnityEngine;

public class BuildingRegistry : DefinitionRegistry<BuildingDefinition>
{
    public static BuildingRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple BuildingRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}