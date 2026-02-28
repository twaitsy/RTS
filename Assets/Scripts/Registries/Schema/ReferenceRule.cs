using System;
using System.Collections.Generic;

public class ReferenceRule<TDefinition>
{
    public ReferenceRule(
        string fieldName,
        Func<TDefinition, IEnumerable<string>> referenceIdsSelector,
        IEnumerable<ReferenceTargetRule> allowedTargets,
        bool isRequired = false,
        bool requireSingleTargetType = true)
    {
        FieldName = fieldName;
        ReferenceIdsSelector = referenceIdsSelector;
        AllowedTargets = allowedTargets != null ? new List<ReferenceTargetRule>(allowedTargets) : new List<ReferenceTargetRule>();
        IsRequired = isRequired;
        RequireSingleTargetType = requireSingleTargetType;
    }

    public string FieldName { get; }
    public Func<TDefinition, IEnumerable<string>> ReferenceIdsSelector { get; }
    public IReadOnlyList<ReferenceTargetRule> AllowedTargets { get; }
    public bool IsRequired { get; }
    public bool RequireSingleTargetType { get; }

    public IEnumerable<string> GetReferenceIds(TDefinition definition)
    {
        return ReferenceIdsSelector?.Invoke(definition);
    }
}

public class ReferenceTargetRule
{
    public ReferenceTargetRule(string targetName, Func<string, bool> targetExists)
    {
        TargetName = targetName;
        TargetExists = targetExists;
    }

    public string TargetName { get; }
    public Func<string, bool> TargetExists { get; }
}
