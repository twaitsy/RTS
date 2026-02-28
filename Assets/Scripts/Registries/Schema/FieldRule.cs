using System;
using System.Collections;

public class FieldRule<TDefinition>
{
    public FieldRule(string fieldName, Func<TDefinition, object> valueSelector, bool isRequired = true)
    {
        FieldName = fieldName;
        ValueSelector = valueSelector;
        IsRequired = isRequired;
    }

    public string FieldName { get; }
    public Func<TDefinition, object> ValueSelector { get; }
    public bool IsRequired { get; }

    public bool HasValue(TDefinition definition)
    {
        if (ValueSelector == null)
            return true;

        var value = ValueSelector(definition);
        if (value == null)
            return false;

        if (value is string text)
            return !string.IsNullOrWhiteSpace(text);

        if (value is ICollection collection)
            return collection.Count > 0;

        return true;
    }
}
