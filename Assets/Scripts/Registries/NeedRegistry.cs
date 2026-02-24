using UnityEngine;

public class NeedRegistry : DefinitionRegistry<NeedDefinition>
{
    public static NeedRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple NeedRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}