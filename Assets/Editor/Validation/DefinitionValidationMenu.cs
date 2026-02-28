#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class DefinitionValidationMenu
{
    [MenuItem("Tools/Validation/Validate All Definitions")]
    public static void ValidateAllDefinitionsInEditor()
    {
        var report = DefinitionValidationOrchestrator.RunValidationAndLog();
        var status = report.HasErrors
            ? $"Validation failed with {report.ErrorCount} error(s)."
            : "Validation passed with 0 errors.";
        EditorUtility.DisplayDialog("Definition Validation", status, "OK");
    }

    [MenuItem("Tools/Validation/Validate Stat Modifier Links")]
    public static void ValidateStatModifierLinks()
    {
        var report = new DefinitionValidationReport();
        AppendStatModifierLinkIssues(report);

        if (!report.HasErrors)
        {
            Debug.Log("[Validation] Stat modifier link validation passed.");
            EditorUtility.DisplayDialog("Validation", "Stat modifier link validation passed.", "OK");
            return;
        }

        foreach (var issue in report.Issues)
            Debug.LogError($"[Validation] ({issue.Code}) {issue.Message}");

        Debug.LogError($"[Validation] Stat modifier link validation failed with {report.ErrorCount} issue(s).");
        EditorUtility.DisplayDialog("Validation", $"Validation found {report.ErrorCount} issue(s). Check Console for details.", "OK");
    }

    // CI/batch-mode entry point for all editor validations.
    public static void ValidateAllForCI()
    {
        var report = DefinitionValidationOrchestrator.RunValidationAndLog();
        if (report.HasErrors)
            throw new Exception($"Definition validation failed with {report.ErrorCount} issue(s).");
    }

    public static void ValidateStatModifierLinksForCI()
    {
        var report = new DefinitionValidationReport();
        AppendStatModifierLinkIssues(report);

        if (!report.HasErrors)
        {
            Debug.Log("[Validation] Stat modifier link CI validation passed.");
            return;
        }

        foreach (var issue in report.Issues)
            Debug.LogError($"[Validation] ({issue.Code}) {issue.Message}");

        throw new Exception($"Stat modifier link validation failed with {report.ErrorCount} issue(s).");
    }

    public static void AppendStatModifierLinkIssues(DefinitionValidationReport report)
    {
        var stats = LoadById<StatDefinition>();
        var statModifiers = LoadById<StatModifierDefinition>();

        StatModifierLinkValidator.ValidateStatModifierDefinitions(
            statModifiers.Values,
            statId => stats.ContainsKey(statId),
            message => report.AddIssue(new ValidationIssue(
                code: "STAT_MODIFIER_LINK_INVALID",
                severity: ValidationIssueSeverity.Error,
                registry: nameof(DefinitionValidationMenu),
                message: message,
                suggestedFix: "Update stat modifier/stat definitions so all links are valid.")));

        ValidateHosts(
            LoadAll<NeedDefinition>(),
            statModifiers,
            stats,
            new HashSet<StatDomain> { StatDomain.Needs, StatDomain.Mood },
            need => need.AllowAnyModifierDomain,
            "Needs or Mood",
            report);

        ValidateHosts(LoadAll<DiseaseDefinition>(), statModifiers, stats, new HashSet<StatDomain>(), _ => true, "any domain", report);
        ValidateHosts(LoadAll<EffectDefinition>(), statModifiers, stats, new HashSet<StatDomain>(), _ => true, "any domain", report);
        ValidateHosts(LoadAll<EventDefinition>(), statModifiers, stats, new HashSet<StatDomain>(), _ => true, "any domain", report);
        ValidateHosts(LoadAll<TechDefinition>(), statModifiers, stats, new HashSet<StatDomain>(), _ => true, "any domain", report);
    }

    private static void ValidateHosts<THost>(
        List<THost> hosts,
        Dictionary<string, StatModifierDefinition> statModifiers,
        Dictionary<string, StatDefinition> stats,
        HashSet<StatDomain> allowedDomains,
        Func<THost, bool> allowAnyDomain,
        string expectedDomainLabel,
        DefinitionValidationReport report)
        where THost : ScriptableObject, IIdentifiable
    {
        StatModifierLinkValidator.ValidateHostStatModifierLinks(
            hosts,
            host => host.Id,
            host => GetStatModifierIds(host),
            host => host.name,
            allowAnyDomain,
            modifierId => statModifiers.ContainsKey(modifierId),
            modifierId => statModifiers.TryGetValue(modifierId, out var statModifier) ? statModifier : null,
            statId => stats.ContainsKey(statId),
            statId => stats.TryGetValue(statId, out var stat) ? stat : null,
            allowedDomains,
            expectedDomainLabel,
            message => report.AddIssue(new ValidationIssue(
                code: "STAT_MODIFIER_LINK_INVALID",
                severity: ValidationIssueSeverity.Error,
                registry: nameof(DefinitionValidationMenu),
                message: message,
                suggestedFix: "Update stat modifier IDs and stat domains to satisfy link rules.")));
    }

    private static IReadOnlyList<string> GetStatModifierIds<THost>(THost host)
    {
        return host switch
        {
            NeedDefinition need => need.StatModifierIds,
            DiseaseDefinition disease => disease.StatModifierIds,
            EffectDefinition effect => effect.StatModifierIds,
            EventDefinition eventDefinition => eventDefinition.StatModifierIds,
            TechDefinition tech => tech.StatModifierIds,
            _ => new List<string>()
        };
    }

    private static Dictionary<string, T> LoadById<T>() where T : ScriptableObject, IIdentifiable
    {
        var dictionary = new Dictionary<string, T>();

        foreach (var definition in LoadAll<T>())
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.Id))
                continue;

            dictionary[definition.Id] = definition;
        }

        return dictionary;
    }

    private static List<T> LoadAll<T>() where T : ScriptableObject
    {
        var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
        var assets = new List<T>(guids.Length);

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
                assets.Add(asset);
        }

        return assets;
    }
}
#endif
