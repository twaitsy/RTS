using UnityEngine;

public class RelationshipRegistry : DefinitionRegistry<RelationshipDefinition>
{
    public static RelationshipRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple RelationshipRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}