using UnityEngine;

public class FormationRegistry : DefinitionRegistry<FormationDefinition>
{
    public static FormationRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple FormationRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}