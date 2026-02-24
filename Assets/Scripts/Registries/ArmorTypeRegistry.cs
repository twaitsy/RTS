using UnityEngine;

public class ArmorTypeRegistry : DefinitionRegistry<ArmorTypeDefinition>
{
    public static ArmorTypeRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple ArmorTypeRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}