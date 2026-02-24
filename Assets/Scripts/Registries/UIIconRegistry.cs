using UnityEngine;

public class UIIconRegistry : DefinitionRegistry<UIIconDefinition>
{
    public static UIIconRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple UIIconRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}