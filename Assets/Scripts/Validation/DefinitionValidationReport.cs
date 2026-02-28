using System.Collections.Generic;
using System.Text;

public class DefinitionValidationReport
{
    private readonly Dictionary<string, List<string>> errorsByRegistry = new();

    public int ErrorCount { get; private set; }

    public bool HasErrors => ErrorCount > 0;

    public void AddError(string registryName, string message)
    {
        if (!errorsByRegistry.TryGetValue(registryName, out var errors))
        {
            errors = new List<string>();
            errorsByRegistry.Add(registryName, errors);
        }

        errors.Add(message);
        ErrorCount++;
    }

    public bool HasErrorsForRegistry(string registryName)
    {
        return errorsByRegistry.TryGetValue(registryName, out var errors) && errors.Count > 0;
    }

    public string BuildSummary()
    {
        if (!HasErrors)
            return "[Validation] Full definition validation passed with 0 issues.";

        var builder = new StringBuilder();
        builder.AppendLine($"[Validation] Full definition validation failed with {ErrorCount} issue(s) across {errorsByRegistry.Count} registries.");

        foreach (var pair in errorsByRegistry)
        {
            builder.AppendLine($"- {pair.Key}: {pair.Value.Count} issue(s)");
            foreach (var error in pair.Value)
                builder.AppendLine($"  • {error}");
        }

        return builder.ToString().TrimEnd();
    }
}
