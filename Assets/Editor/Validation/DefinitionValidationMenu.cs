#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class DefinitionValidationMenu
{
    [MenuItem("Tools/Validation/Validate Stat Modifier Links")]
    public static void ValidateStatModifierLinks()
    {
        var stats = LoadById<StatDefinition>();
        var statModifiers = LoadById<StatModifierDefinition>();

        var errors = new List<string>();

        StatModifierLinkValidator.ValidateStatModifierDefinitions(
            statModifiers.Values,
            statId => stats.ContainsKey(statId),
            message => errors.Add(message));

        ValidateHosts(
            LoadAll<NeedDefinition>(),
            statModifiers,
            stats,
            new HashSet<StatDomain> { StatDomain.Needs, StatDomain.Mood },
            need => need.AllowAnyModifierDomain,
            "Needs or Mood",
            errors);

        ValidateHosts(LoadAll<DiseaseDefinition>(), statModifiers, stats, new HashSet<StatDomain>(), _ => true, "any domain", errors);
        ValidateHosts(LoadAll<EffectDefinition>(), statModifiers, stats, new HashSet<StatDomain>(), _ => true, "any domain", errors);
        ValidateHosts(LoadAll<EventDefinition>(), statModifiers, stats, new HashSet<StatDomain>(), _ => true, "any domain", errors);
        ValidateHosts(LoadAll<TechDefinition>(), statModifiers, stats, new HashSet<StatDomain>(), _ => true, "any domain", errors);

        if (errors.Count == 0)
        {
            Debug.Log("[Validation] Stat modifier link validation passed.");
            EditorUtility.DisplayDialog("Validation", "Stat modifier link validation passed.", "OK");
            return;
        }

        foreach (var error in errors)
            Debug.LogError(error);

        Debug.LogError($"[Validation] Stat modifier link validation failed with {errors.Count} issue(s).");
        EditorUtility.DisplayDialog("Validation", $"Validation found {errors.Count} issue(s). Check Console for details.", "OK");
    }

    // CI/batch-mode entry point for all editor validations.
    public static void ValidateAllForCI()
    {
        StatIdValidationMenu.ValidateCanonicalStatIdsForCI();
        ValidateStatModifierLinksForCI();
    }

    public static void ValidateStatModifierLinksForCI()
    {
        var stats = LoadById<StatDefinition>();
        var statModifiers = LoadById<StatModifierDefinition>();

        var errors = new List<string>();

        StatModifierLinkValidator.ValidateStatModifierDefinitions(
            statModifiers.Values,
            statId => stats.ContainsKey(statId),
            message => errors.Add(message));

        ValidateHosts(
            LoadAll<NeedDefinition>(),
            statModifiers,
            stats,
            new HashSet<StatDomain> { StatDomain.Needs, StatDomain.Mood },
            need => need.AllowAnyModifierDomain,
            "Needs or Mood",
            errors);

        ValidateHosts(LoadAll<DiseaseDefinition>(), statModifiers, stats, new HashSet<StatDomain>(), _ => true, "any domain", errors);
        ValidateHosts(LoadAll<EffectDefinition>(), statModifiers, stats, new HashSet<StatDomain>(), _ => true, "any domain", errors);
        ValidateHosts(LoadAll<EventDefinition>(), statModifiers, stats, new HashSet<StatDomain>(), _ => true, "any domain", errors);
        ValidateHosts(LoadAll<TechDefinition>(), statModifiers, stats, new HashSet<StatDomain>(), _ => true, "any domain", errors);

        if (errors.Count == 0)
        {
            Debug.Log("[Validation] Stat modifier link CI validation passed.");
            return;
        }

        foreach (var error in errors)
            Debug.LogError(error);

        throw new System.Exception($"Stat modifier link validation failed with {errors.Count} issue(s).");
    }

    private static void ValidateHosts<THost>(
        List<THost> hosts,
        Dictionary<string, StatModifierDefinition> statModifiers,
        Dictionary<string, StatDefinition> stats,
        HashSet<StatDomain> allowedDomains,
        System.Func<THost, bool> allowAnyDomain,
        string expectedDomainLabel,
        List<string> errors)
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
            message => errors.Add(message));
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
