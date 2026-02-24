using UnityEngine;

public enum FormationShape
{
    Line,
    Column,
    Square,
    Wedge
}

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/Formation")]
public class FormationDefinition : ScriptableObject, IIdentifiable
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private FormationShape shape;
    public FormationShape Shape => shape;

    [SerializeField] private float spacing = 1.5f;
    public float Spacing => spacing;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(id))
            id = name;
    }
#endif
}