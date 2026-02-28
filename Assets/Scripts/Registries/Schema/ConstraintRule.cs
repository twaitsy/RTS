using System;
using System.Collections.Generic;

public class ConstraintRule<TDefinition>
{
    public ConstraintRule(string name, Func<TDefinition, IEnumerable<string>> validate)
    {
        Name = name;
        Validate = validate;
    }

    public string Name { get; }
    public Func<TDefinition, IEnumerable<string>> Validate { get; }
}
