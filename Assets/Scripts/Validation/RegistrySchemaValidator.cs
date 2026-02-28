using System;
using System.Collections.Generic;

public static class RegistrySchemaValidator
{
    public static void Validate<TDefinition>(
        IEnumerable<TDefinition> definitions,
        IRegistrySchema<TDefinition> schema,
        Func<TDefinition, string> hostAssetName,
        Func<TDefinition, string> hostId,
        Action<string> reportError)
    {
        if (schema == null)
            return;

        foreach (var definition in definitions)
        {
            if (definition == null)
                continue;

            ValidateFields(definition, schema.FieldRules, hostAssetName, hostId, reportError);
            ValidateReferences(definition, schema.ReferenceRules, hostAssetName, hostId, reportError);
            ValidateConstraints(definition, schema.ConstraintRules, hostAssetName, hostId, reportError);
        }
    }

    private static void ValidateFields<TDefinition>(
        TDefinition definition,
        IReadOnlyList<FieldRule<TDefinition>> rules,
        Func<TDefinition, string> hostAssetName,
        Func<TDefinition, string> hostId,
        Action<string> reportError)
    {
        foreach (var rule in rules)
        {
            if (!rule.IsRequired)
                continue;

            if (!rule.HasValue(definition))
                reportError($"[Validation] Asset '{hostAssetName(definition)}' (id: '{hostId(definition)}') field '{rule.FieldName}' is required.");
        }
    }

    private static void ValidateReferences<TDefinition>(
        TDefinition definition,
        IReadOnlyList<ReferenceRule<TDefinition>> rules,
        Func<TDefinition, string> hostAssetName,
        Func<TDefinition, string> hostId,
        Action<string> reportError)
    {
        foreach (var rule in rules)
        {
            var ids = rule.GetReferenceIds(definition);
            var hasAnyReference = false;

            if (ids != null)
            {
                foreach (var id in ids)
                {
                    if (string.IsNullOrWhiteSpace(id))
                        continue;

                    hasAnyReference = true;

                    var matchedTargets = 0;
                    foreach (var target in rule.AllowedTargets)
                    {
                        if (target.TargetExists != null && target.TargetExists(id))
                            matchedTargets++;
                    }

                    if (matchedTargets == 0)
                    {
                        reportError($"[Validation] Asset '{hostAssetName(definition)}' (id: '{hostId(definition)}') field '{rule.FieldName}' references missing target id '{id}'.");
                        continue;
                    }

                    if (rule.RequireSingleTargetType && matchedTargets > 1)
                    {
                        reportError($"[Validation] Asset '{hostAssetName(definition)}' (id: '{hostId(definition)}') field '{rule.FieldName}' id '{id}' resolved to multiple target types.");
                    }
                }
            }

            if (rule.IsRequired && !hasAnyReference)
                reportError($"[Validation] Asset '{hostAssetName(definition)}' (id: '{hostId(definition)}') field '{rule.FieldName}' requires at least one reference.");
        }
    }

    private static void ValidateConstraints<TDefinition>(
        TDefinition definition,
        IReadOnlyList<ConstraintRule<TDefinition>> rules,
        Func<TDefinition, string> hostAssetName,
        Func<TDefinition, string> hostId,
        Action<string> reportError)
    {
        foreach (var rule in rules)
        {
            if (rule.Validate == null)
                continue;

            var errors = rule.Validate(definition);
            if (errors == null)
                continue;

            foreach (var error in errors)
            {
                if (string.IsNullOrWhiteSpace(error))
                    continue;

                reportError($"[Validation] Asset '{hostAssetName(definition)}' (id: '{hostId(definition)}') constraint '{rule.Name}' failed: {error}");
            }
        }
    }
}
