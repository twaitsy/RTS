using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/Sound")]
public class SoundDefinition : ScriptableObject, IIdentifiable
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private AudioClip clip;
    public AudioClip Clip => clip;

    [SerializeField] private float volume = 1f;
    public float Volume => volume;

    [SerializeField] private float pitch = 1f;
    public float Pitch => pitch;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(id))
            id = name;
    }
#endif
}