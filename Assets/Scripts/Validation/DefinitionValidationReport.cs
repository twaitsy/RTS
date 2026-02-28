using System.Collections.Generic;
using System.Linq;
using System.Text;

public class DefinitionValidationReport
{
    private readonly Dictionary<string, List<string>> errorsByRegistry = new();
    private const int HighImpactLimit = 5;

    public DefinitionReferenceMap ReferenceMap { get; set; }

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

        AppendReferenceInsights(builder);

        return builder.ToString().TrimEnd();
    }

    private void AppendReferenceInsights(StringBuilder builder)
    {
        if (ReferenceMap == null)
            return;

        if (!ReferenceMap.TryFindOrphans(out var orphans))
            return;

        builder.AppendLine("[Validation] High-impact missing references:");

        var rankedOrphans = orphans
            .GroupBy(orphan => (orphan.TargetType, orphan.TargetId))
            .Select(group => new
            {
                group.Key.TargetType,
                group.Key.TargetId,
                Count = group.Count(),
                Sources = group.Take(3).ToList()
            })
            .OrderByDescending(item => item.Count)
            .ThenBy(item => item.TargetType)
            .ThenBy(item => item.TargetId)
            .Take(HighImpactLimit);

        foreach (var orphan in rankedOrphans)
        {
            builder.AppendLine($"- Missing '{orphan.TargetType}:{orphan.TargetId}' referenced {orphan.Count} time(s).");
            foreach (var source in orphan.Sources)
            {
                builder.AppendLine($"  • {source.SourceType}:{source.SourceId} via field '{source.Field}'");
                if (ReferenceMap.TryGetDependencyChain(source.SourceType, source.SourceId, out var chain, 2))
                    builder.AppendLine($"    Chain: {chain}");
            }
        }
    }
}
