using System.Collections.Generic;
using UnityEngine;

public class CivilianNeedsProfileRegistry : DefinitionRegistry<CivilianNeedsProfile>
{
    private static RegistrySchema<CivilianNeedsProfile> schema;

    public static CivilianNeedsProfileRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple CivilianNeedsProfileRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }

    protected override RegistrySchema<CivilianNeedsProfile> GetSchema()
    {
        return schema ??= new RegistrySchema<CivilianNeedsProfile>()
            .RequireField(nameof(CivilianNeedsProfile.Id), definition => definition.Id)
            .RequireField(nameof(CivilianNeedsProfile.Metadata), definition => definition.Metadata)
            .OptionalField(nameof(CivilianNeedsProfile.CivilianDefinitionId), definition => definition.CivilianDefinitionId)
            .OptionalField(nameof(CivilianNeedsProfile.Needs), definition => definition.Needs)
            .AddReference(
                nameof(CivilianNeedsProfile.CivilianDefinitionId),
                definition => RegistrySchema<CivilianNeedsProfile>.SingleReference(definition.CivilianDefinitionId),
                false,
                new ReferenceTargetRule(nameof(CivilianRegistry), targetId => CivilianRegistry.Instance != null && CivilianRegistry.Instance.TryGet(targetId, out _)))
            .AddReference(
                nameof(CivilianNeedsProfile.Needs) + ".needId",
                definition => RegistrySchema<CivilianNeedsProfile>.ReferenceCollection(definition.Needs, need => need.needId),
                false,
                new ReferenceTargetRule(nameof(NeedRegistry), targetId => NeedRegistry.Instance != null && NeedRegistry.Instance.TryGet(targetId, out _)))
            .AddConstraint("CivilianNeedsProfileConstraints", ValidateConstraints);
    }

    private static IEnumerable<string> ValidateConstraints(CivilianNeedsProfile definition)
    {
        if (definition.HungerCurve <= 0f)
            yield return $"{nameof(CivilianNeedsProfile.HungerCurve)} must be greater than 0.";
        if (definition.ThirstCurve <= 0f)
            yield return $"{nameof(CivilianNeedsProfile.ThirstCurve)} must be greater than 0.";
        if (definition.FatigueCurve <= 0f)
            yield return $"{nameof(CivilianNeedsProfile.FatigueCurve)} must be greater than 0.";
        if (definition.MoraleCurve <= 0f)
            yield return $"{nameof(CivilianNeedsProfile.MoraleCurve)} must be greater than 0.";
        if (definition.StressCurve <= 0f)
            yield return $"{nameof(CivilianNeedsProfile.StressCurve)} must be greater than 0.";
        if (definition.SocialNeedCurve <= 0f)
            yield return $"{nameof(CivilianNeedsProfile.SocialNeedCurve)} must be greater than 0.";

        var seenNeedIds = new HashSet<string>();
        var needs = definition.Needs;

        if (needs == null)
            yield break;

        for (var index = 0; index < needs.Count; index++)
        {
            var entry = needs[index];
            if (string.IsNullOrWhiteSpace(entry.needId))
            {
                yield return $"{nameof(CivilianNeedsProfile.Needs)}[{index}].needId must not be empty.";
                continue;
            }

            if (!seenNeedIds.Add(entry.needId))
                yield return $"{nameof(CivilianNeedsProfile.Needs)} contains duplicate need id '{entry.needId}'.";

            if (entry.maxValue < 0f)
                yield return $"{nameof(CivilianNeedsProfile.Needs)}[{index}].{nameof(CivilianNeedEntry.maxValue)} must be greater than or equal to 0.";
            if (entry.startValue < 0f)
                yield return $"{nameof(CivilianNeedsProfile.Needs)}[{index}].{nameof(CivilianNeedEntry.startValue)} must be greater than or equal to 0.";
            if (entry.startValue > entry.maxValue)
                yield return $"{nameof(CivilianNeedsProfile.Needs)}[{index}].{nameof(CivilianNeedEntry.startValue)} must be less than or equal to {nameof(CivilianNeedEntry.maxValue)}.";
            if (entry.decayMultiplier < 0f)
                yield return $"{nameof(CivilianNeedsProfile.Needs)}[{index}].{nameof(CivilianNeedEntry.decayMultiplier)} must be greater than or equal to 0.";
        }
    }

    protected override IEnumerable<string> GetValidationDependencyErrors()
    {
        if (CivilianRegistry.Instance == null)
            yield return "Missing dependency: CivilianRegistry.Instance is null.";
        if (NeedRegistry.Instance == null)
            yield return "Missing dependency: NeedRegistry.Instance is null.";
    }
}
