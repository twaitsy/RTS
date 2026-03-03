using UnityEditor;
using UnityEngine;

public static class ValidationIssueFixRouter
{
    public static bool CanFix(ValidationIssue issue, out string reason)
    {
        reason = string.Empty;

        if (issue == null)
        {
            reason = "Issue is missing.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(issue.AssetPath))
        {
            reason = "Fix is unavailable because AssetPath is empty.";
            return false;
        }

        var target = AssetDatabase.LoadAssetAtPath<ScriptableObject>(issue.AssetPath);
        if (target == null)
        {
            reason = "Fix is unavailable because the asset could not be loaded from AssetPath.";
            return false;
        }

        return true;
    }

    public static void OpenFix(ValidationIssue issue)
    {
        if (!CanFix(issue, out _))
            return;

        var target = AssetDatabase.LoadAssetAtPath<ScriptableObject>(issue.AssetPath);
        if (target == null)
            return;

        if (TryOpenKnownTool(issue, target))
            return;

        ShowManualFallback(issue, target);
    }

    private static bool TryOpenKnownTool(ValidationIssue issue, ScriptableObject target)
    {
        switch (issue.Code)
        {
            case "INVALID_OR_NONCANONICAL_ID":
            case "DUPLICATE_ID":
                DefinitionIdMigrationTool.OpenWithTarget(target, issue.Field, issue.SuggestedFix);
                return true;

            case "MISSING_REFERENCE" when target is BuildingDefinition building && IsBuildingField(issue):
                BuildingPrefabMigrationTool.OpenForBuilding(building, issue.Field, issue.SuggestedFix);
                return true;
        }

        return false;
    }

    private static bool IsBuildingField(ValidationIssue issue)
    {
        return !string.IsNullOrWhiteSpace(issue.Field)
               && (issue.Field.IndexOf("prefabId", System.StringComparison.OrdinalIgnoreCase) >= 0
                   || issue.Field.IndexOf("categoryId", System.StringComparison.OrdinalIgnoreCase) >= 0
                   || issue.Field.IndexOf("primaryCategoryId", System.StringComparison.OrdinalIgnoreCase) >= 0
                   || string.Equals(issue.Registry, nameof(BuildingDefinition), System.StringComparison.OrdinalIgnoreCase));
    }

    private static void ShowManualFallback(ValidationIssue issue, ScriptableObject target)
    {
        EditorGUIUtility.PingObject(target);
        Selection.activeObject = target;

        ManualFixHelpWindow.Open(issue, target);
    }

    private sealed class ManualFixHelpWindow : EditorWindow
    {
        private ValidationIssue issue;
        private ScriptableObject target;
        private Vector2 scroll;

        public static void Open(ValidationIssue selectedIssue, ScriptableObject selectedTarget)
        {
            var window = GetWindow<ManualFixHelpWindow>("Issue Fix Help");
            window.minSize = new Vector2(460f, 240f);
            window.issue = selectedIssue;
            window.target = selectedTarget;
            window.Show();
        }

        private void OnGUI()
        {
            if (issue == null)
            {
                EditorGUILayout.HelpBox("No issue selected.", MessageType.Info);
                return;
            }

            scroll = EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.LabelField($"Code: {issue.Code}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Asset: {issue.AssetPath}", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.LabelField($"Field: {issue.Field ?? "(none)"}", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Manual Steps", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                string.IsNullOrWhiteSpace(issue.SuggestedFix)
                    ? "No automatic route is available. Open the pinged asset and resolve this issue manually."
                    : issue.SuggestedFix,
                MessageType.Info);

            EditorGUILayout.Space(8f);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Ping Asset") && target != null)
                    EditorGUIUtility.PingObject(target);

                if (GUILayout.Button("Select Asset") && target != null)
                    Selection.activeObject = target;
            }

            EditorGUILayout.EndScrollView();
        }
    }
}
