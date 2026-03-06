using UnityEngine;
using System.Collections.Generic;

public class StateMachineRegistry : DefinitionRegistry<StateMachineDefinition>
{
    public static StateMachineRegistry Instance { get; private set; }

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Multiple StateMachineRegistry instances detected.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        base.Awake();
    }

    protected override void ValidateDefinitions(List<StateMachineDefinition> defs, System.Action<string> reportError)
    {
        for (int i = 0; i < defs.Count; i++)
        {
            var machine = defs[i];
            if (machine == null)
                continue;

            var seen = new HashSet<string>(System.StringComparer.Ordinal);
            for (int s = 0; s < machine.States.Count; s++)
            {
                var state = machine.States[s];
                if (string.IsNullOrWhiteSpace(state.stateId))
                    continue;

                if (!seen.Add(state.stateId))
                    reportError($"[Validation] StateMachine '{machine.Id}' has duplicate state id '{state.stateId}'.");

                if (string.IsNullOrWhiteSpace(state.actionId))
                {
                    reportError($"[Validation] StateMachine '{machine.Id}' state '{state.stateId}' is missing actionId.");
                    continue;
                }

                if (TaskRegistry.Instance == null || !TaskRegistry.Instance.TryGet(state.actionId, out _))
                    reportError($"[Validation] StateMachine '{machine.Id}' state '{state.stateId}' references missing TaskDefinition '{state.actionId}'.");
            }
        }
    }

    protected override IEnumerable<string> GetValidationDependencyErrors()
    {
        if (TaskRegistry.Instance == null)
            yield return "Missing dependency: TaskRegistry.Instance is null.";
    }
}
