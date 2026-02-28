#if UNITY_EDITOR
using System.Collections.Generic;

public static partial class DefinitionValidationOrchestrator
{
    static partial void ExecuteEditorValidators(DefinitionValidationReport report)
    {
        var orderedEditorValidators = new List<System.Action<DefinitionValidationReport>>
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
