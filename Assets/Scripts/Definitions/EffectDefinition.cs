using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/Effect")]
public class EffectDefinition : ScriptableObject, IIdentifiable, IDefinitionMetadataProvider
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private DefinitionMetadata metadata = DefinitionMetadata.Create(DefinitionCategory.Logic);
    public DefinitionMetadata Metadata => metadata;

    [SerializeField, HideInInspector] private bool isIdFinalized;
    [SerializeField, HideInInspector] private string finalizedId;

    [SerializeField] private float duration;
    public float Duration => duration;

    [SerializeField] private float tickInterval;
    public float TickInterval => tickInterval;

    [SerializeField] private List<string> statModifierIds = new();
    public IReadOnlyList<string> StatModifierIds => statModifierIds;

    [SerializeField] private string visualEffectId;
    public string VisualEffectId => visualEffectId;

#if UNITY_EDITOR
    private void OnValidate()
    {
        DefinitionMetadataUtility.EnsureMetadata(ref metadata, DefinitionCategory.Logic);
        DefinitionIdLifecycle.ValidateOnValidate(this, ref id, ref isIdFinalized, ref finalizedId);
    }
#endif

    /// <summary>
    /// Called automatically by TriggerDefinition.TryFire()
    /// You can override this in child classes if you ever need special instant behaviour
    /// </summary>
    public virtual void Apply(IEffectContext context, object target)
    {
        if (target is IEffectReceiver receiver)
        {
            receiver.ReceiveEffect(this, context);
        }
        else
        {
            Debug.LogWarning($"Target {target} does not implement IEffectReceiver. Effect '{Id}' was not applied.");
        }
    }
}