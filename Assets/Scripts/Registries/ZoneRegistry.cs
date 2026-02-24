using UnityEngine;

public class ZoneRegistry : DefinitionRegistry<ZoneDefinition>
{
    public static ZoneRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple ZoneRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}