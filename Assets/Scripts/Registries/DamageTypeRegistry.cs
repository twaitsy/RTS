using UnityEngine;

public class DamageTypeRegistry : DefinitionRegistry<DamageTypeDefinition>
{
    public static DamageTypeRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple DamageTypeRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}