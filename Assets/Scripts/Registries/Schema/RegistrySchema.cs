using System;
using System.Collections.Generic;

public interface IRegistrySchema<TDefinition>
{
    IReadOnlyList<FieldRule<TDefinition>> FieldRules { get; }
    IReadOnlyList<ReferenceRule<TDefinition>> ReferenceRules { get; }
    IReadOnlyList<ConstraintRule<TDefinition>> ConstraintRules { get; }
}

public class RegistrySchema<TDefinition> : IRegistrySchema<TDefinition>
{
    private readonly List<FieldRule<TDefinition>> fieldRules = new();
    private readonly List<ReferenceRule<TDefinition>> referenceRules = new();
    private readonly List<ConstraintRule<TDefinition>> constraintRules = new();

    public IReadOnlyList<FieldRule<TDefinition>> FieldRules => fieldRules;
    public IReadOnlyList<ReferenceRule<TDefinition>> ReferenceRules => referenceRules;
    public IReadOnlyList<ConstraintRule<TDefinition>> ConstraintRules => constraintRules;

    public RegistrySchema<TDefinition> RequireField(string fieldName, Func<TDefinition, object> selector)
    {
        fieldRules.Add(new FieldRule<TDefinition>(fieldName, selector, true));
        return this;
    }

    public RegistrySchema<TDefinition> OptionalField(string fieldName, Func<TDefinition, object> selector)
    {
        fieldRules.Add(new FieldRule<TDefinition>(fieldName, selector, false));
        return this;
    }

    public RegistrySchema<TDefinition> AddReference(
        string fieldName,
        Func<TDefinition, IEnumerable<string>> selector,
        bool isRequired,
        params ReferenceTargetRule[] allowedTargets)
    {
        referenceRules.Add(new ReferenceRule<TDefinition>(fieldName, selector, allowedTargets, isRequired));
        return this;
    }

    public RegistrySchema<TDefinition> AddConstraint(string name, Func<TDefinition, IEnumerable<string>> validate)
    {
        constraintRules.Add(new ConstraintRule<TDefinition>(name, validate));
        return this;
    }

    public static IEnumerable<string> SingleReference(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Array.Empty<string>();

        return new[] { value };
    }

    public static IEnumerable<string> ReferenceCollection<TItem>(IEnumerable<TItem> items, Func<TItem, string> idSelector)
    {
        if (items == null)
            return Array.Empty<string>();

        var ids = new List<string>();
        foreach (var item in items)
            ids.Add(idSelector(item));

        return ids;
    }
}
