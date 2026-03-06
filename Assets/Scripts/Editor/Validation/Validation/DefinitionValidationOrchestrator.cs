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

    public static DefinitionValidationReport RunValidationAndLog(bool quiet = false)
    {
        var report = RunValidation();

        if (!quiet)
            LogIssues(report);

        return report;
    }

    private static void LogIssues(DefinitionValidationReport report)
    {
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
    }

    private static string FormatIssueForLog(ValidationIssue issue)
    {
        var builder = new StringBuilder();
        builder.Append($"[Validation] [{issue.Code}] [{issue.Registry}] {issue.Message}");

        if (!string.IsNullOrWhiteSpace(issue.AssetPath) ||
            !string.IsNullOrWhiteSpace(issue.AssetId) ||
            !string.IsNullOrWhiteSpace(issue.Field))
        {
            builder.Append(" | ");
            builder.Append(
                $"assetPath={issue.AssetPath ?? "n/a"}, assetId={issue.AssetId ?? "n/a"}, field={issue.Field ?? "n/a"}");
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

        foreach (var registry in registries)
        {
            if (registry is IDefinitionRegistryValidator validator)
                validators.Add(validator);
        }

        var singletonSnapshot = SeedRegistrySingletonInstances(validators);

        try
        {
            validators.Sort((left, right) =>
                string.Compare(left.RegistryName, right.RegistryName, StringComparison.Ordinal));

            foreach (var validator in validators)
            {
                validator.CollectReferenceMap(referenceMap);
                validator.ValidateAll(report);
            }

            PrefabRegistry.AppendValidationIssues(report);
        }
        finally
        {
            singletonSnapshot.Restore();
        }
    }

    private static SingletonInstanceSnapshot SeedRegistrySingletonInstances(IEnumerable<IDefinitionRegistryValidator> validators)
    {
        var snapshot = new SingletonInstanceSnapshot();
        var mismatchCountsByType = new Dictionary<Type, int>();

        foreach (var validator in validators)
        {
            if (validator is not MonoBehaviour registry)
                continue;

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

            if (!snapshot.HasEntryFor(registryType))
                snapshot.Add(registryType, instanceProperty, NormalizeUnityNull(instanceProperty.GetValue(null)));

            var existing = NormalizeUnityNull(instanceProperty.GetValue(null));
            if (existing == null)
            {
                setter.Invoke(null, new object[] { registry });
                continue;
            }

            if (!ReferenceEquals(existing, registry))
            {
                mismatchCountsByType[registryType] =
                    mismatchCountsByType.TryGetValue(registryType, out var mismatchCount)
                        ? mismatchCount + 1
                        : 1;
            }
        }

        foreach (var mismatchEntry in mismatchCountsByType)
            Debug.LogWarning(
                $"[Validation] Singleton mismatch for {mismatchEntry.Key.Name}; kept existing instance for {mismatchEntry.Value} validator candidate(s).");

        return snapshot;
    }

    private static object NormalizeUnityNull(object value)
    {
        if (value is UnityEngine.Object unityObject && unityObject == null)
            return null;

        return value;
    }

    private sealed class SingletonInstanceSnapshot
    {
        private readonly List<SnapshotEntry> entries = new();

        public bool HasEntryFor(Type registryType)
        {
            foreach (var entry in entries)
            {
                if (entry.RegistryType == registryType)
                    return true;
            }

            return false;
        }

        public void Add(Type registryType, PropertyInfo instanceProperty, object originalValue)
        {
            entries.Add(new SnapshotEntry(registryType, instanceProperty, originalValue));
        }

        public void Restore()
        {
            foreach (var entry in entries)
            {
                var setter = entry.InstanceProperty.GetSetMethod(true);
                if (setter == null)
                    continue;

                setter.Invoke(null, new[] { entry.OriginalValue });
            }
        }

        private readonly struct SnapshotEntry
        {
            public SnapshotEntry(Type registryType, PropertyInfo instanceProperty, object originalValue)
            {
                RegistryType = registryType;
                InstanceProperty = instanceProperty;
                OriginalValue = originalValue;
            }

            public Type RegistryType { get; }
            public PropertyInfo InstanceProperty { get; }
            public object OriginalValue { get; }
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