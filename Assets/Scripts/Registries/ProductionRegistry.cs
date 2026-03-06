using System.Collections.Generic;
using UnityEngine;

public class ProductionRegistry : DefinitionRegistry<ProductionDefinition>
{
    public static ProductionRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple ProductionRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }

    protected override IEnumerable<string> GetValidationDependencyErrors()
    {
        if (BuildingRegistry.Instance == null)
            yield return "Missing dependency: BuildingRegistry.Instance is null.";
        if (UnitRegistry.Instance == null)
            yield return "Missing dependency: UnitRegistry.Instance is null.";
        if (ResourceRegistry.Instance == null)
            yield return "Missing dependency: ResourceRegistry.Instance is null.";
        if (StatRegistry.Instance == null)
            yield return "Missing dependency: StatRegistry.Instance is null.";
    }
}
