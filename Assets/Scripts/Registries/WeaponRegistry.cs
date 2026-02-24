using UnityEngine;

public class WeaponRegistry : DefinitionRegistry<WeaponDefinition>
{
    public static WeaponRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple WeaponRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}