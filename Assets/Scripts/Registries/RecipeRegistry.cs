using System.Collections.Generic;
using UnityEngine;

public class RecipeRegistry : DefinitionRegistry<RecipeDefinition>
{
    public static RecipeRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple RecipeRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }

    protected override void ValidateDefinitions(List<RecipeDefinition> defs)
    {
        if (BuildingRegistry.Instance == null || JobRegistry.Instance == null || ItemRegistry.Instance == null)
        {
            Debug.LogError("RecipeRegistry validation skipped: one or more dependent registries are null (BuildingRegistry, JobRegistry, ItemRegistry).");
            return;
        }

        DefinitionReferenceValidator.ValidateSingleReference(
            defs,
            definition => definition.name,
            definition => definition.Id,
            definition => definition.BuildingId,
            nameof(RecipeDefinition.BuildingId),
            targetId => BuildingRegistry.Instance.TryGet(targetId, out _),
            Debug.LogError);

        DefinitionReferenceValidator.ValidateSingleReference(
            defs,
            definition => definition.name,
            definition => definition.Id,
            definition => definition.JobId,
            nameof(RecipeDefinition.JobId),
            targetId => JobRegistry.Instance.TryGet(targetId, out _),
            Debug.LogError);

        DefinitionReferenceValidator.ValidateReferenceCollection(
            defs,
            definition => definition.name,
            definition => definition.Id,
            definition => definition.Inputs,
            amount => amount.itemId,
            nameof(RecipeDefinition.Inputs),
            targetId => ItemRegistry.Instance.TryGet(targetId, out _),
            Debug.LogError);

        DefinitionReferenceValidator.ValidateReferenceCollection(
            defs,
            definition => definition.name,
            definition => definition.Id,
            definition => definition.Outputs,
            amount => amount.itemId,
            nameof(RecipeDefinition.Outputs),
            targetId => ItemRegistry.Instance.TryGet(targetId, out _),
            Debug.LogError);
    }
}
