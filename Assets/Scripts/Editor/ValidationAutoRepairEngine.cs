using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public enum MissingReferenceRepairPolicy
{
    SuggestNearest,
    ClearField
}

public enum ValidationAutoRepairMode
{
    ValidateOnly,
    ValidateAndApplySafeFixes,
    ValidateAndPreviewFixScript
}

public sealed class ValidationAutoRepairOptions
{
    public MissingReferenceRepairPolicy MissingReferencePolicy { get; set; } = MissingReferenceRepairPolicy.SuggestNearest;
}

public static class ValidationAutoRepairEngine
{
    private const string ValidationRegistryName = nameof(ValidationAutoRepairEngine);

    private sealed class DefinitionAssetRecord
    {
        public ScriptableObject Asset;
        public string Path;
        public string Id;
        public string Registry;
    }

    private sealed class PlannedChange
    {
        public ScriptableObject Asset;
        public string Path;
        public string PropertyPath;
        public string PreviousValue;
        public string NewValue;
        public string Reason;
    }

    [MenuItem("Tools/Validation/Definitions/Validate only")]
    public static void ValidateOnlyMenu()
    {
        Run(ValidationAutoRepairMode.ValidateOnly, new ValidationAutoRepairOptions());
    }

    [MenuItem("Tools/Validation/Definitions/Validate + apply safe fixes")]
    public static void ValidateApplySafeFixesMenu()
    {
        Run(ValidationAutoRepairMode.ValidateAndApplySafeFixes, new ValidationAutoRepairOptions());
    }

    [MenuItem("Tools/Validation/Definitions/Validate + preview fix script")]
    public static void ValidatePreviewFixScriptMenu()
    {
        Run(ValidationAutoRepairMode.ValidateAndPreviewFixScript, new ValidationAutoRepairOptions());
    }

    public static DefinitionValidationReport Run(ValidationAutoRepairMode mode, ValidationAutoRepairOptions options)
    {
        options ??= new ValidationAutoRepairOptions();

        var report = DefinitionValidationOrchestrator.RunValidation();
        var plannedChanges = BuildPlannedChangesFromIssues(report, options);

        if (mode == ValidationAutoRepairMode.ValidateAndApplySafeFixes)
            ApplyPlannedChanges(plannedChanges, report);
        else if (mode == ValidationAutoRepairMode.ValidateAndPreviewFixScript)
            LogPreviewScript(plannedChanges);

        var summary = report.BuildSummary();
        if (report.HasErrors)
            Debug.LogError(summary);
        else
            Debug.Log(summary);

        return report;
    }

    public static void AppendRepairableIssues(DefinitionValidationReport report)
    {
        var records = CollectDefinitions();
        var idSet = new HashSet<string>(records.Select(record => record.Id), StringComparer.Ordinal);
        var inboundReferences = new Dictionary<string, int>(StringComparer.Ordinal);

        AppendIdNormalizationIssues(records, report);
        AppendDuplicateIdIssues(records, report);
        AppendMissingReferenceIssues(records, idSet, inboundReferences, report);
        AppendOrphanedDefinitionIssues(records, inboundReferences, report);
    }

    private static List<DefinitionAssetRecord> CollectDefinitions()
    {
        var records = new List<DefinitionAssetRecord>();

        foreach (var guid in AssetDatabase.FindAssets("t:ScriptableObject"))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            if (asset == null)
                continue;

            var serializedObject = new SerializedObject(asset);
            var idProperty = serializedObject.FindProperty("id");
            if (idProperty == null || idProperty.propertyType != SerializedPropertyType.String)
                continue;

            var id = idProperty.stringValue?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(id))
                continue;

            records.Add(new DefinitionAssetRecord
            {
                Asset = asset,
                Path = path,
                Id = id,
                Registry = asset.GetType().Name
            });
        }

        return records;
    }

    private static void AppendIdNormalizationIssues(List<DefinitionAssetRecord> records, DefinitionValidationReport report)
    {
        var normalizedGroups = records
            .GroupBy(record => DefinitionIdLifecycle.NormalizeId(record.Id), StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.Ordinal);

        foreach (var record in records)
        {
            var normalized = DefinitionIdLifecycle.NormalizeId(record.Id);
            if (string.Equals(record.Id, normalized, StringComparison.Ordinal) && DefinitionIdLifecycle.IsValidIdFormat(record.Id))
                continue;

            report.AddIssue(new ValidationIssue(
                code: "INVALID_OR_NONCANONICAL_ID",
                severity: ValidationIssueSeverity.Warning,
                registry: record.Registry,
                message: $"Definition id '{record.Id}' is invalid or non-canonical.",
                assetPath: record.Path,
                assetId: record.Id,
                field: "id",
                suggestedFix: $"Normalize id '{record.Id}' -> '{normalized}'."));

            if (string.IsNullOrWhiteSpace(normalized) || !DefinitionIdLifecycle.IsValidIdFormat(normalized))
                continue;

            if (normalizedGroups.TryGetValue(normalized, out var group) && group.Count > 1)
                continue;

            report.AddIssue(new ValidationIssue(
                code: "SAFE_FIX_PLAN_NORMALIZE_ID",
                severity: ValidationIssueSeverity.Info,
                registry: ValidationRegistryName,
                message: $"Planned safe fix: normalize id '{record.Id}' -> '{normalized}'.",
                assetPath: record.Path,
                assetId: record.Id,
                field: "id",
                suggestedFix: normalized));

            report.AddIssue(new ValidationIssue(
                code: "SAFE_FIX_PLAN_SYNC_FINALIZED_ID",
                severity: ValidationIssueSeverity.Info,
                registry: ValidationRegistryName,
                message: $"Planned safe fix: sync finalizedId to '{normalized}'.",
                assetPath: record.Path,
                assetId: string.Empty,
                field: "finalizedId",
                suggestedFix: normalized));
        }
    }

    private static void AppendDuplicateIdIssues(List<DefinitionAssetRecord> records, DefinitionValidationReport report)
    {
        foreach (var duplicateGroup in records.GroupBy(record => record.Id, StringComparer.Ordinal).Where(group => group.Count() > 1))
        {
            foreach (var record in duplicateGroup)
            {
                var suggestions = BuildRenameSuggestions(record.Id).ToArray();
                report.AddIssue(new ValidationIssue(
                    code: "DUPLICATE_ID",
                    severity: ValidationIssueSeverity.Error,
                    registry: record.Registry,
                    message: $"Duplicate id '{record.Id}' detected ({duplicateGroup.Count()} assets).",
                    assetPath: record.Path,
                    assetId: record.Id,
                    field: "id",
                    suggestedFix: $"Rename to one of: {string.Join(", ", suggestions)}"));
            }
        }
    }

    private static void AppendMissingReferenceIssues(
        List<DefinitionAssetRecord> records,
        HashSet<string> idSet,
        Dictionary<string, int> inboundReferences,
        DefinitionValidationReport report)
    {
        foreach (var record in records)
        {
            var serializedObject = new SerializedObject(record.Asset);
            var iterator = serializedObject.GetIterator();
            var enterChildren = true;

            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = true;

                if (iterator.propertyType != SerializedPropertyType.String)
                    continue;

                if (!iterator.name.EndsWith("id", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (iterator.name.Equals("id", StringComparison.OrdinalIgnoreCase)
                    || iterator.name.Equals("finalizedId", StringComparison.OrdinalIgnoreCase))
                    continue;

                var value = iterator.stringValue?.Trim();
                if (string.IsNullOrWhiteSpace(value))
                    continue;

                if (idSet.Contains(value))
                {
                    inboundReferences[value] = inboundReferences.TryGetValue(value, out var count) ? count + 1 : 1;
                    continue;
                }

                var nearest = FindNearestId(value, idSet);
                var suggestion = nearest != null
                    ? $"Replace with nearest valid id '{nearest}'."
                    : "No nearest match found; clear field if optional.";

                report.AddIssue(new ValidationIssue(
                    code: "MISSING_REFERENCE",
                    severity: ValidationIssueSeverity.Error,
                    registry: record.Registry,
                    message: $"Missing reference id '{value}' in field '{iterator.name}'.",
                    assetPath: record.Path,
                    assetId: value,
                    field: iterator.propertyPath,
                    suggestedFix: suggestion));

                report.AddIssue(new ValidationIssue(
                    code: "SAFE_FIX_PLAN_CLEAR_MISSING_REFERENCE",
                    severity: ValidationIssueSeverity.Info,
                    registry: ValidationRegistryName,
                    message: $"Planned safe fix: clear missing reference '{value}' at '{iterator.propertyPath}'.",
                    assetPath: record.Path,
                    assetId: value,
                    field: iterator.propertyPath,
                    suggestedFix: string.Empty));
            }
        }
    }

    private static void AppendOrphanedDefinitionIssues(List<DefinitionAssetRecord> records, Dictionary<string, int> inboundReferences, DefinitionValidationReport report)
    {
        foreach (var record in records)
        {
            if (inboundReferences.ContainsKey(record.Id))
                continue;

            report.AddIssue(new ValidationIssue(
                code: "ORPHANED_DEFINITION",
                severity: ValidationIssueSeverity.Info,
                registry: record.Registry,
                message: $"Definition '{record.Id}' has no inbound references.",
                assetPath: record.Path,
                assetId: record.Id,
                field: null,
                suggestedFix: "Review for archive/delete list."));
        }
    }

    private static List<PlannedChange> BuildPlannedChangesFromIssues(DefinitionValidationReport report, ValidationAutoRepairOptions options)
    {
        var plannedChanges = new List<PlannedChange>();

        foreach (var issue in report.Issues)
        {
            if (string.IsNullOrWhiteSpace(issue.AssetPath) || string.IsNullOrWhiteSpace(issue.Field))
                continue;

            var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(issue.AssetPath);
            if (asset == null)
                continue;

            switch (issue.Code)
            {
                case "SAFE_FIX_PLAN_NORMALIZE_ID":
                    plannedChanges.Add(new PlannedChange
                    {
                        Asset = asset,
                        Path = issue.AssetPath,
                        PropertyPath = issue.Field,
                        PreviousValue = issue.AssetId,
                        NewValue = issue.SuggestedFix,
                        Reason = "normalize-id"
                    });
                    break;

                case "SAFE_FIX_PLAN_SYNC_FINALIZED_ID":
                    plannedChanges.Add(new PlannedChange
                    {
                        Asset = asset,
                        Path = issue.AssetPath,
                        PropertyPath = issue.Field,
                        PreviousValue = issue.AssetId,
                        NewValue = issue.SuggestedFix,
                        Reason = "sync-finalized-id"
                    });
                    break;

                case "SAFE_FIX_PLAN_CLEAR_MISSING_REFERENCE" when options.MissingReferencePolicy == MissingReferenceRepairPolicy.ClearField:
                    plannedChanges.Add(new PlannedChange
                    {
                        Asset = asset,
                        Path = issue.AssetPath,
                        PropertyPath = issue.Field,
                        PreviousValue = issue.AssetId,
                        NewValue = string.Empty,
                        Reason = "clear-missing-reference"
                    });
                    break;
            }
        }

        return plannedChanges;
    }

    private static void ApplyPlannedChanges(List<PlannedChange> plannedChanges, DefinitionValidationReport report)
    {
        if (plannedChanges.Count == 0)
            return;

        var applied = 0;

        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (var groupedByAsset in plannedChanges.GroupBy(change => change.Asset))
            {
                var serializedObject = new SerializedObject(groupedByAsset.Key);
                var hasChanges = false;

                foreach (var change in groupedByAsset)
                {
                    var property = serializedObject.FindProperty(change.PropertyPath);
                    if (property == null || property.propertyType != SerializedPropertyType.String)
                        continue;

                    if (string.Equals(property.stringValue, change.NewValue, StringComparison.Ordinal))
                        continue;

                    property.stringValue = change.NewValue;
                    hasChanges = true;
                    applied++;
                }

                var finalizedToggle = serializedObject.FindProperty("isIdFinalized");
                if (hasChanges && finalizedToggle != null && finalizedToggle.propertyType == SerializedPropertyType.Boolean)
                    finalizedToggle.boolValue = true;

                if (!hasChanges)
                    continue;

                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(groupedByAsset.Key);
            }

            AssetDatabase.SaveAssets();
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.Refresh();
        }

        report.AddIssue(new ValidationIssue(
            code: "SAFE_FIXES_APPLIED",
            severity: ValidationIssueSeverity.Info,
            registry: ValidationRegistryName,
            message: $"Applied {applied} safe fix edit(s).",
            suggestedFix: "Re-run validation to confirm a clean state."));
    }

    private static void LogPreviewScript(List<PlannedChange> plannedChanges)
    {
        if (plannedChanges.Count == 0)
        {
            Debug.Log("[Validation] Preview fix script: no safe changes queued.");
            return;
        }

        var lines = plannedChanges
            .Select(change =>
                $"SET '{change.Path}' :: {change.PropertyPath} = '{change.NewValue}' // from '{change.PreviousValue}' [{change.Reason}]")
            .OrderBy(line => line, StringComparer.Ordinal)
            .ToArray();

        Debug.Log("[Validation] Preview fix script:\n" + string.Join("\n", lines));
    }

    private static IEnumerable<string> BuildRenameSuggestions(string id)
    {
        var normalized = DefinitionIdLifecycle.NormalizeId(id);
        if (string.IsNullOrWhiteSpace(normalized))
            normalized = "definition";

        yield return normalized + "-variant";
        yield return normalized + "-alt";
        yield return normalized + "-v2";
    }

    private static string FindNearestId(string missingId, HashSet<string> idSet)
    {
        var best = null as string;
        var bestDistance = int.MaxValue;

        foreach (var candidate in idSet)
        {
            var distance = LevenshteinDistance(missingId, candidate);
            if (distance >= bestDistance)
                continue;

            best = candidate;
            bestDistance = distance;
        }

        return bestDistance <= 3 ? best : null;
    }

    private static int LevenshteinDistance(string a, string b)
    {
        if (string.IsNullOrEmpty(a))
            return string.IsNullOrEmpty(b) ? 0 : b.Length;

        if (string.IsNullOrEmpty(b))
            return a.Length;

        var matrix = new int[a.Length + 1, b.Length + 1];

        for (var i = 0; i <= a.Length; i++)
            matrix[i, 0] = i;

        for (var j = 0; j <= b.Length; j++)
            matrix[0, j] = j;

        for (var i = 1; i <= a.Length; i++)
        {
            for (var j = 1; j <= b.Length; j++)
            {
                var cost = a[i - 1] == b[j - 1] ? 0 : 1;
                matrix[i, j] = Math.Min(
                    Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                    matrix[i - 1, j - 1] + cost);
            }
        }

        return matrix[a.Length, b.Length];
    }
}
