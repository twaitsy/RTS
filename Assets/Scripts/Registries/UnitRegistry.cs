using System.Collections.Generic;
using UnityEngine;

public class UnitRegistry : DefinitionRegistry<UnitDefinition>
{
    public static UnitRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple UnitRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }

    protected override void ValidateDefinitions(List<UnitDefinition> defs)
    {
        if (StatRegistry.Instance == null || WeaponTypeRegistry.Instance == null || ArmorTypeRegistry.Instance == null ||
            RoleRegistry.Instance == null || TraitRegistry.Instance == null || SkillRegistry.Instance == null ||
            CivilianNeedsProfileRegistry.Instance == null || MoodRegistry.Instance == null || ItemRegistry.Instance == null ||
            JobRegistry.Instance == null || AIGoalRegistry.Instance == null || AIPriorityRegistry.Instance == null ||
            AIPerceptionRegistry.Instance == null || FactionRegistry.Instance == null || ResourceRegistry.Instance == null)
        {
            Debug.LogError("UnitRegistry validation skipped: one or more dependent registries are null.");
            return;
        }

        DefinitionReferenceValidator.ValidateReferenceCollection(
            defs,
            definition => definition.name,
            definition => definition.Id,
            definition => definition.Stats.Entries,
            stat => stat.StatId,
            $"{nameof(UnitDefinition.Stats)}.{nameof(SerializedStatContainer.Entries)}",
            targetId => StatRegistry.Instance.TryGet(targetId, out _),
            Debug.LogError);

        foreach (var definition in defs)
        {
            if (definition == null)
                continue;

            foreach (var modifier in definition.StatModifiers)
            {
                if (string.IsNullOrWhiteSpace(modifier.targetStatId))
                    continue;

                if (!StatRegistry.Instance.TryGet(modifier.targetStatId, out _))
                {
                    Debug.LogError($"[Validation] Asset '{definition.name}' (id: '{definition.Id}') field '{nameof(UnitDefinition.StatModifiers)}' references missing target id '{modifier.targetStatId}'.");
                }
            }
        }

        DefinitionReferenceValidator.ValidateSingleReference(defs, definition => definition.name, definition => definition.Id, definition => definition.WeaponTypeId, nameof(UnitDefinition.WeaponTypeId), targetId => WeaponTypeRegistry.Instance.TryGet(targetId, out _), Debug.LogError);
        DefinitionReferenceValidator.ValidateSingleReference(defs, definition => definition.name, definition => definition.Id, definition => definition.ArmorTypeId, nameof(UnitDefinition.ArmorTypeId), targetId => ArmorTypeRegistry.Instance.TryGet(targetId, out _), Debug.LogError);
        DefinitionReferenceValidator.ValidateSingleReference(defs, definition => definition.name, definition => definition.Id, definition => definition.RoleId, nameof(UnitDefinition.RoleId), targetId => RoleRegistry.Instance.TryGet(targetId, out _), Debug.LogError);
        DefinitionReferenceValidator.ValidateReferenceList(defs, definition => definition.name, definition => definition.Id, definition => definition.TraitIds, nameof(UnitDefinition.TraitIds), targetId => TraitRegistry.Instance.TryGet(targetId, out _), Debug.LogError);
        DefinitionReferenceValidator.ValidateReferenceList(defs, definition => definition.name, definition => definition.Id, definition => definition.StartingSkillIds, nameof(UnitDefinition.StartingSkillIds), targetId => SkillRegistry.Instance.TryGet(targetId, out _), Debug.LogError);
        DefinitionReferenceValidator.ValidateSingleReference(defs, definition => definition.name, definition => definition.Id, definition => definition.NeedsProfileId, nameof(UnitDefinition.NeedsProfileId), targetId => CivilianNeedsProfileRegistry.Instance.TryGet(targetId, out _), Debug.LogError);
        DefinitionReferenceValidator.ValidateReferenceList(defs, definition => definition.name, definition => definition.Id, definition => definition.MoodModifierIds, nameof(UnitDefinition.MoodModifierIds), targetId => MoodRegistry.Instance.TryGet(targetId, out _), Debug.LogError);
        DefinitionReferenceValidator.ValidateReferenceList(defs, definition => definition.name, definition => definition.Id, definition => definition.StartingItemIds, nameof(UnitDefinition.StartingItemIds), targetId => ItemRegistry.Instance.TryGet(targetId, out _), Debug.LogError);
        DefinitionReferenceValidator.ValidateReferenceList(defs, definition => definition.name, definition => definition.Id, definition => definition.JobIds, nameof(UnitDefinition.JobIds), targetId => JobRegistry.Instance.TryGet(targetId, out _), Debug.LogError);
        DefinitionReferenceValidator.ValidateReferenceList(defs, definition => definition.name, definition => definition.Id, definition => definition.AIGoalIds, nameof(UnitDefinition.AIGoalIds), targetId => AIGoalRegistry.Instance.TryGet(targetId, out _), Debug.LogError);
        DefinitionReferenceValidator.ValidateSingleReference(defs, definition => definition.name, definition => definition.Id, definition => definition.AIPriorityId, nameof(UnitDefinition.AIPriorityId), targetId => AIPriorityRegistry.Instance.TryGet(targetId, out _), Debug.LogError);
        DefinitionReferenceValidator.ValidateSingleReference(defs, definition => definition.name, definition => definition.Id, definition => definition.PerceptionProfileId, nameof(UnitDefinition.PerceptionProfileId), targetId => AIPerceptionRegistry.Instance.TryGet(targetId, out _), Debug.LogError);
        DefinitionReferenceValidator.ValidateSingleReference(defs, definition => definition.name, definition => definition.Id, definition => definition.DefaultFactionId, nameof(UnitDefinition.DefaultFactionId), targetId => FactionRegistry.Instance.TryGet(targetId, out _), Debug.LogError);

        DefinitionReferenceValidator.ValidateReferenceCollection(defs, definition => definition.name, definition => definition.Id, definition => definition.Costs, amount => amount.ResourceId, nameof(UnitDefinition.Costs), targetId => ResourceRegistry.Instance.TryGet(targetId, out _), Debug.LogError);
        DefinitionReferenceValidator.ValidateReferenceCollection(defs, definition => definition.name, definition => definition.Id, definition => definition.UpkeepCosts, amount => amount.ResourceId, nameof(UnitDefinition.UpkeepCosts), targetId => ResourceRegistry.Instance.TryGet(targetId, out _), Debug.LogError);
    }
}
