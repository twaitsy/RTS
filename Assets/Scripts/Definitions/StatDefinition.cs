using UnityEngine;

public enum StatDomain
{
    Core,
    Movement,
    Combat,
    Defense,
    Resistance,
    Production,
    Economy,
    Needs,
    Mood,
    Skills,
    AI,
    Social,
    Building,
    Weather,
    Biome,
    Item,
    StatusEffects,
    Governance,
    Global,
    Utility,
    Resource,
    // Legacy domains kept for compatibility with already-authored assets.
    Locomotion,
    Environment
}

public enum StatValueType
{
    Float,
    Integer,
    Boolean
}

public enum StatStackingBehavior
{
    Additive,
    Multiplicative,
    Override,
    Highest,
    Lowest
}

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/Stat")]
public class StatDefinition : ScriptableObject, IIdentifiable
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField, HideInInspector] private bool isIdFinalized;
    [SerializeField, HideInInspector] private string finalizedId;

    [SerializeField] private string displayName;
    public string DisplayName => displayName;

    [SerializeField] private StatDomain domain = StatDomain.Utility;
    public StatDomain Domain => domain;

    [SerializeField] private StatValueType valueType = StatValueType.Float;
    public StatValueType ValueType => valueType;

    [SerializeField] private string unit;
    public string Unit => unit;

    [SerializeField] private float defaultValue;
    public float DefaultValue => defaultValue;

    [Header("Optional Metadata")]
    [SerializeField] private bool useClamp;
    public bool UseClamp => useClamp;

    [SerializeField] private float minValue;
    public float MinValue => minValue;

    [SerializeField] private float maxValue = 1f;
    public float MaxValue => maxValue;

    [SerializeField] private StatStackingBehavior stackingBehavior = StatStackingBehavior.Additive;
    public StatStackingBehavior StackingBehavior => stackingBehavior;

#if UNITY_EDITOR
    private void OnValidate()
    {
        DefinitionIdLifecycle.ValidateOnValidate(this, ref id, ref isIdFinalized, ref finalizedId);

        if (useClamp && maxValue < minValue)
            maxValue = minValue;
    }
#endif
}
