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

    protected override void ValidateDefinitions(List<UnitDefinition> defs, System.Action<string> reportError)
    {
        DefinitionReferenceValidator.ValidateReferenceCollection(
            defs,
            definition => definition.name,
            definition => definition.Id,
            definition => definition.Stats.Entries,
            stat => stat.StatId,
            $"{nameof(UnitDefinition.Stats)}.{nameof(SerializedStatContainer.Entries)}",
            targetId => StatRegistry.Instance.TryGet(targetId, out _),
            reportError);

        foreach (var definition in defs)
        {
            if (definition == null)
                continue;

            foreach (var modifier in definition.StatModifiers)
            {
                if (string.IsNullOrWhiteSpace(modifier.targetStatId))
                    continue;

                if (!StatRegistry.Instance.TryGet(modifier.targetStatId, out _))
                    reportError($"[Validation] Asset '{definition.name}' (id: '{definition.Id}') field '{nameof(UnitDefinition.StatModifiers)}' references missing target id '{modifier.targetStatId}'.");
            }
        }

        DefinitionReferenceValidator.ValidateSingleReference(defs, definition => definition.name, definition => definition.Id, definition => definition.WeaponTypeId, nameof(UnitDefinition.WeaponTypeId), targetId => WeaponTypeRegistry.Instance.TryGet(targetId, out _), reportError);
        DefinitionReferenceValidator.ValidateSingleReference(defs, definition => definition.name, definition => definition.Id, definition => definition.ArmorTypeId, nameof(UnitDefinition.ArmorTypeId), targetId => ArmorTypeRegistry.Instance.TryGet(targetId, out _), reportError);
        DefinitionReferenceValidator.ValidateSingleReference(defs, definition => definition.name, definition => definition.Id, definition => definition.RoleId, nameof(UnitDefinition.RoleId), targetId => RoleRegistry.Instance.TryGet(targetId, out _), reportError);
        DefinitionReferenceValidator.ValidateReferenceList(defs, definition => definition.name, definition => definition.Id, definition => definition.TraitIds, nameof(UnitDefinition.TraitIds), targetId => TraitRegistry.Instance.TryGet(targetId, out _), reportError);
        DefinitionReferenceValidator.ValidateReferenceList(defs, definition => definition.name, definition => definition.Id, definition => definition.StartingSkillIds, nameof(UnitDefinition.StartingSkillIds), targetId => SkillRegistry.Instance.TryGet(targetId, out _), reportError);
        DefinitionReferenceValidator.ValidateSingleReference(defs, definition => definition.name, definition => definition.Id, definition => definition.NeedsProfileId, nameof(UnitDefinition.NeedsProfileId), targetId => CivilianNeedsProfileRegistry.Instance.TryGet(targetId, out _), reportError);
        DefinitionReferenceValidator.ValidateReferenceList(defs, definition => definition.name, definition => definition.Id, definition => definition.MoodModifierIds, nameof(UnitDefinition.MoodModifierIds), targetId => MoodRegistry.Instance.TryGet(targetId, out _), reportError);
        DefinitionReferenceValidator.ValidateReferenceList(defs, definition => definition.name, definition => definition.Id, definition => definition.StartingItemIds, nameof(UnitDefinition.StartingItemIds), targetId => ItemRegistry.Instance.TryGet(targetId, out _), reportError);
        DefinitionReferenceValidator.ValidateReferenceList(defs, definition => definition.name, definition => definition.Id, definition => definition.JobIds, nameof(UnitDefinition.JobIds), targetId => JobRegistry.Instance.TryGet(targetId, out _), reportError);
        DefinitionReferenceValidator.ValidateReferenceList(defs, definition => definition.name, definition => definition.Id, definition => definition.AIGoalIds, nameof(UnitDefinition.AIGoalIds), targetId => AIGoalRegistry.Instance.TryGet(targetId, out _), reportError);
        DefinitionReferenceValidator.ValidateSingleReference(defs, definition => definition.name, definition => definition.Id, definition => definition.AIPriorityId, nameof(UnitDefinition.AIPriorityId), targetId => AIPriorityRegistry.Instance.TryGet(targetId, out _), reportError);
        DefinitionReferenceValidator.ValidateSingleReference(defs, definition => definition.name, definition => definition.Id, definition => definition.PerceptionProfileId, nameof(UnitDefinition.PerceptionProfileId), targetId => AIPerceptionRegistry.Instance.TryGet(targetId, out _), reportError);
        DefinitionReferenceValidator.ValidateSingleReference(defs, definition => definition.name, definition => definition.Id, definition => definition.DefaultFactionId, nameof(UnitDefinition.DefaultFactionId), targetId => FactionRegistry.Instance.TryGet(targetId, out _), reportError);

        DefinitionReferenceValidator.ValidateReferenceCollection(defs, definition => definition.name, definition => definition.Id, definition => definition.Costs, amount => amount.ResourceId, nameof(UnitDefinition.Costs), targetId => ResourceRegistry.Instance.TryGet(targetId, out _), reportError);
        DefinitionReferenceValidator.ValidateReferenceCollection(defs, definition => definition.name, definition => definition.Id, definition => definition.UpkeepCosts, amount => amount.ResourceId, nameof(UnitDefinition.UpkeepCosts), targetId => ResourceRegistry.Instance.TryGet(targetId, out _), reportError);
    }

    protected override IEnumerable<string> GetValidationDependencyErrors()
    {
        if (StatRegistry.Instance == null) yield return "Missing dependency: StatRegistry.Instance is null.";
        if (WeaponTypeRegistry.Instance == null) yield return "Missing dependency: WeaponTypeRegistry.Instance is null.";
        if (ArmorTypeRegistry.Instance == null) yield return "Missing dependency: ArmorTypeRegistry.Instance is null.";
        if (RoleRegistry.Instance == null) yield return "Missing dependency: RoleRegistry.Instance is null.";
        if (TraitRegistry.Instance == null) yield return "Missing dependency: TraitRegistry.Instance is null.";
        if (SkillRegistry.Instance == null) yield return "Missing dependency: SkillRegistry.Instance is null.";
        if (CivilianNeedsProfileRegistry.Instance == null) yield return "Missing dependency: CivilianNeedsProfileRegistry.Instance is null.";
        if (MoodRegistry.Instance == null) yield return "Missing dependency: MoodRegistry.Instance is null.";
        if (ItemRegistry.Instance == null) yield return "Missing dependency: ItemRegistry.Instance is null.";
        if (JobRegistry.Instance == null) yield return "Missing dependency: JobRegistry.Instance is null.";
        if (AIGoalRegistry.Instance == null) yield return "Missing dependency: AIGoalRegistry.Instance is null.";
        if (AIPriorityRegistry.Instance == null) yield return "Missing dependency: AIPriorityRegistry.Instance is null.";
        if (AIPerceptionRegistry.Instance == null) yield return "Missing dependency: AIPerceptionRegistry.Instance is null.";
        if (FactionRegistry.Instance == null) yield return "Missing dependency: FactionRegistry.Instance is null.";
        if (ResourceRegistry.Instance == null) yield return "Missing dependency: ResourceRegistry.Instance is null.";
    }
}
