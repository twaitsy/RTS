using UnityEngine;

public class DiseaseRegistry : DefinitionRegistry<DiseaseDefinition>
{
    public static DiseaseRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple DiseaseRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}