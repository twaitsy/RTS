#if UNITY_EDITOR
using UnityEditor;

public static class PrefabRegistryEditorValidator
{
    private const string MissingPrefabIssueCode = "PREFAB_DEFINITION_MISSING_PREFAB";

    public static void AppendValidationIssues(DefinitionValidationReport report)
    {
        if (report == null)
            return;

        PrefabRegistry.Initialize();

        foreach (var definition in PrefabRegistry.All())
        {
            if (definition == null)
                continue;

            var id = definition.Id?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(id))
            {
                report.AddError(nameof(PrefabRegistry), $"[Validation] Asset '{definition.name}' has empty field '{nameof(PrefabDefinition.Id)}'.");
                continue;
            }

            if (definition.Prefab != null)
                continue;

            var assetPath = AssetDatabase.GetAssetPath(definition);
            report.AddIssue(new ValidationIssue(
                code: MissingPrefabIssueCode,
                severity: ValidationIssueSeverity.Error,
                registry: nameof(PrefabRegistry),
                message: $"[Validation] Asset '{definition.name}' (id: '{id}') field '{nameof(PrefabDefinition.Prefab)}' is required.",
                assetPath: assetPath,
                assetId: id,
                field: "prefab",
                suggestedFix: "Assign a prefab asset from Assets/Prefabs."));
        }
    }
}
#endif
