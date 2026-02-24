using UnityEngine;

public class AIGoalRegistry : DefinitionRegistry<AIGoalDefinition>
{
    public static AIGoalRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple AIGoalRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}