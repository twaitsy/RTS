using System;
using System.Reflection;
using UnityEngine;

public static class DefinitionValidationOrchestrator
{
    public static DefinitionValidationReport RunValidation()
    {
        var report = new DefinitionValidationReport();
        var bridgeType = Type.GetType("DefinitionValidationEditorBridge, Validation.Editor");
        var runMethod = bridgeType?.GetMethod("Run", BindingFlags.Public | BindingFlags.Static);
        runMethod?.Invoke(null, new object[] { report });
        return report;
    }

    public static DefinitionValidationReport RunValidationAndLog(bool quiet = false)
    {
        var report = RunValidation();
        if (!quiet)
            Debug.Log(report.BuildSummary());
        return report;
    }
}
