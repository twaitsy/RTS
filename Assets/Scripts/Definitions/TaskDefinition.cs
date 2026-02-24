using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/Task")]
public class TaskDefinition : ScriptableObject, IIdentifiable
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private string jobId;
    public string JobId => jobId;

    [SerializeField] private float workTime;
    public float WorkTime => workTime;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(id))
            id = name;
    }
#endif
}