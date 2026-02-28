using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/Animation")]
public class AnimationDefinition : ScriptableObject, IIdentifiable
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField, HideInInspector] private bool isIdFinalized;
    [SerializeField, HideInInspector] private string finalizedId;

    [SerializeField] private string clipName;
    public string ClipName => clipName;

    [SerializeField] private float speed = 1f;
    public float Speed => speed;

#if UNITY_EDITOR
    private void OnValidate()
    {
        DefinitionIdLifecycle.ValidateOnValidate(this, ref id, ref isIdFinalized, ref finalizedId);
    }
#endif
}