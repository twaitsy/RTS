using UnityEngine;

public class FactionRegistry : DefinitionRegistry<FactionDefinition>
{
    public static FactionRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple FactionRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}