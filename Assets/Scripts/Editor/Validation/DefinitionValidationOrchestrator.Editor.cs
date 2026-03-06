#if UNITY_EDITOR
using System;
using System.Collections.Generic;

public static class DefinitionValidationEditorBridge
{
    public static void Run(DefinitionValidationReport report)
    {
        if (report == null)
            return;

        var orderedEditorValidators = new List<Action<DefinitionValidationReport>>
        {
            StatIdValidationMenu.AppendCanonicalStatIdIssues,
            DefinitionValidationMenu.AppendStatModifierLinkIssues,
            PrefabRegistryEditorValidator.AppendValidationIssues,
            ValidationAutoRepairEngine.AppendRepairableIssues
        };

        foreach (var validator in orderedEditorValidators)
            validator(report);
    }
}
#endif
