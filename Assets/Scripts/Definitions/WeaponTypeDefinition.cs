using UnityEngine;

[CreateAssetMenu(menuName = "DataDrivenRTS/Definitions/WeaponType")]
public class WeaponTypeDefinition : ScriptableObject, IIdentifiable
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private float baseDamage;
    public float BaseDamage => baseDamage;

    [SerializeField] private float attackSpeed;
    public float AttackSpeed => attackSpeed;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(id))
            id = name;
    }
#endif
}