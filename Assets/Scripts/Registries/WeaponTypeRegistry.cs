using UnityEngine;

public class WeaponTypeRegistry : DefinitionRegistry<WeaponTypeDefinition>
{
    public static WeaponTypeRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple WeaponTypeRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}