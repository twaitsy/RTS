using UnityEngine;

public class AnimationRegistry : DefinitionRegistry<AnimationDefinition>
{
    public static AnimationRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple AnimationRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}