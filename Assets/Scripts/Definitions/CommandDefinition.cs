using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/Command")]
public class CommandDefinition : ScriptableObject, IIdentifiable
{
    [SerializeField] private string id;
    public string Id => id;

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
        if (string.IsNullOrWhiteSpace(id))
            id = name;
    }
#endif
}