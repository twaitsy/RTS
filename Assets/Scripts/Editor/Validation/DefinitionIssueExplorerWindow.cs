using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public sealed class DefinitionIssueExplorerWindow : EditorWindow
{
    private enum SortField
    {
        Severity,
        Code,
        Message,
        AssetPath
    }

    private sealed class IssueRow
    {
        public IssueRow(ValidationIssue issue)
        {
            Issue = issue;
            SelectionKey = BuildSelectionKey(issue);
        }

        public ValidationIssue Issue { get; }
        public string SelectionKey { get; }

        public static string BuildSelectionKey(ValidationIssue issue)
        {
            return string.Join("|",
                issue.Code ?? string.Empty,
                issue.AssetPath ?? string.Empty,
                issue.Field ?? string.Empty,
                issue.AssetId ?? string.Empty);
        }
    }

    private DefinitionValidationReport lastReport;
    private readonly List<IssueRow> flattenedIssues = new();
    private readonly List<IssueRow> visibleIssues = new();

    private bool showErrors = true;
    private bool showWarnings = true;
    private bool showInfos = true;
    private string searchText = string.Empty;
    private SortField sortField = SortField.Severity;
    private bool sortAscending;

    private string selectedIssueKey;
    private int selectedVisibleIndex = -1;

    private Vector2 leftScroll;
    private Vector2 rightScroll;

    [MenuItem("Tools/Validation/Issue Explorer")]
    public static void OpenWindow()
    {
        var window = GetWindow<DefinitionIssueExplorerWindow>("Issue Explorer");
        window.minSize = new Vector2(900f, 480f);
        window.Show();
    }

    private void OnGUI()
    {
        DrawToolbar();
        DrawFilters();

        if (lastReport == null)
        {
            EditorGUILayout.HelpBox("Run validation to load issues.", MessageType.Info);
            return;
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            DrawIssueListPane();
            DrawIssueDetailPane();
        }
    }

    private void DrawToolbar()
    {
        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            if (GUILayout.Button("Run Validation", EditorStyles.toolbarButton, GUILayout.Width(120f)))
                RunValidation();

            GUILayout.Space(10f);
            GUILayout.Label($"Issues: {flattenedIssues.Count} (Errors: {lastReport?.ErrorCount ?? 0})", EditorStyles.miniLabel);
            GUILayout.FlexibleSpace();
        }
    }

    private void DrawFilters()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            showErrors = GUILayout.Toggle(showErrors, "Error", "Button", GUILayout.Width(70f));
            showWarnings = GUILayout.Toggle(showWarnings, "Warning", "Button", GUILayout.Width(80f));
            showInfos = GUILayout.Toggle(showInfos, "Info", "Button", GUILayout.Width(60f));

            GUILayout.Space(8f);
            GUILayout.Label("Search", GUILayout.Width(45f));
            searchText = EditorGUILayout.TextField(searchText);

            GUILayout.Space(8f);
            GUILayout.Label("Sort", GUILayout.Width(30f));
            sortField = (SortField)EditorGUILayout.EnumPopup(sortField, GUILayout.Width(100f));
            sortAscending = GUILayout.Toggle(sortAscending, sortAscending ? "Asc" : "Desc", "Button", GUILayout.Width(52f));
        }

        RefreshVisibleIssues();
    }

    private void DrawIssueListPane()
    {
        using (new EditorGUILayout.VerticalScope(GUILayout.Width(position.width * 0.46f)))
        {
            EditorGUILayout.LabelField("Issues", EditorStyles.boldLabel);
            leftScroll = EditorGUILayout.BeginScrollView(leftScroll);

            if (visibleIssues.Count == 0)
            {
                EditorGUILayout.HelpBox("No issues match the current filters/search.", MessageType.None);
            }
            else
            {
                for (var index = 0; index < visibleIssues.Count; index++)
                {
                    var row = visibleIssues[index];
                    var issue = row.Issue;

                    using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        var selected = index == selectedVisibleIndex;
                        var title = $"[{issue.Severity}] {issue.Code}";
                        if (GUILayout.Toggle(selected, title, "Button"))
                        {
                            selectedVisibleIndex = index;
                            selectedIssueKey = row.SelectionKey;
                        }

                        EditorGUILayout.LabelField(issue.Message ?? string.Empty, EditorStyles.wordWrappedMiniLabel);
                        if (!string.IsNullOrWhiteSpace(issue.AssetPath))
                            EditorGUILayout.LabelField(issue.AssetPath, EditorStyles.miniLabel);
                    }
                }
            }

            EditorGUILayout.EndScrollView();
        }
    }

    private void DrawIssueDetailPane()
    {
        using (new EditorGUILayout.VerticalScope())
        {
            EditorGUILayout.LabelField("Details", EditorStyles.boldLabel);
            rightScroll = EditorGUILayout.BeginScrollView(rightScroll);

            var selectedIssue = GetSelectedIssue();
            if (selectedIssue == null)
            {
                EditorGUILayout.HelpBox("Select an issue to inspect details.", MessageType.Info);
            }
            else
            {
                DrawDetailField("Code", selectedIssue.Code);
                DrawDetailField("Severity", selectedIssue.Severity.ToString());
                DrawDetailField("Registry", selectedIssue.Registry);
                DrawDetailField("Message", selectedIssue.Message, true);
                DrawDetailField("AssetPath", selectedIssue.AssetPath);
                DrawDetailField("AssetId", selectedIssue.AssetId);
                DrawDetailField("Field", selectedIssue.Field);

                EditorGUILayout.Space(8f);
                EditorGUILayout.LabelField("SuggestedFix", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox(string.IsNullOrWhiteSpace(selectedIssue.SuggestedFix) ? "(none)" : selectedIssue.SuggestedFix, MessageType.None);
            }

            EditorGUILayout.EndScrollView();
        }
    }

    private void DrawDetailField(string label, string value, bool multiLine = false)
    {
        var displayValue = string.IsNullOrWhiteSpace(value) ? "(none)" : value;
        if (multiLine)
        {
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            EditorGUILayout.LabelField(displayValue, EditorStyles.wordWrappedLabel);
        }
        else
        {
            EditorGUILayout.LabelField(label, displayValue);
        }
    }

    private void RunValidation()
    {
        var previousSelectionKey = selectedIssueKey;

        lastReport = DefinitionValidationOrchestrator.RunValidation();
        flattenedIssues.Clear();
        flattenedIssues.AddRange(lastReport.Issues.Select(issue => new IssueRow(issue)));

        RefreshVisibleIssues();
        RestoreSelection(previousSelectionKey);
        Repaint();
    }

    private void RefreshVisibleIssues()
    {
        visibleIssues.Clear();

        foreach (var row in flattenedIssues)
        {
            if (!PassesSeverityFilter(row.Issue))
                continue;

            if (!PassesSearchFilter(row.Issue))
                continue;

            visibleIssues.Add(row);
        }

        visibleIssues.Sort(CompareRows);

        if (selectedVisibleIndex >= visibleIssues.Count)
            selectedVisibleIndex = -1;

        if (selectedVisibleIndex < 0 && visibleIssues.Count > 0 && !string.IsNullOrWhiteSpace(selectedIssueKey))
            RestoreSelection(selectedIssueKey);
    }

    private void RestoreSelection(string selectionKey)
    {
        selectedIssueKey = selectionKey;
        selectedVisibleIndex = -1;

        if (string.IsNullOrWhiteSpace(selectionKey))
            return;

        for (var index = 0; index < visibleIssues.Count; index++)
        {
            if (string.Equals(visibleIssues[index].SelectionKey, selectionKey, StringComparison.Ordinal))
            {
                selectedVisibleIndex = index;
                return;
            }
        }
    }

    private int CompareRows(IssueRow left, IssueRow right)
    {
        var result = sortField switch
        {
            SortField.Severity => CompareSeverity(left.Issue.Severity, right.Issue.Severity),
            SortField.Code => string.Compare(left.Issue.Code, right.Issue.Code, StringComparison.OrdinalIgnoreCase),
            SortField.Message => string.Compare(left.Issue.Message, right.Issue.Message, StringComparison.OrdinalIgnoreCase),
            SortField.AssetPath => string.Compare(left.Issue.AssetPath, right.Issue.AssetPath, StringComparison.OrdinalIgnoreCase),
            _ => 0
        };

        if (result == 0)
            result = string.Compare(left.Issue.Message, right.Issue.Message, StringComparison.OrdinalIgnoreCase);

        return sortAscending ? result : -result;
    }

    private static int CompareSeverity(ValidationIssueSeverity left, ValidationIssueSeverity right)
    {
        return SeverityRank(left).CompareTo(SeverityRank(right));
    }

    private static int SeverityRank(ValidationIssueSeverity severity)
    {
        return severity switch
        {
            ValidationIssueSeverity.Error => 3,
            ValidationIssueSeverity.Warning => 2,
            ValidationIssueSeverity.Info => 1,
            _ => 0
        };
    }

    private bool PassesSeverityFilter(ValidationIssue issue)
    {
        return issue.Severity switch
        {
            ValidationIssueSeverity.Error => showErrors,
            ValidationIssueSeverity.Warning => showWarnings,
            ValidationIssueSeverity.Info => showInfos,
            _ => false
        };
    }

    private bool PassesSearchFilter(ValidationIssue issue)
    {
        if (string.IsNullOrWhiteSpace(searchText))
            return true;

        var term = searchText.Trim();
        return ContainsIgnoreCase(issue.Code, term)
               || ContainsIgnoreCase(issue.Message, term)
               || ContainsIgnoreCase(issue.AssetPath, term);
    }

    private static bool ContainsIgnoreCase(string source, string term)
    {
        return !string.IsNullOrWhiteSpace(source)
               && source.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private ValidationIssue GetSelectedIssue()
    {
        if (selectedVisibleIndex < 0 || selectedVisibleIndex >= visibleIssues.Count)
            return null;

        return visibleIssues[selectedVisibleIndex].Issue;
    }
}
