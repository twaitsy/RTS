using UnityEngine;

public class PolicyRegistry : DefinitionRegistry<PolicyDefinition>
{
    public static PolicyRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple PolicyRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}