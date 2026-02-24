using UnityEngine;

public class SquadRegistry : DefinitionRegistry<SquadDefinition>
{
    public static SquadRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple SquadRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}