using System.Collections.Generic;
using System.Linq;
using System.Text;

public enum ValidationIssueSeverity
{
    Info,
    Warning,
    Error
}

public sealed class ValidationIssue
{
    public ValidationIssue(
        string code,
        ValidationIssueSeverity severity,
        string registry,
        string message,
        string assetPath = null,
        string assetId = null,
        string field = null,
        string suggestedFix = null)
    {
        Code = code;
        Severity = severity;
        Registry = registry;
        Message = message;
        AssetPath = assetPath;
        AssetId = assetId;
        Field = field;
        SuggestedFix = suggestedFix;
    }

    public string Code { get; }
    public ValidationIssueSeverity Severity { get; }
    public string Registry { get; }
    public string Message { get; }
    public string AssetPath { get; }
    public string AssetId { get; }
    public string Field { get; }
    public string SuggestedFix { get; }
}

public class DefinitionValidationReport
{
    private readonly Dictionary<string, List<ValidationIssue>> issuesByRegistry = new();
    private const int HighImpactLimit = 5;

    public DefinitionReferenceMap ReferenceMap { get; set; }

    public int ErrorCount { get; private set; }

    public bool HasErrors => ErrorCount > 0;

    public IReadOnlyList<ValidationIssue> Issues => issuesByRegistry.Values.SelectMany(x => x).ToList();

    public void AddError(string registryName, string message)
    {
        AddIssue(new ValidationIssue(
            code: "VALIDATION_ERROR",
            severity: ValidationIssueSeverity.Error,
            registry: registryName,
            message: message));
    }

    public void AddIssue(ValidationIssue issue)
    {
        var registryName = string.IsNullOrWhiteSpace(issue.Registry) ? "UnknownRegistry" : issue.Registry;

        if (!issuesByRegistry.TryGetValue(registryName, out var issues))
        {
            issues = new List<ValidationIssue>();
            issuesByRegistry.Add(registryName, issues);
        }

        issues.Add(issue);

        if (issue.Severity == ValidationIssueSeverity.Error)
            ErrorCount++;
    }

    public bool HasErrorsForRegistry(string registryName)
    {
        return issuesByRegistry.TryGetValue(registryName, out var issues) && issues.Any(issue => issue.Severity == ValidationIssueSeverity.Error);
    }

    public string BuildSummary()
    {
        if (!Issues.Any())
            return "[Validation] Full definition validation passed with 0 issues.";

        var builder = new StringBuilder();
        builder.AppendLine($"[Validation] Full definition validation found {Issues.Count} issue(s) ({ErrorCount} error(s)) across {issuesByRegistry.Count} registries.");

        foreach (var pair in issuesByRegistry.OrderBy(entry => entry.Key))
        {
            builder.AppendLine($"- {pair.Key}: {pair.Value.Count} issue(s)");
            foreach (var issue in pair.Value)
            {
                builder.AppendLine($"  • [{issue.Severity}] ({issue.Code}) {issue.Message}");

                if (!string.IsNullOrWhiteSpace(issue.AssetPath) || !string.IsNullOrWhiteSpace(issue.AssetId) || !string.IsNullOrWhiteSpace(issue.Field))
                {
                    builder.AppendLine($"    asset: {issue.AssetPath ?? "n/a"}, id: {issue.AssetId ?? "n/a"}, field: {issue.Field ?? "n/a"}");
                }

                if (!string.IsNullOrWhiteSpace(issue.SuggestedFix))
                    builder.AppendLine($"    suggested fix: {issue.SuggestedFix}");
            }
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
