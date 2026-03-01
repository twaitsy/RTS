using UnityEngine;

[ExecuteAlways]
public class StatRegistry : DefinitionRegistry<StatDefinition>
{
    public static StatRegistry Instance { get; private set; }

    protected override void Awake()
    {
        // Ensure this runs in edit mode too
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple StatRegistry instances detected.");
            DestroyImmediate(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}