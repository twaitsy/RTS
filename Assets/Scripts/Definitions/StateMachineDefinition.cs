using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct StateDefinitionEntry
{
    public string stateId;
    public string actionId;
}

[Serializable]
public struct StateTransitionEntry
{
    public string fromStateId;
    public string toStateId;
    public string conditionDescription;
}

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/StateMachine")]
public class StateMachineDefinition : ScriptableObject, IIdentifiable, IDefinitionMetadataProvider
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private DefinitionMetadata metadata = DefinitionMetadata.Create(DefinitionCategory.Stat);
    public DefinitionMetadata Metadata => metadata;

    [SerializeField, HideInInspector] private bool isIdFinalized;
    [SerializeField, HideInInspector] private string finalizedId;

    [SerializeField] private List<StateDefinitionEntry> states = new();
    public IReadOnlyList<StateDefinitionEntry> States => states;

    [SerializeField] private List<StateTransitionEntry> transitions = new();
    public IReadOnlyList<StateTransitionEntry> Transitions => transitions;

#if UNITY_EDITOR
    private void OnValidate()
    {
        DefinitionMetadataUtility.EnsureMetadata(ref metadata, DefinitionCategory.Stat);
        DefinitionIdLifecycle.ValidateOnValidate(this, ref id, ref isIdFinalized, ref finalizedId);
    }
#endif
}