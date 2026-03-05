using System.Collections.Generic;
using UnityEngine;

public class WeaponRegistry : DefinitionRegistry<WeaponDefinition>
{
    private static RegistrySchema<WeaponDefinition> schema;

    public static WeaponRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple WeaponRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }

    protected override RegistrySchema<WeaponDefinition> GetSchema()
    {
        return schema ??= new RegistrySchema<WeaponDefinition>()
            .RequireField(nameof(WeaponDefinition.Id), definition => definition.Id)
            .RequireField(nameof(WeaponDefinition.Metadata), definition => definition.Metadata)
            .RequireField(nameof(WeaponDefinition.DisplayName), definition => definition.DisplayName)
            .RequireField(nameof(WeaponDefinition.Stats), definition => definition.Stats)
            .OptionalField(nameof(WeaponDefinition.StatModifiers), definition => definition.StatModifiers)
            .OptionalField(nameof(WeaponDefinition.ProjectileId), definition => definition.ProjectileId)
            .AddReference(
                $"{nameof(WeaponDefinition.Stats)}.{nameof(SerializedStatContainer.Entries)}",
                definition => RegistrySchema<WeaponDefinition>.ReferenceCollection(definition.Stats.Entries, stat => stat.StatId),
                false,
                new ReferenceTargetRule(nameof(StatRegistry), targetId => StatRegistry.Instance != null && StatRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(
                nameof(WeaponDefinition.StatModifiers),
                definition => RegistrySchema<WeaponDefinition>.ReferenceCollection(definition.StatModifiers, modifier => modifier.targetStatId),
                false,
                new ReferenceTargetRule(nameof(StatRegistry), targetId => StatRegistry.Instance != null && StatRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(
                nameof(WeaponDefinition.ProjectileId),
                definition => RegistrySchema<WeaponDefinition>.SingleReference(definition.ProjectileId),
                false,
                new ReferenceTargetRule(nameof(ProjectileRegistry), targetId => ProjectileRegistry.Instance != null && ProjectileRegistry.Instance.TryGet(targetId, out _)))
            .AddConstraint("WeaponConstraints", ValidateConstraints);
    }

    protected override void ValidateDefinitions(List<WeaponDefinition> defs, System.Action<string> reportError)
    {
        // Schema handles validation.
    }

    private static IEnumerable<string> ValidateConstraints(WeaponDefinition definition)
    {
        if (definition.Damage < 0f)
            yield return $"{nameof(WeaponDefinition.Damage)} must be greater than or equal to 0.";
        if (definition.AttackSpeed < 0f)
            yield return $"{nameof(WeaponDefinition.AttackSpeed)} must be greater than or equal to 0.";
        if (definition.Range < 0f)
            yield return $"{nameof(WeaponDefinition.Range)} must be greater than or equal to 0.";
    }

    protected override IEnumerable<string> GetValidationDependencyErrors()
    {
        if (StatRegistry.Instance == null)
            yield return "Missing dependency: StatRegistry.Instance is null.";
        if (ProjectileRegistry.Instance == null)
            yield return "Missing dependency: ProjectileRegistry.Instance is null.";
    }
}
