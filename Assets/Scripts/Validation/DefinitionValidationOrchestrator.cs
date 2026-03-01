using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

public static class DefinitionValidationOrchestrator
{
    public static DefinitionValidationReport RunValidation()
    {
        var report = new DefinitionValidationReport();
        var referenceMap = new DefinitionReferenceMap();
        report.ReferenceMap = referenceMap;

        ExecuteRegistryValidators(report, referenceMap);
        ExecuteEditorValidators(report);

        return report;
    }

    public static DefinitionValidationReport RunValidationAndLog()
    {
        var report = RunValidation();

        var issues = report.Issues;

        foreach (var issue in issues)
        {
            var formattedIssue = FormatIssueForLog(issue);
            switch (issue.Severity)
            {
                case ValidationIssueSeverity.Error:
                    Debug.LogError(formattedIssue);
                    break;
                case ValidationIssueSeverity.Warning:
                    Debug.LogWarning(formattedIssue);
                    break;
                default:
                    Debug.Log(formattedIssue);
                    break;
            }
        }

        Debug.Log($"[Validation] {issues.Count} issue(s) / {report.ErrorCount} error(s).");

        return report;
    }

    private static string FormatIssueForLog(ValidationIssue issue)
    {
        var builder = new StringBuilder();
        builder.Append($"[Validation] [{issue.Code}] [{issue.Registry}] {issue.Message}");

        if (!string.IsNullOrWhiteSpace(issue.AssetPath) || !string.IsNullOrWhiteSpace(issue.AssetId) || !string.IsNullOrWhiteSpace(issue.Field))
        {
            builder.Append(" | ");
            builder.Append($"assetPath={issue.AssetPath ?? "n/a"}, assetId={issue.AssetId ?? "n/a"}, field={issue.Field ?? "n/a"}");
        }

        return builder.ToString();
    }

    private static void ExecuteRegistryValidators(DefinitionValidationReport report, DefinitionReferenceMap referenceMap)
    {
        var validators = new List<IDefinitionRegistryValidator>();
        var registries = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        SeedRegistrySingletonInstances(registries);

        foreach (var registry in registries)
        {
            if (registry is IDefinitionRegistryValidator validator)
                validators.Add(validator);
        }

        validators.Sort((left, right) => string.Compare(left.RegistryName, right.RegistryName, StringComparison.Ordinal));

        foreach (var validator in validators)
        {
            validator.CollectReferenceMap(referenceMap);
            validator.ValidateAll(report);
        }

        PrefabRegistry.AppendValidationIssues(report);

    }

    private static void SeedRegistrySingletonInstances(IEnumerable<MonoBehaviour> registries)
    {
        foreach (var registry in registries)
        {
            if (registry == null)
                continue;

            var registryType = registry.GetType();
            var instanceProperty = registryType.GetProperty(
                "Instance",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

            if (instanceProperty == null || instanceProperty.PropertyType != registryType)
                continue;

            var setter = instanceProperty.GetSetMethod(true);
            if (setter == null)
                continue;

            var existing = instanceProperty.GetValue(null);
            if (existing == null)
            {
                setter.Invoke(null, new object[] { registry });
                continue;
            }

            if (!ReferenceEquals(existing, registry))
                Debug.LogWarning($"[Validation] Singleton mismatch for {registryType.Name}; keeping existing instance '{((MonoBehaviour)existing).name}'.");
        }
    }

    private static void ExecuteEditorValidators(DefinitionValidationReport report)
    {
#if UNITY_EDITOR
        var bridgeType = Type.GetType("DefinitionValidationEditorBridge, Assembly-CSharp-Editor");
        var runMethod = bridgeType?.GetMethod("Run", BindingFlags.Public | BindingFlags.Static);
        runMethod?.Invoke(null, new object[] { report });
#endif
    }
}
