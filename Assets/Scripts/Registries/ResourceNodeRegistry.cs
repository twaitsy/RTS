using System.Collections.Generic;
using UnityEngine;

public class ResourceNodeRegistry : DefinitionRegistry<ResourceNodeDefinition>
{
    public static ResourceNodeRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple ResourceNodeRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }

    protected override void ValidateDefinitions(List<ResourceNodeDefinition> defs, System.Action<string> reportError)
    {
        DefinitionReferenceValidator.ValidateSingleReference(
            defs,
            definition => definition.name,
            definition => definition.Id,
            definition => definition.ResourceTypeId,
            nameof(ResourceNodeDefinition.ResourceTypeId),
            targetId => ResourceRegistry.Instance.TryGet(targetId, out _),
            reportError);

        for (int i = 0; i < defs.Count; i++)
        {
            var definition = defs[i];
            if (definition == null)
                continue;

            if (definition.Amount <= 0)
                reportError($"[Validation] Asset '{definition.name}' (id: '{definition.Id}') must have Amount > 0.");

            if (definition.GatherDifficulty <= 0f)
                reportError($"[Validation] Asset '{definition.name}' (id: '{definition.Id}') must have GatherDifficulty > 0.");

            if (definition.InteractionRadius <= 0f)
                reportError($"[Validation] Asset '{definition.name}' (id: '{definition.Id}') must have InteractionRadius > 0.");
        }
    }


    protected override void CollectCustomReferences(List<ResourceNodeDefinition> defs, DefinitionReferenceMap map)
    {
        foreach (var definition in defs)
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.Id) || string.IsNullOrWhiteSpace(definition.ResourceTypeId))
                continue;

            map.AddReference(RegistryName, definition.Id, nameof(ResourceNodeDefinition.ResourceTypeId), nameof(ResourceRegistry), definition.ResourceTypeId);
        }
    }

    protected override IEnumerable<string> GetValidationDependencyErrors()
    {
        if (ResourceRegistry.Instance == null)
            yield return "Missing dependency: ResourceRegistry.Instance is null.";
    }
}
