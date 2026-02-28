#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class RegistrySyncUtility
{
    private const string AutoSyncMenuPath = "Tools/Data/Auto Sync Registries";
    private const string ManualSyncMenuPath = "Tools/Data/Sync Registries";
    private const string SnapshotSessionKey = "RegistrySyncUtility.AssetSnapshot";
    private const string PendingSessionKey = "RegistrySyncUtility.Pending";
    private static bool syncQueued;

    static RegistrySyncUtility()
    {
        EditorApplication.delayCall += SyncOnLoad;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    [MenuItem(ManualSyncMenuPath)]
    public static void SyncRegistriesFromMenu()
    {
        SyncRegistries("manual menu");
    }

    [MenuItem(AutoSyncMenuPath)]
    private static void ToggleAutoSync()
    {
        RegistrySyncProjectSettings.AutoSyncEnabled = !RegistrySyncProjectSettings.AutoSyncEnabled;
        Menu.SetChecked(AutoSyncMenuPath, RegistrySyncProjectSettings.AutoSyncEnabled);

        if (RegistrySyncProjectSettings.AutoSyncEnabled)
            RequestSync("auto-sync toggled on");
    }

    [MenuItem(AutoSyncMenuPath, true)]
    private static bool ToggleAutoSyncValidate()
    {
        Menu.SetChecked(AutoSyncMenuPath, RegistrySyncProjectSettings.AutoSyncEnabled);
        return true;
    }

    public static void RequestSync(string reason)
    {
        if (!RegistrySyncProjectSettings.AutoSyncEnabled)
            return;

        RequestSyncInternal(reason);
    }

    public static void RequestSyncInternal(string reason)
    {
        SessionState.SetBool(PendingSessionKey, true);

        if (syncQueued)
            return;

        syncQueued = true;
        EditorApplication.delayCall += () =>
        {
            syncQueued = false;
            SyncRegistries(reason);
        };
    }

    private static void SyncOnLoad()
    {
        if (SessionState.GetBool(PendingSessionKey, false) || RegistrySyncProjectSettings.AutoSyncEnabled)
            SyncRegistries("domain reload");
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
            SyncRegistries("before entering play mode", force: true);
    }

    public static bool SyncRegistries(string reason, bool force = false)
    {
        if (!force && !RegistrySyncProjectSettings.AutoSyncEnabled && reason != "manual menu")
            return false;

        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            RequestSyncInternal("editor busy");
            return false;
        }

        SessionState.SetBool(PendingSessionKey, false);

        var registries = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Where(IsDefinitionRegistry)
            .ToList();

        var tracker = new RegistryChangeTracker(LoadSnapshot());
        var totalUpdates = 0;

        foreach (var registry in registries)
        {
            if (!TryGetDefinitionType(registry.GetType(), out var definitionType))
                continue;

            var entries = DiscoverDefinitions(definitionType, tracker);
            if (ApplyToRegistry(registry, entries))
                totalUpdates++;
        }

        SaveSnapshot(tracker.CurrentSnapshot);
        tracker.LogChanges();

        if (totalUpdates > 0)
            Debug.Log($"[RegistrySync] Updated {totalUpdates} registry component(s) ({reason}).");

        return totalUpdates > 0;
    }

    private static bool ApplyToRegistry(MonoBehaviour registry, List<DefinitionEntry> entries)
    {
        var serializedObject = new SerializedObject(registry);
        var definitionsProperty = serializedObject.FindProperty("definitions");

        if (definitionsProperty == null || !definitionsProperty.isArray)
            return false;

        var current = new List<UnityEngine.Object>(definitionsProperty.arraySize);
        for (var i = 0; i < definitionsProperty.arraySize; i++)
            current.Add(definitionsProperty.GetArrayElementAtIndex(i).objectReferenceValue);

        var next = entries.Select(entry => (UnityEngine.Object)entry.Asset).ToList();
        var changed = current.Count != next.Count || current.Where((t, i) => t != next[i]).Any();

        if (!changed)
            return false;

        LogDrift(registry, current, entries);

        definitionsProperty.arraySize = next.Count;
        for (var i = 0; i < next.Count; i++)
            definitionsProperty.GetArrayElementAtIndex(i).objectReferenceValue = next[i];

        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(registry);

        if (registry.gameObject.scene.IsValid())
            EditorSceneManager.MarkSceneDirty(registry.gameObject.scene);

        return true;
    }

    private static void LogDrift(MonoBehaviour registry, List<UnityEngine.Object> current, List<DefinitionEntry> expected)
    {
        var currentNames = current.Where(obj => obj != null).Select(AssetDatabase.GetAssetPath).OrderBy(path => path, StringComparer.Ordinal).ToList();
        var expectedNames = expected.Select(e => e.Path).OrderBy(path => path, StringComparer.Ordinal).ToList();

        Debug.LogWarning($"[RegistrySync] Drift detected in {registry.GetType().Name}. Scene list count={current.Count}, asset database count={expected.Count}.");

        var missing = expectedNames.Except(currentNames).ToList();
        var extra = currentNames.Except(expectedNames).ToList();

        if (missing.Count > 0)
            Debug.LogWarning($"[RegistrySync] Missing from scene registry {registry.GetType().Name}: {string.Join(", ", missing)}");

        if (extra.Count > 0)
            Debug.LogWarning($"[RegistrySync] Extra in scene registry {registry.GetType().Name}: {string.Join(", ", extra)}");
    }

    private static List<DefinitionEntry> DiscoverDefinitions(Type definitionType, RegistryChangeTracker tracker)
    {
        var guids = AssetDatabase.FindAssets($"t:{definitionType.Name}");
        var entries = new List<DefinitionEntry>(guids.Length);

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrWhiteSpace(path))
                continue;

            var asset = AssetDatabase.LoadAssetAtPath(path, definitionType) as ScriptableObject;
            if (!(asset is IIdentifiable identifiable))
                continue;

            entries.Add(new DefinitionEntry
            {
                Guid = guid,
                Id = identifiable.Id ?? string.Empty,
                Path = path,
                Asset = asset
            });
        }

        entries.Sort((a, b) =>
        {
            var idComparison = string.Compare(a.Id, b.Id, StringComparison.Ordinal);
            if (idComparison != 0)
                return idComparison;

            return string.Compare(a.Path, b.Path, StringComparison.Ordinal);
        });

        tracker.Track(definitionType.FullName, entries);
        return entries;
    }

    private static bool IsDefinitionRegistry(MonoBehaviour behaviour)
    {
        return behaviour != null && TryGetDefinitionType(behaviour.GetType(), out _);
    }

    private static bool TryGetDefinitionType(Type registryType, out Type definitionType)
    {
        definitionType = null;

        while (registryType != null)
        {
            if (registryType.IsGenericType && registryType.GetGenericTypeDefinition() == typeof(DefinitionRegistry<>))
            {
                definitionType = registryType.GetGenericArguments()[0];
                return true;
            }

            registryType = registryType.BaseType;
        }

        return false;
    }

    private static RegistrySnapshot LoadSnapshot()
    {
        var json = SessionState.GetString(SnapshotSessionKey, string.Empty);
        if (string.IsNullOrWhiteSpace(json))
            return new RegistrySnapshot();

        return JsonUtility.FromJson<RegistrySnapshot>(json) ?? new RegistrySnapshot();
    }

    private static void SaveSnapshot(RegistrySnapshot snapshot)
    {
        SessionState.SetString(SnapshotSessionKey, JsonUtility.ToJson(snapshot));
    }

    [Serializable]
    private class RegistrySnapshot
    {
        public List<RegistryTypeSnapshot> Types = new();
    }

    [Serializable]
    private class RegistryTypeSnapshot
    {
        public string TypeName;
        public List<DefinitionSnapshot> Definitions = new();
    }

    [Serializable]
    private class DefinitionSnapshot
    {
        public string Guid;
        public string Path;
        public string Id;
    }

    private class RegistryChangeTracker
    {
        private readonly Dictionary<string, Dictionary<string, DefinitionSnapshot>> previous;
        private readonly List<string> logs = new();
        public RegistrySnapshot CurrentSnapshot { get; } = new();

        public RegistryChangeTracker(RegistrySnapshot previousSnapshot)
        {
            previous = previousSnapshot.Types.ToDictionary(
                item => item.TypeName,
                item => item.Definitions.ToDictionary(def => def.Guid, def => def));
        }

        public void Track(string typeName, List<DefinitionEntry> entries)
        {
            var currentType = new RegistryTypeSnapshot { TypeName = typeName };
            foreach (var entry in entries)
            {
                currentType.Definitions.Add(new DefinitionSnapshot
                {
                    Guid = entry.Guid,
                    Path = entry.Path,
                    Id = entry.Id
                });
            }

            CurrentSnapshot.Types.Add(currentType);

            previous.TryGetValue(typeName, out var oldByGuid);
            oldByGuid ??= new Dictionary<string, DefinitionSnapshot>();
            var currentByGuid = currentType.Definitions.ToDictionary(item => item.Guid, item => item);

            foreach (var added in currentByGuid.Values.Where(item => !oldByGuid.ContainsKey(item.Guid)))
                logs.Add($"[RegistrySync] Added {typeName}: {added.Id} ({added.Path})");

            foreach (var removed in oldByGuid.Values.Where(item => !currentByGuid.ContainsKey(item.Guid)))
                logs.Add($"[RegistrySync] Removed {typeName}: {removed.Id} ({removed.Path})");

            foreach (var pair in currentByGuid)
            {
                if (!oldByGuid.TryGetValue(pair.Key, out var oldEntry))
                    continue;

                if (!string.Equals(oldEntry.Path, pair.Value.Path, StringComparison.Ordinal))
                    logs.Add($"[RegistrySync] Renamed/Moved {typeName}: {oldEntry.Path} -> {pair.Value.Path}");
                else if (!string.Equals(oldEntry.Id, pair.Value.Id, StringComparison.Ordinal))
                    logs.Add($"[RegistrySync] Id changed {typeName}: {oldEntry.Id} -> {pair.Value.Id} ({pair.Value.Path})");
            }
        }

        public void LogChanges()
        {
            foreach (var log in logs)
                Debug.Log(log);
        }
    }

    private struct DefinitionEntry
    {
        public string Guid;
        public string Id;
        public string Path;
        public ScriptableObject Asset;
    }
}

internal static class RegistrySyncProjectSettings
{
    private const string AutoSyncSuffix = "RegistrySync.AutoSync";

    private static string ProjectKey => $"{Application.productName}.{Application.dataPath}.{AutoSyncSuffix}";

    public static bool AutoSyncEnabled
    {
        get => EditorPrefs.GetBool(ProjectKey, true);
        set => EditorPrefs.SetBool(ProjectKey, value);
    }
}

public sealed class RegistrySyncAssetPostprocessor : AssetPostprocessor
{
    private static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        if (importedAssets.Length == 0 && deletedAssets.Length == 0 && movedAssets.Length == 0 && movedFromAssetPaths.Length == 0)
            return;

        RegistrySyncUtility.RequestSync("asset import");
    }
}
#endif
