using UnityEngine;

public class NaturalResourceRegistry : DefinitionRegistry<NaturalResourceDefinition>
{
    public static NaturalResourceRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple NaturalResourceRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}