using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/Command")]
public class CommandDefinition : ScriptableObject, IIdentifiable, IDefinitionMetadataProvider
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private DefinitionMetadata metadata = DefinitionMetadata.Create(DefinitionCategory.AI);
    public DefinitionMetadata Metadata => metadata;

    [SerializeField, HideInInspector] private bool isIdFinalized;
    [SerializeField, HideInInspector] private string finalizedId;

    [SerializeField] private string displayName;
    public string DisplayName => displayName;

    [SerializeField] private string iconId;
    public string IconId => iconId;

    [SerializeField] private string hotkey;
    public string Hotkey => hotkey;

    [SerializeField] private CommandActionType actionType;
    public CommandActionType ActionType => actionType;

    [SerializeField] private string actionData;
    public string ActionData => actionData;

#if UNITY_EDITOR
    private void OnValidate()
    {
        DefinitionMetadataUtility.EnsureMetadata(ref metadata, DefinitionCategory.AI);
        DefinitionIdLifecycle.ValidateOnValidate(this, ref id, ref isIdFinalized, ref finalizedId);
    }
#endif
}