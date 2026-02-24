using UnityEngine;

public class SkillRegistry : DefinitionRegistry<SkillDefinition>
{
    public static SkillRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple SkillRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}