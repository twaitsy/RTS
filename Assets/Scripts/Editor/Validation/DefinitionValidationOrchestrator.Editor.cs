#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class DefinitionValidationEditorBridge
{
    private const string BridgeRegistry = nameof(DefinitionValidationEditorBridge);

    public static void Run(DefinitionValidationReport report)
    {
        if (report == null)
            return;

        var passOrder = new List<Action<DefinitionValidationReport>>
        {
            BuildAndAttachReferenceMap,
            AppendSchemaAndReferenceIssues,
            StatIdValidationMenu.AppendCanonicalStatIdIssues,
            DefinitionValidationMenu.AppendStatModifierLinkIssues,
            PrefabRegistryEditorValidator.AppendValidationIssues,
            ValidationAutoRepairEngine.AppendRepairableIssues
        };

        foreach (var pass in passOrder)
            pass(report);
    }

    private static void BuildAndAttachReferenceMap(DefinitionValidationReport report)
    {
        var map = new DefinitionReferenceMap();
        var definitions = LoadAllIdentifiableAssets();

        foreach (var definition in definitions.OrderBy(x => x.DefinitionType.Name, StringComparer.Ordinal).ThenBy(x => x.Id, StringComparer.Ordinal))
            map.AddDefinition(definition.DefinitionType.Name, definition.Id);

        foreach (var schemaContext in LoadRegistrySchemas())
        {
            foreach (var definition in definitions.Where(x => x.DefinitionClrType == schemaContext.DefinitionType))
            {
                foreach (var reference in schemaContext.EnumerateReferences(definition.Asset))
                    map.AddReference(schemaContext.DefinitionType.Name, definition.Id, reference.Field, reference.TargetType, reference.TargetId);
            }
        }

        report.ReferenceMap = map;
    }

    private static void AppendSchemaAndReferenceIssues(DefinitionValidationReport report)
    {
        var definitions = LoadAllIdentifiableAssets();
        var definitionsByType = definitions
            .GroupBy(x => x.DefinitionClrType)
            .ToDictionary(group => group.Key, group => group.OrderBy(x => x.Id, StringComparer.Ordinal).ToList());

        foreach (var schemaContext in LoadRegistrySchemas())
        {
            if (!definitionsByType.TryGetValue(schemaContext.DefinitionType, out var typedDefinitions))
                continue;

            foreach (var definition in typedDefinitions)
                AppendSchemaIssuesForDefinition(report, schemaContext, definition.Asset, definition.AssetPath, definition.Id);
        }

        AppendOrphanReferenceIssues(report, definitions);
    }

    private static void AppendOrphanReferenceIssues(DefinitionValidationReport report, IReadOnlyList<IdentifiableAssetRecord> definitions)
    {
        var map = report.ReferenceMap;
        if (map == null || !map.TryFindOrphans(out var orphans))
            return;

        var pathLookup = definitions.ToDictionary(
            item => $"{item.DefinitionType.Name}:{item.Id}",
            item => item.AssetPath,
            StringComparer.Ordinal);

        foreach (var orphan in orphans
                     .OrderBy(x => x.SourceType, StringComparer.Ordinal)
                     .ThenBy(x => x.SourceId, StringComparer.Ordinal)
                     .ThenBy(x => x.Field, StringComparer.Ordinal)
                     .ThenBy(x => x.TargetType, StringComparer.Ordinal)
                     .ThenBy(x => x.TargetId, StringComparer.Ordinal))
        {
            var sourceKey = $"{orphan.SourceType}:{orphan.SourceId}";
            pathLookup.TryGetValue(sourceKey, out var sourcePath);

            report.AddIssue(new ValidationIssue(
                code: "DEFINITION_REFERENCE_MISSING_TARGET",
                severity: ValidationIssueSeverity.Error,
                registry: BridgeRegistry,
                message: $"[Validation] Definition '{orphan.SourceType}:{orphan.SourceId}' field '{orphan.Field}' references missing target '{orphan.TargetType}:{orphan.TargetId}'.",
                assetPath: sourcePath,
                assetId: orphan.SourceId,
                field: orphan.Field,
                suggestedFix: "Update the reference value or add the missing target definition."));
        }
    }

    private static void AppendSchemaIssuesForDefinition(
        DefinitionValidationReport report,
        RegistrySchemaContext schema,
        ScriptableObject definition,
        string assetPath,
        string assetId)
    {
        var registryName = schema.RegistryType.Name;

        foreach (var field in schema.RequiredFields)
        {
            if (field.HasValue(definition))
                continue;

            report.AddIssue(new ValidationIssue(
                code: "SCHEMA_REQUIRED_FIELD_MISSING",
                severity: ValidationIssueSeverity.Error,
                registry: registryName,
                message: $"[Validation] Asset '{definition.name}' (id: '{assetId}') field '{field.FieldName}' is required.",
                assetPath: assetPath,
                assetId: assetId,
                field: field.FieldName,
                suggestedFix: "Populate the required field value."));
        }

        foreach (var rule in schema.ReferenceRules)
        {
            var references = rule.GetReferenceIds(definition);
            var sawReference = false;

            if (references != null)
            {
                foreach (var referenceId in references)
                {
                    if (string.IsNullOrWhiteSpace(referenceId))
                        continue;

                    sawReference = true;
                    var matchedTargets = 0;

                    foreach (var target in rule.AllowedTargets)
                    {
                        try
                        {
                            if (target.TargetExists?.Invoke(referenceId) == true)
                                matchedTargets++;
                        }
                        catch (Exception ex)
                        {
                            report.AddIssue(new ValidationIssue(
                                code: "SCHEMA_REFERENCE_TARGET_EXCEPTION",
                                severity: ValidationIssueSeverity.Error,
                                registry: registryName,
                                message: $"[Validation] Asset '{definition.name}' (id: '{assetId}') field '{rule.FieldName}' target '{target.TargetName}' threw '{ex.GetType().Name}' for id '{referenceId}': {ex.Message}",
                                assetPath: assetPath,
                                assetId: assetId,
                                field: rule.FieldName,
                                suggestedFix: "Ensure dependent registries are initialized and target predicates are null-safe."));
                        }
                    }

                    if (matchedTargets == 0)
                    {
                        report.AddIssue(new ValidationIssue(
                            code: "SCHEMA_REFERENCE_MISSING_TARGET",
                            severity: ValidationIssueSeverity.Error,
                            registry: registryName,
                            message: $"[Validation] Asset '{definition.name}' (id: '{assetId}') field '{rule.FieldName}' references missing target id '{referenceId}'.",
                            assetPath: assetPath,
                            assetId: assetId,
                            field: rule.FieldName,
                            suggestedFix: "Update the reference id or create the missing target definition."));
                    }
                    else if (rule.RequireSingleTargetType && matchedTargets > 1)
                    {
                        report.AddIssue(new ValidationIssue(
                            code: "SCHEMA_REFERENCE_AMBIGUOUS_TARGET",
                            severity: ValidationIssueSeverity.Error,
                            registry: registryName,
                            message: $"[Validation] Asset '{definition.name}' (id: '{assetId}') field '{rule.FieldName}' id '{referenceId}' resolved to multiple target types.",
                            assetPath: assetPath,
                            assetId: assetId,
                            field: rule.FieldName,
                            suggestedFix: "Disambiguate IDs so each reference resolves to one target type."));
                    }
                }
            }

            if (!rule.IsRequired || sawReference)
                continue;

            report.AddIssue(new ValidationIssue(
                code: "SCHEMA_REFERENCE_REQUIRED",
                severity: ValidationIssueSeverity.Error,
                registry: registryName,
                message: $"[Validation] Asset '{definition.name}' (id: '{assetId}') field '{rule.FieldName}' requires at least one reference.",
                assetPath: assetPath,
                assetId: assetId,
                field: rule.FieldName,
                suggestedFix: "Add at least one valid reference id."));
        }
        foreach (var rule in schema.ConstraintRules)
        {
            IEnumerable<string> errors;

            try
            {
                errors = rule.Validate(definition);
            }
            catch (Exception ex)
            {
                report.AddIssue(new ValidationIssue(
                    code: "SCHEMA_CONSTRAINT_EXCEPTION",
                    severity: ValidationIssueSeverity.Error,
                    registry: registryName,
                    message: $"[Validation] Asset '{definition.name}' (id: '{assetId}') constraint '{rule.Name}' threw '{ex.GetType().Name}': {ex.Message}",
                    assetPath: assetPath,
                    assetId: assetId,
                    field: rule.Name,
                    suggestedFix: "Make constraint validators null-safe and deterministic."));
                continue;
            }

            if (errors == null)
                continue;

            foreach (var error in errors.Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                report.AddIssue(new ValidationIssue(
                    code: "SCHEMA_CONSTRAINT_FAILED",
                    severity: ValidationIssueSeverity.Error,
                    registry: registryName,
                    message: $"[Validation] Asset '{definition.name}' (id: '{assetId}') constraint '{rule.Name}' failed: {error}",
                    assetPath: assetPath,
                    assetId: assetId,
                    field: rule.Name,
                    suggestedFix: "Adjust definition data to satisfy the schema constraint."));
            }
        }
        }

    private static List<RegistrySchemaContext> LoadRegistrySchemas()
    {
        var contexts = new List<RegistrySchemaContext>();

        foreach (var registryType in TypeCache.GetTypesDerivedFrom(typeof(MonoBehaviour))
                     .Where(IsDefinitionRegistryType)
                     .OrderBy(type => type.Name, StringComparer.Ordinal))
        {
            var definitionType = GetDefinitionType(registryType);
            if (definitionType == null || !typeof(ScriptableObject).IsAssignableFrom(definitionType))
                continue;

            var schema = TryCreateSchemaContext(registryType, definitionType);
            if (schema != null)
                contexts.Add(schema);
        }

        return contexts;
    }

    private static RegistrySchemaContext TryCreateSchemaContext(Type registryType, Type definitionType)
    {
        var gameObject = new GameObject($"{registryType.Name}_ValidationProbe");
        gameObject.hideFlags = HideFlags.HideAndDontSave;

        try
        {
            var registry = gameObject.AddComponent(registryType);
            var getSchemaMethod = registryType.GetMethod("GetSchema", BindingFlags.Instance | BindingFlags.NonPublic);
            if (getSchemaMethod == null)
                return null;

            var schema = getSchemaMethod.Invoke(registry, null);
            if (schema == null)
                return null;

            return RegistrySchemaContext.Create(registryType, definitionType, schema);
        }
        catch
        {
            return null;
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(gameObject);
        }
    }

    private static IReadOnlyList<IdentifiableAssetRecord> LoadAllIdentifiableAssets()
    {
        var records = new List<IdentifiableAssetRecord>();

        foreach (var definitionType in TypeCache.GetTypesDerivedFrom<ScriptableObject>()
                     .Where(type => typeof(IIdentifiable).IsAssignableFrom(type) && !type.IsAbstract)
                     .OrderBy(type => type.Name, StringComparer.Ordinal))
        {
            var guids = AssetDatabase.FindAssets($"t:{definitionType.Name}");
            foreach (var guid in guids.OrderBy(x => x, StringComparer.Ordinal))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath(path, definitionType) as ScriptableObject;
                if (asset is not IIdentifiable identifiable)
                    continue;

                if (string.IsNullOrWhiteSpace(identifiable.Id))
                    continue;

                records.Add(new IdentifiableAssetRecord(asset, definitionType, identifiable.Id, path));
            }
        }

        return records;
    }

    private static bool IsDefinitionRegistryType(Type type)
    {
        if (type == null || type.IsAbstract)
            return false;

        var current = type;
        while (current != null)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(DefinitionRegistry<>))
                return true;
            current = current.BaseType;
        }

        return false;
    }

    private static Type GetDefinitionType(Type registryType)
    {
        var current = registryType;
        while (current != null)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(DefinitionRegistry<>))
                return current.GetGenericArguments()[0];
            current = current.BaseType;
        }

        return null;
    }

    private readonly struct IdentifiableAssetRecord
    {
        public IdentifiableAssetRecord(ScriptableObject asset, Type definitionType, string id, string assetPath)
        {
            Asset = asset;
            DefinitionType = definitionType;
            DefinitionClrType = definitionType;
            Id = id;
            AssetPath = assetPath;
        }

        public ScriptableObject Asset { get; }
        public Type DefinitionType { get; }
        public Type DefinitionClrType { get; }
        public string Id { get; }
        public string AssetPath { get; }
    }

    private sealed class RegistrySchemaContext
    {
        private RegistrySchemaContext(
            Type registryType,
            Type definitionType,
            IReadOnlyList<FieldRuleAdapter> requiredFields,
            IReadOnlyList<ReferenceRuleAdapter> referenceRules,
            IReadOnlyList<ConstraintRuleAdapter> constraintRules)
        {
            RegistryType = registryType;
            DefinitionType = definitionType;
            RequiredFields = requiredFields;
            ReferenceRules = referenceRules;
            ConstraintRules = constraintRules;
        }

        public Type RegistryType { get; }
        public Type DefinitionType { get; }
        public IReadOnlyList<FieldRuleAdapter> RequiredFields { get; }
        public IReadOnlyList<ReferenceRuleAdapter> ReferenceRules { get; }
        public IReadOnlyList<ConstraintRuleAdapter> ConstraintRules { get; }

        public static RegistrySchemaContext Create(Type registryType, Type definitionType, object schema)
        {
            var schemaType = schema.GetType();
            var fieldRules = (IEnumerable)schemaType.GetProperty("FieldRules")?.GetValue(schema);
            var referenceRules = (IEnumerable)schemaType.GetProperty("ReferenceRules")?.GetValue(schema);
            var constraintRules = (IEnumerable)schemaType.GetProperty("ConstraintRules")?.GetValue(schema);

            return new RegistrySchemaContext(
                registryType,
                definitionType,
                FieldRuleAdapter.CreateMany(definitionType, fieldRules).Where(x => x.IsRequired).ToList(),
                ReferenceRuleAdapter.CreateMany(definitionType, referenceRules).ToList(),
                ConstraintRuleAdapter.CreateMany(definitionType, constraintRules).ToList());
        }

        public IEnumerable<(string Field, string TargetType, string TargetId)> EnumerateReferences(ScriptableObject definition)
        {
            foreach (var referenceRule in ReferenceRules)
            {
                foreach (var referenceId in referenceRule.GetReferenceIds(definition))
                {
                    if (string.IsNullOrWhiteSpace(referenceId))
                        continue;

                    foreach (var target in referenceRule.AllowedTargets)
                    {
                        if (string.IsNullOrWhiteSpace(target.TargetName))
                            continue;

                        yield return (referenceRule.FieldName, target.TargetName, referenceId);
                    }
                }
            }
        }
    }

    private sealed class FieldRuleAdapter
    {
        private FieldRuleAdapter(string fieldName, bool isRequired, Func<ScriptableObject, bool> hasValue)
        {
            FieldName = fieldName;
            IsRequired = isRequired;
            HasValue = hasValue;
        }

        public string FieldName { get; }
        public bool IsRequired { get; }
        public Func<ScriptableObject, bool> HasValue { get; }

        public static IEnumerable<FieldRuleAdapter> CreateMany(Type definitionType, IEnumerable rules)
        {
            if (rules == null)
                yield break;

            foreach (var rule in rules)
            {
                if (rule == null)
                    continue;

                var ruleType = rule.GetType();
                var fieldName = ruleType.GetProperty("FieldName")?.GetValue(rule) as string;
                var isRequired = (bool?)ruleType.GetProperty("IsRequired")?.GetValue(rule) ?? false;
                var hasValueMethod = ruleType.GetMethod("HasValue");

                yield return new FieldRuleAdapter(
                    fieldName,
                    isRequired,
                    definition => (bool)(hasValueMethod?.Invoke(rule, new object[] { definition }) ?? true));
            }
        }
    }

    private sealed class ReferenceRuleAdapter
    {
        private readonly Func<ScriptableObject, IEnumerable<string>> getReferenceIds;

        private ReferenceRuleAdapter(
            string fieldName,
            bool isRequired,
            bool requireSingleTargetType,
            IReadOnlyList<ReferenceTargetRule> allowedTargets,
            Func<ScriptableObject, IEnumerable<string>> getReferenceIds)
        {
            FieldName = fieldName;
            IsRequired = isRequired;
            RequireSingleTargetType = requireSingleTargetType;
            AllowedTargets = allowedTargets;
            this.getReferenceIds = getReferenceIds;
        }

        public string FieldName { get; }
        public bool IsRequired { get; }
        public bool RequireSingleTargetType { get; }
        public IReadOnlyList<ReferenceTargetRule> AllowedTargets { get; }

        public IEnumerable<string> GetReferenceIds(ScriptableObject definition)
        {
            return getReferenceIds?.Invoke(definition) ?? Array.Empty<string>();
        }

        public static IEnumerable<ReferenceRuleAdapter> CreateMany(Type definitionType, IEnumerable rules)
        {
            if (rules == null)
                yield break;

            foreach (var rule in rules)
            {
                if (rule == null)
                    continue;

                var ruleType = rule.GetType();
                var fieldName = ruleType.GetProperty("FieldName")?.GetValue(rule) as string;
                var isRequired = (bool?)ruleType.GetProperty("IsRequired")?.GetValue(rule) ?? false;
                var requireSingleTargetType = (bool?)ruleType.GetProperty("RequireSingleTargetType")?.GetValue(rule) ?? true;
                var allowedTargets = ruleType.GetProperty("AllowedTargets")?.GetValue(rule) as IReadOnlyList<ReferenceTargetRule> ?? Array.Empty<ReferenceTargetRule>();
                var getRefMethod = ruleType.GetMethod("GetReferenceIds");

                yield return new ReferenceRuleAdapter(
                    fieldName,
                    isRequired,
                    requireSingleTargetType,
                    allowedTargets,
                    definition => (IEnumerable<string>)(getRefMethod?.Invoke(rule, new object[] { definition }) ?? Array.Empty<string>()));
            }
        }
    }

    private sealed class ConstraintRuleAdapter
    {
        private readonly Func<ScriptableObject, IEnumerable<string>> validate;

        private ConstraintRuleAdapter(string name, Func<ScriptableObject, IEnumerable<string>> validate)
        {
            Name = name;
            this.validate = validate;
        }

        public string Name { get; }

        public IEnumerable<string> Validate(ScriptableObject definition)
        {
            return validate?.Invoke(definition);
        }

        public static IEnumerable<ConstraintRuleAdapter> CreateMany(Type definitionType, IEnumerable rules)
        {
            if (rules == null)
                yield break;

            foreach (var rule in rules)
            {
                if (rule == null)
                    continue;

                var ruleType = rule.GetType();
                var name = ruleType.GetProperty("Name")?.GetValue(rule) as string;
                var validateMethod = ruleType.GetProperty("Validate")?.GetValue(rule) as Delegate;

                yield return new ConstraintRuleAdapter(
                    name,
                    definition => (IEnumerable<string>)validateMethod?.DynamicInvoke(definition));
            }
        }
    }
}
#endif
