using UnityEngine;

public class ProjectileRegistry : DefinitionRegistry<ProjectileDefinition>
{
    public static ProjectileRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple ProjectileRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}