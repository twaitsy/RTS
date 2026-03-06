#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public static class DefinitionValidationEditorBridge
{
    public static void Run(DefinitionValidationReport report)
    {
        if (report == null)
            return;

        var referenceMap = new DefinitionReferenceMap();
        report.ReferenceMap = referenceMap;

        ExecuteRegistryValidators(report, referenceMap);

        var orderedEditorValidators = new List<Action<DefinitionValidationReport>>
        {
            StatIdValidationMenu.AppendCanonicalStatIdIssues,
            DefinitionValidationMenu.AppendStatModifierLinkIssues,
            ValidationAutoRepairEngine.AppendRepairableIssues
        };

        foreach (var validator in orderedEditorValidators)
            validator(report);
    }

    private static void ExecuteRegistryValidators(DefinitionValidationReport report, DefinitionReferenceMap referenceMap)
    {
        var adapters = RuntimeRegistryValidationAdapterFactory.CreateAdapters();
        var singletonSnapshot = SeedRegistrySingletonInstances(adapters.Select(adapter => adapter.Registry));

        try
        {
            foreach (var adapter in adapters.OrderBy(adapter => adapter.RegistryName, StringComparer.Ordinal))
            {
                adapter.CollectReferenceMap(referenceMap);
                adapter.ValidateAll(report);
            }

            PrefabRegistry.AppendValidationIssues(report);
        }
        finally
        {
            singletonSnapshot.Restore();
        }
    }

    private static SingletonInstanceSnapshot SeedRegistrySingletonInstances(IEnumerable<MonoBehaviour> registries)
    {
        var snapshot = new SingletonInstanceSnapshot();

        foreach (var registry in registries)
        {
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
                snapshot.Add(registryType, instanceProperty, instanceProperty.GetValue(null));

            if (instanceProperty.GetValue(null) == null)
                setter.Invoke(null, new object[] { registry });
        }

        return snapshot;
    }

    private sealed class SingletonInstanceSnapshot
    {
        private readonly List<SnapshotEntry> entries = new();

        public bool HasEntryFor(Type registryType) => entries.Any(entry => entry.RegistryType == registryType);

        public void Add(Type registryType, PropertyInfo instanceProperty, object originalValue)
        {
            entries.Add(new SnapshotEntry(registryType, instanceProperty, originalValue));
        }

        public void Restore()
        {
            foreach (var entry in entries)
            {
                var setter = entry.InstanceProperty.GetSetMethod(true);
                setter?.Invoke(null, new[] { entry.OriginalValue });
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
}

internal sealed class RuntimeRegistryValidationAdapter
{
    private readonly IDefinitionRegistryValidator validator;

    public RuntimeRegistryValidationAdapter(IDefinitionRegistryValidator validator)
    {
        this.validator = validator;
    }

    public string RegistryName => validator.RegistryName;
    public MonoBehaviour Registry => validator as MonoBehaviour;

    public void ValidateAll(DefinitionValidationReport report) => validator.ValidateAll(report);
    public void CollectReferenceMap(DefinitionReferenceMap map) => validator.CollectReferenceMap(map);
}

internal static class RuntimeRegistryValidationAdapterFactory
{
    public static List<RuntimeRegistryValidationAdapter> CreateAdapters()
    {
        var adapters = new List<RuntimeRegistryValidationAdapter>();
        var registries = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var registry in registries)
        {
            if (registry is IDefinitionRegistryValidator validator)
                adapters.Add(new RuntimeRegistryValidationAdapter(validator));
        }

        return adapters;
    }
}
#endif
