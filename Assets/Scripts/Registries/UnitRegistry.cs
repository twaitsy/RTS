using UnityEngine;

public class UnitRegistry : DefinitionRegistry<UnitDefinition>
{
    public static UnitRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple UnitRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}