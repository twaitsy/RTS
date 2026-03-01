using System;
using System.Collections.Generic;
using UnityEngine;

public interface IStateMachineConditionContext
{
    bool TryGetElapsedSecondsSinceEvent(string eventName, out float seconds);
}

public sealed class StateMachineRuntime
{
    private const string EventKey = "event";
    private const string ConditionIdKey = "conditionId";
    private const string MinElapsedKey = "minElapsed";

    private readonly Dictionary<string, BehaviourState> runtimeStateById = new();
    private readonly Dictionary<BehaviourState, string> runtimeIdByState = new();
    private readonly Dictionary<BehaviourState, BehaviourState> parentByState = new();
    private readonly Dictionary<string, List<RuntimeTransition>> transitionsByFromStateId = new();

    private BehaviourState initialRuntimeState;

    public bool Initialize(
        StateMachineDefinition machineDefinition,
        string machineDefinitionId,
        List<LegacyStateIdMapping> legacyStateMappings,
        BehaviourState legacyInitialState,
        BehaviourState obsoleteInitialState)
    {
        if (!TryResolveMachineDefinition(machineDefinition, machineDefinitionId, out StateMachineDefinition resolvedDefinition))
        {
            Debug.LogError("StateMachineRuntime could not resolve a StateMachineDefinition.");
            return false;
        }

        BuildStateLookups(resolvedDefinition, legacyStateMappings ?? new List<LegacyStateIdMapping>());
        BuildTransitionLookups(resolvedDefinition);

        initialRuntimeState = ResolveInitialState(resolvedDefinition, legacyInitialState, obsoleteInitialState);

        if (initialRuntimeState == null)
        {
            Debug.LogError("StateMachineRuntime could not resolve an initial state.");
            return false;
        }

        return true;
    }

    public BehaviourState GetInitialRuntimeState() => initialRuntimeState;

    public BehaviourState GetParentState(BehaviourState state)
    {
        if (state == null)
            return null;

        return parentByState.TryGetValue(state, out var parent) ? parent : null;
    }

    public List<BehaviourState> BuildAncestryChain(BehaviourState state)
    {
        var chain = new List<BehaviourState>();

        while (state != null)
        {
            chain.Add(state);
            state = GetParentState(state);
        }

        return chain;
    }

    public bool TryResolveTransition(
        BehaviourState fromState,
        string evt,
        IStateMachineConditionContext conditionContext,
        out BehaviourState target)
    {
        target = null;

        if (fromState == null || !runtimeIdByState.TryGetValue(fromState, out string fromStateId))
            return false;

        if (!transitionsByFromStateId.TryGetValue(fromStateId, out var candidates))
            return false;

        foreach (var transition in candidates)
        {
            if (!transition.Matches(evt, conditionContext))
                continue;

            if (!runtimeStateById.TryGetValue(transition.ToStateId, out target) || target == null)
            {
                Debug.LogError($"Transition target state '{transition.ToStateId}' could not be resolved.");
                return false;
            }

            return true;
        }

        return false;
    }

    private bool TryResolveMachineDefinition(
        StateMachineDefinition machineDefinition,
        string machineDefinitionId,
        out StateMachineDefinition resolved)
    {
        resolved = machineDefinition;

        if (resolved != null)
            return true;

        if (!string.IsNullOrWhiteSpace(machineDefinitionId) &&
            StateMachineRegistry.Instance != null &&
            StateMachineRegistry.Instance.TryGet(machineDefinitionId, out resolved) &&
            resolved != null)
        {
            return true;
        }

        return false;
    }

    private void BuildStateLookups(StateMachineDefinition machineDefinition, List<LegacyStateIdMapping> legacyStateMappings)
    {
        runtimeStateById.Clear();
        runtimeIdByState.Clear();
        parentByState.Clear();

        var mappedById = new Dictionary<string, BehaviourState>();

        foreach (var mapping in legacyStateMappings)
        {
            if (mapping.state == null || string.IsNullOrWhiteSpace(mapping.stateDefinitionId))
                continue;

            if (!mappedById.TryAdd(mapping.stateDefinitionId, mapping.state))
                Debug.LogWarning($"Duplicate legacy mapping for state ID '{mapping.stateDefinitionId}'.");
        }

        foreach (var entry in machineDefinition.States)
        {
            if (string.IsNullOrWhiteSpace(entry.stateId))
                continue;

            if (!mappedById.TryGetValue(entry.stateId, out BehaviourState runtimeState) || runtimeState == null)
            {
                Debug.LogWarning($"No BehaviourState mapped for definition state '{entry.stateId}'.");
                continue;
            }

            if (!runtimeStateById.TryAdd(entry.stateId, runtimeState))
                Debug.LogWarning($"Duplicate state ID '{entry.stateId}' in machine '{machineDefinition.Id}'.");

            if (!runtimeIdByState.TryAdd(runtimeState, entry.stateId))
                Debug.LogWarning($"BehaviourState '{runtimeState.name}' mapped to multiple state IDs.");
        }

        var stateDefinitionById = new Dictionary<string, StateDefinition>();

        if (StateRegistry.Instance != null)
        {
            foreach (var stateDefinition in StateRegistry.Instance.GetDefinitions())
            {
                if (stateDefinition == null || string.IsNullOrWhiteSpace(stateDefinition.Id))
                    continue;

                stateDefinitionById.TryAdd(stateDefinition.Id, stateDefinition);
            }
        }

        foreach (var pair in runtimeStateById)
        {
            var stateId = pair.Key;
            var runtimeState = pair.Value;

            if (!stateDefinitionById.TryGetValue(stateId, out StateDefinition stateDefinition) || stateDefinition == null)
                continue;

            var parentStateId = stateDefinition.ParentStateId;
            if (string.IsNullOrWhiteSpace(parentStateId))
                continue;

            if (!runtimeStateById.TryGetValue(parentStateId, out BehaviourState parentState) || parentState == null)
            {
                Debug.LogWarning($"State '{stateId}' has parent ID '{parentStateId}' but no runtime mapping exists for parent.");
                continue;
            }

            parentByState[runtimeState] = parentState;
        }

        foreach (var pair in runtimeStateById)
        {
            if (parentByState.ContainsKey(pair.Value))
                continue;

            parentByState[pair.Value] = pair.Value.Parent;
        }
    }

    private void BuildTransitionLookups(StateMachineDefinition machineDefinition)
    {
        transitionsByFromStateId.Clear();

        foreach (var entry in machineDefinition.Transitions)
        {
            if (string.IsNullOrWhiteSpace(entry.fromStateId) || string.IsNullOrWhiteSpace(entry.toStateId))
                continue;

            var runtimeTransition = RuntimeTransition.Parse(entry);

            if (!transitionsByFromStateId.TryGetValue(entry.fromStateId, out var list))
            {
                list = new List<RuntimeTransition>();
                transitionsByFromStateId[entry.fromStateId] = list;
            }

            list.Add(runtimeTransition);
        }
    }

    private BehaviourState ResolveInitialState(
        StateMachineDefinition machineDefinition,
        BehaviourState legacyInitialState,
        BehaviourState obsoleteInitialState)
    {
        if (!string.IsNullOrWhiteSpace(machineDefinition.InitialStateId) &&
            runtimeStateById.TryGetValue(machineDefinition.InitialStateId, out var explicitInitialState))
        {
            return explicitInitialState;
        }

        if (machineDefinition.States.Count > 0)
        {
            var firstStateId = machineDefinition.States[0].stateId;
            if (!string.IsNullOrWhiteSpace(firstStateId) && runtimeStateById.TryGetValue(firstStateId, out var initialFromDefinition))
                return initialFromDefinition;
        }

        if (legacyInitialState != null)
            return legacyInitialState;

        return obsoleteInitialState;
    }

    private sealed class RuntimeTransition
    {
        public string FromStateId { get; }
        public string ToStateId { get; }
        public string EventName { get; }
        public string ConditionId { get; }
        public float? MinElapsedSecondsSinceEvent { get; }

        private RuntimeTransition(string fromStateId, string toStateId, string eventName, string conditionId, float? minElapsedSecondsSinceEvent)
        {
            FromStateId = fromStateId;
            ToStateId = toStateId;
            EventName = eventName;
            ConditionId = conditionId;
            MinElapsedSecondsSinceEvent = minElapsedSecondsSinceEvent;
        }

        public bool Matches(string evt, IStateMachineConditionContext conditionContext)
        {
            if (!string.Equals(EventName, evt, StringComparison.Ordinal))
                return false;

            if (!string.IsNullOrWhiteSpace(ConditionId))
            {
                if (ConditionRegistry.Instance == null || !ConditionRegistry.Instance.TryGet(ConditionId, out ConditionDefinition conditionDef) || conditionDef == null)
                    return false;

                if (!conditionDef.Evaluate(new RuntimeConditionContext(conditionContext, evt)))
                    return false;
            }

            if (MinElapsedSecondsSinceEvent.HasValue)
            {
                if (conditionContext == null || !conditionContext.TryGetElapsedSecondsSinceEvent(evt, out float elapsed))
                    return false;

                if (elapsed < MinElapsedSecondsSinceEvent.Value)
                    return false;
            }

            return true;
        }

        public static RuntimeTransition Parse(StateTransitionEntry entry)
        {
            string evt = entry.eventName;
            string conditionId = entry.conditionId;
            float? minElapsed = null;
            string conditionDescription = entry.conditionDescription;

            if (string.IsNullOrWhiteSpace(evt) && !string.IsNullOrWhiteSpace(conditionDescription) && !conditionDescription.Contains(';'))
                evt = conditionDescription;

            if (!string.IsNullOrWhiteSpace(conditionDescription) && conditionDescription.Contains(';'))
            {
                var tokens = conditionDescription.Split(';');

                foreach (var token in tokens)
                {
                    var trimmedToken = token.Trim();
                    if (string.IsNullOrWhiteSpace(trimmedToken))
                        continue;

                    var split = trimmedToken.Split('=', 2);
                    if (split.Length != 2)
                        continue;

                    var key = split[0].Trim();
                    var value = split[1].Trim();

                    if (key.Equals(EventKey, StringComparison.OrdinalIgnoreCase))
                        evt = value;
                    else if (key.Equals(ConditionIdKey, StringComparison.OrdinalIgnoreCase))
                        conditionId = value;
                    else if (key.Equals(MinElapsedKey, StringComparison.OrdinalIgnoreCase) && float.TryParse(value, out float parsed))
                        minElapsed = parsed;
                }
            }

            return new RuntimeTransition(entry.fromStateId, entry.toStateId, evt?.Trim(), conditionId, minElapsed);
        }
    }

    private sealed class RuntimeConditionContext : IConditionContext, IStandardConditionData
    {
        private readonly IStateMachineConditionContext runtimeContext;
        private readonly string evt;

        public RuntimeConditionContext(IStateMachineConditionContext runtimeContext, string evt)
        {
            this.runtimeContext = runtimeContext;
            this.evt = evt;
        }

        public bool EvaluateLeaf(ConditionNode node)
        {
            if (node == null)
                return false;

            switch (node.leafType)
            {
                case ConditionLeafType.AlwaysTrue:
                    return true;
                case ConditionLeafType.AlwaysFalse:
                    return false;
                case ConditionLeafType.TimeSinceLastEvent:
                    if (runtimeContext == null || !runtimeContext.TryGetElapsedSecondsSinceEvent(evt, out float elapsed))
                        return false;
                    return elapsed >= node.floatValue;
                default:
                    return StandardConditionFunctions.Evaluate(node, this);
            }
        }


        public bool GetFlag(string key)
        {
            if (string.Equals(key, "cooldownReady", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        public float GetNumber(string key)
        {
            if (string.Equals(key, "timeSinceLastEvent", StringComparison.OrdinalIgnoreCase) &&
                runtimeContext != null &&
                runtimeContext.TryGetElapsedSecondsSinceEvent(evt, out float elapsed))
            {
                return elapsed;
            }

            return 0f;
        }

        public string GetText(string key) => string.Empty;

        public bool HasTag(string tag) => false;
    }
}
