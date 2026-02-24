using UnityEngine;

public class BuildingCategoryRegistry : DefinitionRegistry<BuildingCategoryDefinition>
{
    public static BuildingCategoryRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple BuildingCategoryRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}