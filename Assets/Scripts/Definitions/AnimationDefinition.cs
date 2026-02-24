using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/Animation")]
public class AnimationDefinition : ScriptableObject, IIdentifiable
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private string clipName;
    public string ClipName => clipName;

    [SerializeField] private float speed = 1f;
    public float Speed => speed;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(id))
            id = name;
    }
#endif
}