using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/Task")]
public class TaskDefinition : ScriptableObject, IIdentifiable
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField, HideInInspector] private bool isIdFinalized;
    [SerializeField, HideInInspector] private string finalizedId;

    [SerializeField] private string jobId;
    public string JobId => jobId;

    [SerializeField] private float workTime;
    public float WorkTime => workTime;

#if UNITY_EDITOR
    private void OnValidate()
    {
        DefinitionIdLifecycle.ValidateOnValidate(this, ref id, ref isIdFinalized, ref finalizedId);
    }
#endif
}