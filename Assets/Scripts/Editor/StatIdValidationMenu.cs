#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public static class StatIdValidationMenu
{
    private static readonly Regex ArrayElementRegex = new(@"^(.*)\.Array\.data\[(\d+)\]$", RegexOptions.Compiled);
    private const string StrictModeMenuPath = "Tools/Validation/Strict Canonical Stat IDs";

    private readonly struct MigrationPhaseResult
    {
        public MigrationPhaseResult(int updatedAssets, int updatedFields)
        {
            UpdatedAssets = updatedAssets;
            UpdatedFields = updatedFields;
        }

        public int UpdatedAssets { get; }
        public int UpdatedFields { get; }
    }

    [MenuItem("Tools/Validation/Migrate Legacy Stat IDs")]
    public static void MigrateLegacyStatIds()
    {
        var legacyFieldLift = StatFieldMigrationUtility.RunLegacyFieldLiftPhase();
        var canonicalRewrite = RewriteCanonicalStatIdsPhase();
        var cleanup = CleanupEmptyStatReferencesPhase();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(
            "[Validation] Legacy stat migration complete. " +
            $"Phase legacy field lift: {legacyFieldLift.LiftedFields} field(s) across {legacyFieldLift.UpdatedAssets} asset(s). " +
            $"Phase canonical statId rewrite: {canonicalRewrite.UpdatedFields} field(s) across {canonicalRewrite.UpdatedAssets} asset(s). " +
            $"Phase cleanup/removals: {cleanup.UpdatedFields} removal(s) across {cleanup.UpdatedAssets} asset(s).");
    }

    [MenuItem("Tools/Validation/Validate Canonical Stat IDs")]
    public static void ValidateCanonicalStatIdsMenu()
    {
        var report = new DefinitionValidationReport();
        AppendCanonicalStatIdIssues(report);
        if (!report.HasErrors)
        {
            Debug.Log("[Validation] Canonical stat ID validation passed.");
            EditorUtility.DisplayDialog("Validation", "Canonical stat ID validation passed.", "OK");
            return;
        }

        foreach (var issue in report.Issues)
            Debug.LogError($"[Validation] ({issue.Code}) {issue.Message}");

        Debug.LogError($"[Validation] Canonical stat ID validation failed with {report.ErrorCount} issue(s).");
        EditorUtility.DisplayDialog("Validation", $"Validation found {report.ErrorCount} issue(s). Check Console for details.", "OK");
    }

    [MenuItem(StrictModeMenuPath)]
    private static void ToggleStrictCanonicalMode()
    {
        StatIdValidationSettings.StrictCanonicalStatIds = !StatIdValidationSettings.StrictCanonicalStatIds;
        Menu.SetChecked(StrictModeMenuPath, StatIdValidationSettings.StrictCanonicalStatIds);
        Debug.Log($"[Validation] Strict canonical stat ID mode {(StatIdValidationSettings.StrictCanonicalStatIds ? "enabled" : "disabled")}.");
    }

    [MenuItem(StrictModeMenuPath, true)]
    private static bool ToggleStrictCanonicalModeValidate()
    {
        Menu.SetChecked(StrictModeMenuPath, StatIdValidationSettings.StrictCanonicalStatIds);
        return true;
    }

    // CI/batch-mode entry point.
    public static void ValidateCanonicalStatIdsForCI()
    {
        var report = new DefinitionValidationReport();
        AppendCanonicalStatIdIssues(report);
        if (!report.HasErrors)
        {
            Debug.Log("[Validation] Canonical stat ID CI validation passed.");
            return;
        }

        foreach (var issue in report.Issues)
            Debug.LogError($"[Validation] ({issue.Code}) {issue.Message}");

        throw new Exception($"Canonical stat ID validation failed with {report.ErrorCount} issue(s).");
    }

    public static void AppendCanonicalStatIdIssues(DefinitionValidationReport report)
    {
        var statDefinitions = LoadStatDefinitionIds();

        ValidateCatalogParity(statDefinitions, report);
        ValidateGeneratedConstants(report);

        foreach (var path in EnumerateScriptableObjectAssetPaths())
        {
            var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            if (asset == null)
                continue;

            if (asset is StatDefinition statDefinition)
                ValidateStatDefinition(statDefinition, path, report);

            ValidateSerializedStatIdFields(asset, path, statDefinitions, report);
        }
    }

    private static HashSet<string> LoadStatDefinitionIds()
    {
        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var guid in AssetDatabase.FindAssets("t:StatDefinition"))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<StatDefinition>(path);
            if (asset != null && !string.IsNullOrWhiteSpace(asset.Id))
                ids.Add(asset.Id);
        }

        return ids;
    }

    private static void ValidateCatalogParity(HashSet<string> statDefinitionIds, DefinitionValidationReport report)
    {
        var catalogIds = new HashSet<string>(CanonicalStatIds.Catalog, StringComparer.Ordinal);

        foreach (var catalogId in catalogIds)
        {
            if (!statDefinitionIds.Contains(catalogId))
                report.AddIssue(new ValidationIssue("STAT_CATALOG_PARITY_MISSING_DEFINITION", ValidationIssueSeverity.Error, nameof(StatIdValidationMenu), $"CanonicalStatIds.Catalog includes '{catalogId}' but no StatDefinition asset exists at Assets/GameData/Stats.", suggestedFix: "Create missing StatDefinition asset or remove ID from catalog."));
        }

        foreach (var statDefinitionId in statDefinitionIds)
        {
            if (!catalogIds.Contains(statDefinitionId))
                report.AddIssue(new ValidationIssue("STAT_CATALOG_PARITY_MISSING_CATALOG_ENTRY", ValidationIssueSeverity.Error, nameof(StatIdValidationMenu), $"StatDefinition id '{statDefinitionId}' is missing from CanonicalStatIds.Catalog.", assetId: statDefinitionId, field: "id", suggestedFix: "Add ID to CanonicalStatIds.Catalog via generator flow."));
        }
    }

    private static void ValidateGeneratedConstants(DefinitionValidationReport report)
    {
        if (!CanonicalStatIdsGenerator.IsCanonicalStatIdsSourceCurrent(out var reason))
            report.AddIssue(new ValidationIssue("STAT_CANONICAL_CONSTANTS_OUTDATED", ValidationIssueSeverity.Error, nameof(StatIdValidationMenu), reason, suggestedFix: "Regenerate CanonicalStatIds constants."));
    }

    private static void ValidateStatDefinition(StatDefinition definition, string path, DefinitionValidationReport report)
    {
        bool strictMode = StatIdValidationSettings.StrictCanonicalStatIds;

        if (string.IsNullOrWhiteSpace(definition.Id))
        {
            report.AddIssue(new ValidationIssue("STAT_DEFINITION_EMPTY_ID", ValidationIssueSeverity.Error, nameof(StatIdValidationMenu), $"StatDefinition '{path}' has an empty id.", assetPath: path, field: "id", suggestedFix: "Set a canonical stat ID."));
            return;
        }

        if (strictMode && StatIdCompatibilityMap.LegacyToCanonical.TryGetValue(definition.Id, out var requiredCanonicalId))
        {
            report.AddIssue(new ValidationIssue("STAT_LEGACY_ID_IN_STRICT_MODE", ValidationIssueSeverity.Error, nameof(StatIdValidationMenu), $"Asset '{path}' property 'id' uses legacy stat ID '{definition.Id}'. Required canonical ID: '{requiredCanonicalId}'.", assetPath: path, assetId: definition.Id, field: "id", suggestedFix: "Run Tools/Validation/Migrate Legacy Stat IDs."));
        }

        if (!StatIdCanonicalization.IsCanonicalFormat(definition.Id))
            report.AddIssue(new ValidationIssue("STAT_NON_CANONICAL_FORMAT", ValidationIssueSeverity.Error, nameof(StatIdValidationMenu), $"StatDefinition '{path}' has non-canonical id format '{definition.Id}'.", assetPath: path, assetId: definition.Id, field: "id", suggestedFix: "Rename ID to canonical kebab-case format."));

        if (!StatIdCanonicalization.ExistsInCanonicalCatalog(definition.Id))
            report.AddIssue(new ValidationIssue("STAT_ID_NOT_IN_CATALOG", ValidationIssueSeverity.Error, nameof(StatIdValidationMenu), $"StatDefinition '{path}' id '{definition.Id}' is not present in CanonicalStatIds.Catalog.", assetPath: path, assetId: definition.Id, field: "id", suggestedFix: "Add ID to catalog or fix stat ID."));
    }

    private static void ValidateSerializedStatIdFields(ScriptableObject asset, string path, HashSet<string> statDefinitionIds, DefinitionValidationReport report)
    {
        bool strictMode = StatIdValidationSettings.StrictCanonicalStatIds;
        var serializedObject = new SerializedObject(asset);
        var iterator = serializedObject.GetIterator();

        if (!iterator.NextVisible(true))
            return;

        do
        {
            if (iterator.propertyType != SerializedPropertyType.String)
                continue;

            if (iterator.name != "statId" && iterator.name != "targetStatId")
                continue;

            var value = iterator.stringValue;
            if (string.IsNullOrWhiteSpace(value))
            {
                report.AddIssue(new ValidationIssue("STAT_REFERENCE_EMPTY", ValidationIssueSeverity.Error, nameof(StatIdValidationMenu), $"Asset '{path}' has empty {iterator.name} at '{iterator.propertyPath}'.", assetPath: path, field: iterator.propertyPath, suggestedFix: "Populate the stat reference with a canonical stat ID."));
                continue;
            }

            if (strictMode && StatIdCompatibilityMap.LegacyToCanonical.TryGetValue(value, out var requiredCanonicalId))
            {
                report.AddIssue(new ValidationIssue("STAT_LEGACY_REFERENCE_IN_STRICT_MODE", ValidationIssueSeverity.Error, nameof(StatIdValidationMenu), $"Asset '{path}' property '{iterator.propertyPath}' uses legacy stat ID '{value}'. Required canonical ID: '{requiredCanonicalId}'.", assetPath: path, assetId: value, field: iterator.propertyPath, suggestedFix: "Run Tools/Validation/Migrate Legacy Stat IDs."));
            }

            if (!StatIdCanonicalization.IsCanonicalFormat(value))
                report.AddIssue(new ValidationIssue("STAT_REFERENCE_NON_CANONICAL_FORMAT", ValidationIssueSeverity.Error, nameof(StatIdValidationMenu), $"Asset '{path}' has non-canonical {iterator.name} '{value}' at '{iterator.propertyPath}'.", assetPath: path, assetId: value, field: iterator.propertyPath, suggestedFix: "Rewrite to canonical stat ID format."));

            bool resolvable = statDefinitionIds.Contains(value) || StatIdCanonicalization.ExistsInCanonicalCatalog(value);
            if (!resolvable)
                report.AddIssue(new ValidationIssue("STAT_REFERENCE_UNRESOLVED", ValidationIssueSeverity.Error, nameof(StatIdValidationMenu), $"Asset '{path}' references unresolved {iterator.name} '{value}' at '{iterator.propertyPath}'.", assetPath: path, assetId: value, field: iterator.propertyPath, suggestedFix: "Point to an existing canonical StatDefinition."));

        } while (iterator.NextVisible(false));
    }

    private static MigrationPhaseResult RewriteCanonicalStatIdsPhase()
    {
        int updatedAssets = 0;
        int rewrittenFields = 0;

        foreach (var path in EnumerateScriptableObjectAssetPaths())
        {
            var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            if (asset == null)
                continue;

            var serializedObject = new SerializedObject(asset);
            var result = RewriteCanonicalStatIds(serializedObject);
            if (result.UpdatedFields == 0)
                continue;

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(asset);
            updatedAssets++;
            rewrittenFields += result.UpdatedFields;
        }

        return new MigrationPhaseResult(updatedAssets, rewrittenFields);
    }

    private static MigrationPhaseResult CleanupEmptyStatReferencesPhase()
    {
        int updatedAssets = 0;
        int removals = 0;

        foreach (var path in EnumerateScriptableObjectAssetPaths())
        {
            var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            if (asset == null)
                continue;

            var serializedObject = new SerializedObject(asset);
            int removed = RemoveEmptyStatReferences(serializedObject, path);
            if (removed == 0)
                continue;

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(asset);
            updatedAssets++;
            removals += removed;
        }

        return new MigrationPhaseResult(updatedAssets, removals);
    }

    private static MigrationPhaseResult RewriteCanonicalStatIds(SerializedObject serializedObject)
    {
        int rewrittenFields = 0;

        var iterator = serializedObject.GetIterator();
        if (!iterator.NextVisible(true))
            return new MigrationPhaseResult(0, 0);

        do
        {
            if (iterator.propertyType != SerializedPropertyType.String)
                continue;

            if (iterator.name != "statId" && iterator.name != "targetStatId")
                continue;

            if (string.IsNullOrWhiteSpace(iterator.stringValue))
                continue;

            if (!StatIdCanonicalization.TryGetCanonicalForMigration(iterator.stringValue, out var canonicalId))
                continue;

            if (string.Equals(iterator.stringValue, canonicalId, StringComparison.Ordinal))
                continue;

            iterator.stringValue = canonicalId;
            rewrittenFields++;
        } while (iterator.NextVisible(false));

        return new MigrationPhaseResult(rewrittenFields > 0 ? 1 : 0, rewrittenFields);
    }

    private static int RemoveEmptyStatReferences(SerializedObject serializedObject, string path)
    {
        var removals = new List<(string arrayPath, int index)>();
        var iterator = serializedObject.GetIterator();
        if (!iterator.NextVisible(true))
            return 0;

        do
        {
            if (iterator.propertyType != SerializedPropertyType.String)
                continue;

            if (iterator.name != "statId" && iterator.name != "targetStatId")
                continue;

            if (!string.IsNullOrWhiteSpace(iterator.stringValue))
                continue;

            if (TryGetArrayElementInfo(iterator.propertyPath, out var arrayPath, out var index))
                removals.Add((arrayPath, index));

            Debug.LogWarning($"[Validation] Removing empty {iterator.name} from '{path}' at '{iterator.propertyPath}'.");
        } while (iterator.NextVisible(false));

        if (removals.Count == 0)
            return 0;

        removals.Sort((a, b) =>
        {
            int compare = string.CompareOrdinal(a.arrayPath, b.arrayPath);
            return compare != 0 ? compare : b.index.CompareTo(a.index);
        });

        int removed = 0;
        foreach (var (arrayPath, index) in removals)
        {
            var array = serializedObject.FindProperty(arrayPath);
            if (array == null || !array.isArray || index < 0 || index >= array.arraySize)
                continue;

            array.DeleteArrayElementAtIndex(index);
            removed++;
        }

        return removed;
    }

    private static bool TryGetArrayElementInfo(string propertyPath, out string arrayPath, out int index)
    {
        arrayPath = null;
        index = -1;

        int separator = propertyPath.LastIndexOf('.');
        if (separator <= 0)
            return false;

        var parentPath = propertyPath.Substring(0, separator);
        var match = ArrayElementRegex.Match(parentPath);
        if (!match.Success)
            return false;

        arrayPath = match.Groups[1].Value;
        return int.TryParse(match.Groups[2].Value, out index);
    }

    private static IEnumerable<string> EnumerateScriptableObjectAssetPaths()
    {
        foreach (var guid in AssetDatabase.FindAssets("t:ScriptableObject"))
            yield return AssetDatabase.GUIDToAssetPath(guid);
    }
}
#endif
