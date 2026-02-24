using UnityEngine;

public class BehaviourRegistry : DefinitionRegistry<BehaviourDefinition>
{
    public static BehaviourRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple BehaviourRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}