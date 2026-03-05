using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitBrain : MonoBehaviour, IStateMachineConditionContext
{
    [Header("Definition-Driven FSM")]
    public StateMachineDefinition MachineDefinition;

    [Header("Simulation Runtime")]
    public UnitDefinition UnitDefinition;

    [Tooltip("Optional: load machine from StateMachineRegistry by ID when MachineDefinition is unset.")]
    public string MachineDefinitionId;

    [Tooltip("Migration glue mapping legacy BehaviourState assets to StateDefinition IDs.")]
    public List<LegacyStateIdMapping> LegacyStateMappings = new();

    [Tooltip("Fallback state used only during migration when no mapping exists.")]
    public BehaviourState LegacyInitialState;

    [Header("Deprecated Direct-Wired Transition Path")]
    [Obsolete("Use MachineDefinition + LegacyStateMappings instead.")]
    public BehaviourState InitialState;

    [Obsolete("Use MachineDefinition transitions instead.")]
    public List<BehaviourTransition> Transitions;

    private readonly Dictionary<string, float> lastEventTimeByName = new();

    private StateMachineRuntime runtime;
    private BehaviourState current;
    private TaskRunner taskRunner;
    private UnitRuntimeContext runtimeContext;

    public BehaviourState CurrentState => current;

    private void Start()
    {
        runtime = new StateMachineRuntime();

        if (!runtime.Initialize(MachineDefinition, MachineDefinitionId, LegacyStateMappings, LegacyInitialState, InitialState))
        {
            Debug.LogError($"{nameof(UnitBrain)} on '{name}' failed to initialize FSM runtime.");
            enabled = false;
            return;
        }

        current = runtime.GetInitialRuntimeState();
        current?.OnEnter(this);

        runtimeContext = UnitRuntimeContextResolver.Resolve(UnitDefinition, definitionResolver: null);

        EmitEvent("OnOrderGather");
    }

    private void Update()
    {
        current?.Tick(this);

        if (taskRunner != null && !taskRunner.IsComplete)
            taskRunner.Tick();
    }

    public void StartTask(TaskDefinition task)
    {
        taskRunner = new TaskRunner(task, gameObject, runtimeContext);
    }

    public void EmitEvent(string evt)
    {
        if (string.IsNullOrWhiteSpace(evt) || current == null || runtime == null)
            return;

        lastEventTimeByName[evt] = Time.time;

        BehaviourState s = current;

        while (s != null)
        {
            if (s.HandleEvent(this, evt))
                return;

            s = runtime.GetParentState(s);
        }

        if (runtime.TryResolveTransition(current, evt, this, out BehaviourState target))
            TransitionTo(target);
    }

    public bool TryGetElapsedSecondsSinceEvent(string eventName, out float seconds)
    {
        if (!lastEventTimeByName.TryGetValue(eventName, out float eventTime))
        {
            seconds = 0f;
            return false;
        }

        seconds = Mathf.Max(0f, Time.time - eventTime);
        return true;
    }

    private void TransitionTo(BehaviourState target)
    {
        if (target == null)
            throw new ArgumentNullException(nameof(target));

        if (current == target)
            return;

        List<BehaviourState> oldChain = runtime.BuildAncestryChain(current);
        List<BehaviourState> newChain = runtime.BuildAncestryChain(target);

        int oldIndex = oldChain.Count - 1;
        int newIndex = newChain.Count - 1;

        while (oldIndex >= 0 && newIndex >= 0 && oldChain[oldIndex] == newChain[newIndex])
        {
            oldIndex--;
            newIndex--;
        }

        for (int i = 0; i <= oldIndex; i++)
            oldChain[i].OnExit(this);

        current = target;

        for (int i = newIndex; i >= 0; i--)
            newChain[i].OnEnter(this);
    }
}

[Serializable]
public struct LegacyStateIdMapping
{
    public BehaviourState state;
    public string stateDefinitionId;
}
