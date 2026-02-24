using UnityEngine;

public class CivilianRegistry : DefinitionRegistry<CivilianDefinition>
{
    public static CivilianRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple CivilianRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}