#if UNITY_EDITOR
using System;
using System.Collections.Generic;

public static class DefinitionValidationEditorBridge
{
    public static void Run(DefinitionValidationReport report)
    {
        var orderedEditorValidators = new List<Action<DefinitionValidationReport>>
        {
            StatIdValidationMenu.AppendCanonicalStatIdIssues,
            DefinitionValidationMenu.AppendStatModifierLinkIssues,
            ValidationAutoRepairEngine.AppendRepairableIssues
        };

        foreach (var validator in orderedEditorValidators)
            validator(report);
    }
}
#endif
