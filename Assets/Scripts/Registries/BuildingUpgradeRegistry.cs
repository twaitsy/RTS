using UnityEngine;

public class BuildingUpgradeRegistry : DefinitionRegistry<BuildingUpgradeDefinition>
{
    public static BuildingUpgradeRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple BuildingUpgradeRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}