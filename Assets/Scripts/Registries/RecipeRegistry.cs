using UnityEngine;

public class RecipeRegistry : DefinitionRegistry<RecipeDefinition>
{
    public static RecipeRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple RecipeRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}