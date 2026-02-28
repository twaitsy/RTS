using System.Collections.Generic;

public class StandardConditionContext : IConditionContext, IStandardConditionData
{
    private readonly HashSet<string> tags = new();
    private readonly Dictionary<string, bool> flags = new();
    private readonly Dictionary<string, float> numbers = new();
    private readonly Dictionary<string, string> texts = new();

    public StandardConditionContext WithTag(string tag) { tags.Add(tag); return this; }
    public StandardConditionContext WithFlag(string key, bool value) { flags[key] = value; return this; }
    public StandardConditionContext WithNumber(string key, float value) { numbers[key] = value; return this; }
    public StandardConditionContext WithText(string key, string value) { texts[key] = value; return this; }

    public bool EvaluateLeaf(ConditionNode node) => StandardConditionFunctions.Evaluate(node, this);
    public bool GetFlag(string key) => flags.TryGetValue(key, out var value) && value;
    public float GetNumber(string key) => numbers.TryGetValue(key, out var value) ? value : 0f;
    public string GetText(string key) => texts.TryGetValue(key, out var value) ? value : string.Empty;
    public bool HasTag(string tag) => tags.Contains(tag);
}

public class StandardRequirementContext : IRequirementContext, IStandardRequirementData
{
    private readonly HashSet<string> ids = new();
    private readonly Dictionary<string, float> values = new();

    public StandardRequirementContext WithId(string id) { ids.Add(id); return this; }
    public StandardRequirementContext WithValue(string key, float value) { values[key] = value; return this; }

    public bool EvaluateRequirementLeaf(RequirementNode node) => StandardRequirementFunctions.Evaluate(node, this);
    public bool HasId(string key) => ids.Contains(key);
    public float GetValue(string key) => values.TryGetValue(key, out var value) ? value : 0f;
}
