using UnityEngine;

public class DamageTableRegistry : DefinitionRegistry<DamageTableDefinition>
{
    public static DamageTableRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple DamageTableRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}