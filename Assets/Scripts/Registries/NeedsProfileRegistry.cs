using System.Collections.Generic;
using UnityEngine;

public class NeedsProfileRegistry : DefinitionRegistry<NeedsProfileDefinition>
{
    private static RegistrySchema<NeedsProfileDefinition> schema;

    public static NeedsProfileRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple NeedsProfileRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }

    protected override RegistrySchema<NeedsProfileDefinition> GetSchema()
    {
        return schema ??= new RegistrySchema<NeedsProfileDefinition>()
            .RequireField(nameof(NeedsProfileDefinition.Id), definition => definition.Id)
            .OptionalField(nameof(NeedsProfileDefinition.CivilianDefinitionId), definition => definition.CivilianDefinitionId)
            .OptionalField(nameof(NeedsProfileDefinition.Needs), definition => definition.Needs)
            .AddReference(
                nameof(NeedsProfileDefinition.CivilianDefinitionId),
                definition => RegistrySchema<NeedsProfileDefinition>.SingleReference(definition.CivilianDefinitionId),
                false,
                new ReferenceTargetRule(nameof(CivilianRegistry), targetId => CivilianRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(
                nameof(NeedsProfileDefinition.Needs) + ".needId",
                definition => RegistrySchema<NeedsProfileDefinition>.ReferenceCollection(definition.Needs, need => need.needId),
                false,
                new ReferenceTargetRule(nameof(NeedRegistry), targetId => NeedRegistry.Instance.TryGet(targetId, out _)));
    }

    protected override void ValidateDefinitions(List<NeedsProfileDefinition> defs, System.Action<string> reportError)
    {
    }

    protected override IEnumerable<string> GetValidationDependencyErrors()
    {
        if (CivilianRegistry.Instance == null)
            yield return "Missing dependency: CivilianRegistry.Instance is null.";
        if (NeedRegistry.Instance == null)
            yield return "Missing dependency: NeedRegistry.Instance is null.";
    }
}
