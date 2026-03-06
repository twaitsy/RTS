using System;
using UnityEngine;

public static class DefinitionValidationOrchestrator
{
    private const string BridgeRegistry = nameof(DefinitionValidationOrchestrator);
    private const string BridgeMissingCode = "VALIDATION_EDITOR_BRIDGE_MISSING";

    public static DefinitionValidationReport RunValidation(bool failOnBridgeMissing = false)
    {
        var report = new DefinitionValidationReport();
        var bridgeRan = TryRunEditorBridge(report);

        if (failOnBridgeMissing && !bridgeRan)
        {
            throw new InvalidOperationException(
                $"{nameof(DefinitionValidationMenu)}.{nameof(DefinitionValidationMenu.ValidateAllForCI)} requires {nameof(DefinitionValidationEditorBridge)} to be available.");
        }

        return report;
    }

    public static DefinitionValidationReport RunValidationAndLog(bool quiet = false, bool failOnBridgeMissing = false)
    {
        var report = RunValidation(failOnBridgeMissing);
        if (!quiet)
            Debug.Log(report.BuildSummary());
        return report;
    }

    private static bool TryRunEditorBridge(DefinitionValidationReport report)
    {
#if UNITY_EDITOR
        DefinitionValidationEditorBridge.Run(report);
        return true;
#else
        RegisterMissingBridgeIssue(report, $"{nameof(DefinitionValidationEditorBridge)} is only available in UNITY_EDITOR builds.");
        return false;
#endif
    }

    private static void RegisterMissingBridgeIssue(DefinitionValidationReport report, string message)
    {
        report.AddIssue(new ValidationIssue(
            code: BridgeMissingCode,
            severity: ValidationIssueSeverity.Error,
            registry: BridgeRegistry,
            message: message,
            suggestedFix: "Ensure editor validation scripts compile and are included in the active editor assembly."));
        Debug.LogError($"[Validation] ({BridgeMissingCode}) {message}");
    }
}
