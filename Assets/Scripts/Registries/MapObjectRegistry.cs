using UnityEngine;

public class MapObjectRegistry : DefinitionRegistry<MapObjectDefinition>
{
    public static MapObjectRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple MapObjectRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}