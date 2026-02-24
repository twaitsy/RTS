using UnityEngine;

public class CivilianNeedsProfileRegistry : DefinitionRegistry<CivilianNeedsProfile>
{
    public static CivilianNeedsProfileRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple CivilianNeedsProfileRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }
}