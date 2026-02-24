using UnityEngine;

public class RoleRegistry : DefinitionRegistry<RoleDefinition>
{
    public static RoleRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple RoleRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}