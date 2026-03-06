using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class PrefabRegistry
{
    public const string PrefabRegistryAssetFolder = "Assets/GameData/PrefabRegistry";
    public const string LegacyPrefabRegistryAssetFolder = "Assets/Resources/GameData/PrefabRegistry";
    private const string ResourcesFolder = "GameData/PrefabRegistry";

    private static readonly List<PrefabDefinition> allDefinitions = new();
    private static readonly Dictionary<string, PrefabDefinition> definitionsById = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, GameObject> prefabsById = new(StringComparer.Ordinal);
    private static bool initialized;
    private static bool loggedFallbackSearch;

#if UNITY_EDITOR
    [InitializeOnLoadMethod]
    private static void InitializeOnEditorLoad()
    {
        Initialize();
    }
#endif

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeOnRuntimeLoad()
    {
        Initialize();
    }

    public static void Initialize()
    {
        allDefinitions.Clear();
        definitionsById.Clear();
        prefabsById.Clear();

        foreach (var definition in LoadDefinitions())
        {
            if (definition == null)
                continue;

            allDefinitions.Add(definition);

            var id = definition.Id?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(id))
            {
                Debug.LogError($"[Validation] [PrefabRegistry] Asset '{definition.name}' has an empty prefab definition id.");
                continue;
            }

            if (!definitionsById.TryAdd(id, definition))
            {
                Debug.LogError($"[Validation] [PrefabRegistry] Duplicate PrefabDefinition id '{id}' found in '{definition.name}' and '{definitionsById[id].name}'.");
                continue;
            }

            if (definition.Prefab == null)
            {
                Debug.LogError($"[Validation] [PrefabRegistry] PrefabDefinition '{definition.name}' (id: '{id}') has no prefab assigned.");
                continue;
            }

            prefabsById[id] = definition.Prefab;
        }

        initialized = true;
    }

    public static GameObject Get(string prefabId)
    {
        if (TryGet(prefabId, out var prefab))
            return prefab;

        Debug.LogError($"PrefabRegistry could not find prefab for id '{prefabId}'.");
        return null;
    }

    public static bool TryGet(string prefabId, out GameObject prefab)
    {
        EnsureInitialized();

        if (string.IsNullOrWhiteSpace(prefabId))
        {
            prefab = null;
            return false;
        }

        return prefabsById.TryGetValue(prefabId, out prefab);
    }

    public static bool TryGetDefinition(string prefabId, out PrefabDefinition definition)
    {
        EnsureInitialized();

        if (string.IsNullOrWhiteSpace(prefabId))
        {
            definition = null;
            return false;
        }

        return definitionsById.TryGetValue(prefabId, out definition);
    }

    public static IEnumerable<PrefabDefinition> All()
    {
        EnsureInitialized();
        return allDefinitions;
    }

    private static void EnsureInitialized()
    {
        if (!initialized)
            Initialize();
    }

    private static IEnumerable<PrefabDefinition> LoadDefinitions()
    {
#if UNITY_EDITOR
        // EDITOR PATH
        var searchFolders = GetSearchFolders();
        if (searchFolders.Length > 0)
        {
            foreach (var invalidType in AssetDatabase.FindAssets("t:ScriptableObject", searchFolders))
            {
                var invalidPath = AssetDatabase.GUIDToAssetPath(invalidType);
                var scriptableObject = AssetDatabase.LoadAssetAtPath<ScriptableObject>(invalidPath);
                if (scriptableObject != null && !(scriptableObject is PrefabDefinition))
                    Debug.LogError($"[Validation] [PrefabRegistry] Invalid asset type in prefab registry folder: '{invalidPath}' ({scriptableObject.GetType().Name}). Only PrefabDefinition assets are allowed.");
            }

            foreach (var guid in AssetDatabase.FindAssets($"t:{nameof(PrefabDefinition)}", searchFolders))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var definition = AssetDatabase.LoadAssetAtPath<PrefabDefinition>(path);
                if (definition != null)
                    yield return definition;
            }

            yield break;
        }

        if (!loggedFallbackSearch)
        {
            Debug.LogWarning($"[Validation] [PrefabRegistry] Expected folder not found. Falling back to global '{nameof(PrefabDefinition)}' search.");
            loggedFallbackSearch = true;
        }

        foreach (var guid in AssetDatabase.FindAssets($"t:{nameof(PrefabDefinition)}"))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var definition = AssetDatabase.LoadAssetAtPath<PrefabDefinition>(path);
            if (definition != null)
                yield return definition;
        }

        yield break;
#else
        // RUNTIME PATH
        var loaded = Resources.LoadAll<PrefabDefinition>(ResourcesFolder);
        foreach (var definition in loaded.Where(definition => definition != null))
            yield return definition;
#endif
    }

#if UNITY_EDITOR
    private static string[] GetSearchFolders()
    {
        var folders = new List<string>(2);
        if (AssetDatabase.IsValidFolder(PrefabRegistryAssetFolder))
            folders.Add(PrefabRegistryAssetFolder);
        if (AssetDatabase.IsValidFolder(LegacyPrefabRegistryAssetFolder))
            folders.Add(LegacyPrefabRegistryAssetFolder);
        return folders.ToArray();
    }
#endif
}