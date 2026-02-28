using System;
using System.Collections.Generic;
using UnityEngine;

public static partial class DefinitionValidationOrchestrator
{
    public static DefinitionValidationReport RunValidation()
    {
        var report = new DefinitionValidationReport();
        var referenceMap = new DefinitionReferenceMap();
        report.ReferenceMap = referenceMap;

        ExecuteRegistryValidators(report, referenceMap);
        ExecuteEditorValidators(report);

        return report;
    }

    public static DefinitionValidationReport RunValidationAndLog()
    {
        var report = RunValidation();
        var summary = report.BuildSummary();

        if (report.HasErrors)
            Debug.LogError(summary);
        else
            Debug.Log(summary);

        return report;
    }

    private static void ExecuteRegistryValidators(DefinitionValidationReport report, DefinitionReferenceMap referenceMap)
    {
        var validators = new List<IDefinitionRegistryValidator>();
        var registries = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var registry in registries)
        {
            if (registry is IDefinitionRegistryValidator validator)
                validators.Add(validator);
        }

        validators.Sort((left, right) => string.Compare(left.RegistryName, right.RegistryName, StringComparison.Ordinal));

        foreach (var validator in validators)
        {
            validator.CollectReferenceMap(referenceMap);
            validator.ValidateAll(report);
        }

    }

    static partial void ExecuteEditorValidators(DefinitionValidationReport report);
}
