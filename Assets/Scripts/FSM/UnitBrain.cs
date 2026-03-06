using System;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

public class UnitBrain : MonoBehaviour, IStateMachineConditionContext, ITaskEventSink
{
    private static readonly ProfilerMarker TickMarker = new("Simulation.UnitBrain.Tick");
    private static readonly ProfilerMarker InterpreterMarker = new("Simulation.UnitBrain.Interpreters");
    private static readonly ProfilerMarker TaskMarker = new("Simulation.UnitBrain.Task");

    [Header("Definition-Driven FSM")]
    public StateMachineDefinition MachineDefinition;

    [Header("Simulation Runtime")]
    public UnitDefinition UnitDefinition;

    [Tooltip("Optional: load machine from StateMachineRegistry by ID when MachineDefinition is unset.")]
    public string MachineDefinitionId;

    [Tooltip("Migration glue mapping legacy BehaviourState assets to StateDefinition IDs.")]
    public List<LegacyStateIdMapping> LegacyStateMappings = new();

    [Header("Simulation Tick")]
    [SerializeField, Min(0.001f)] private float simulationTickIntervalSeconds = 0.1f;
    [SerializeField, Min(1)] private int maxTicksPerFrame = 5;

    [Tooltip("Fallback state used only during migration when no mapping exists.")]
    public BehaviourState LegacyInitialState;

    [Header("Deprecated Direct-Wired Transition Path")]
    [Obsolete("Use MachineDefinition + LegacyStateMappings instead.")]
    public BehaviourState InitialState;

    [Obsolete("Use MachineDefinition transitions instead.")]
    public List<BehaviourTransition> Transitions;

    private readonly Dictionary<string, float> lastEventTimeByName = new();
    private readonly HashSet<string> loggedMissingTaskIds = new(StringComparer.Ordinal);

    private StateMachineRuntime runtime;
    private BehaviourState current;
    private TaskRunner taskRunner;
    private UnitRuntimeContext runtimeContext;
    private InterpreterSet interpreters;
    private UnitRuntime unitRuntime;
    private readonly TaskBlackboard taskBlackboard = new();
    private UnitNeedsState needsState = new(100f, 100f, 100f, 100f, 0f);
    private string activeActionId;
    private float tickAccumulator;

    public BehaviourState CurrentState => current;
    public UnitRuntimeContext RuntimeContext => runtimeContext;

    private void Start()
    {
        unitRuntime = GetComponent<UnitRuntime>();
        if (unitRuntime == null)
            unitRuntime = gameObject.AddComponent<UnitRuntime>();

        if (unitRuntime.UnitDefinition == null)
            unitRuntime.UnitDefinition = UnitDefinition;

        unitRuntime.RuntimeRefreshed += OnRuntimeRefreshed;

        runtime = new StateMachineRuntime();

        if (!runtime.Initialize(MachineDefinition, MachineDefinitionId, LegacyStateMappings, LegacyInitialState, InitialState))
        {
            Debug.LogError($"{nameof(UnitBrain)} on '{name}' failed to initialize FSM runtime.");
            enabled = false;
            return;
        }

        RefreshRuntimePipeline(UnitRuntimeInvalidationReason.ProfileChanged);

        current = runtime.GetInitialRuntimeState();
        current?.OnEnter(this);
        EnsureStateTaskBinding(current);
    }

    private void OnDestroy()
    {
        if (unitRuntime != null)
            unitRuntime.RuntimeRefreshed -= OnRuntimeRefreshed;
    }

    private void Update()
    {
        if (!enabled)
            return;

        tickAccumulator += Time.deltaTime;
        var executedTicks = 0;

        while (tickAccumulator >= simulationTickIntervalSeconds && executedTicks < maxTicksPerFrame)
        {
            TickSimulation();
            tickAccumulator -= simulationTickIntervalSeconds;
            executedTicks++;
        }

        if (executedTicks == maxTicksPerFrame && tickAccumulator >= simulationTickIntervalSeconds)
            tickAccumulator = 0f;
    }

    private void TickSimulation()
    {
        using var tickScope = TickMarker.Auto();

        EnsureStateTaskBinding(current);

        using (InterpreterMarker.Auto())
        {
            if (interpreters?.Needs != null)
                needsState = interpreters.Needs.Tick(needsState, simulationTickIntervalSeconds);

            _ = interpreters?.AI?.ComputeDecisionScore();
        }

        current?.Tick(this);

        using (TaskMarker.Auto())
        {
            if (taskRunner != null && !taskRunner.IsComplete)
            {
                taskRunner.SetRuntimeContext(runtimeContext);
                taskRunner.Tick();
            }
        }
    }

    public void StartTask(TaskDefinition task)
    {
        StartTask(task, task?.Id);
    }

    public void EmitEvent(string evt)
    {
        EmitEvent(evt, null);
    }

    public void EmitEvent(string eventId, string payload)
    {
        if (string.IsNullOrWhiteSpace(eventId) || current == null || runtime == null)
            return;

        lastEventTimeByName[eventId] = Time.time;

        BehaviourState state = current;
        while (state != null)
        {
            if (state.HandleEvent(this, eventId))
                return;

            state = runtime.GetParentState(state);
        }

        if (runtime.TryResolveTransition(current, eventId, this, out BehaviourState target))
            TransitionTo(target);
    }

    public void OnStatChanged()
    {
        RefreshRuntimePipeline(UnitRuntimeInvalidationReason.StatChanged);
    }

    public void OnEquipmentChanged()
    {
        RefreshRuntimePipeline(UnitRuntimeInvalidationReason.EquipmentChanged);
    }

    public void OnTechChanged()
    {
        RefreshRuntimePipeline(UnitRuntimeInvalidationReason.TechChanged);
    }

    public void OnProfileChanged()
    {
        RefreshRuntimePipeline(UnitRuntimeInvalidationReason.ProfileChanged);
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

    private void RefreshRuntimePipeline(UnitRuntimeInvalidationReason reason)
    {
        if (unitRuntime != null)
        {
            unitRuntime.Refresh(reason);
            runtimeContext = unitRuntime.Context;
            interpreters = unitRuntime.Interpreters;
        }
        else if (UnitDefinition != null)
        {
            UnitRuntimeContextResolver.Invalidate(UnitDefinition, reason);
            runtimeContext = UnitRuntimeContextResolver.Resolve(UnitDefinition, definitionResolver: null);
            interpreters = InterpreterSet.Create(runtimeContext);
            UnitInterpreterRegistry.Register(runtimeContext, interpreters);
        }

        if (taskRunner != null && !taskRunner.IsComplete)
            taskRunner.SetRuntimeContext(runtimeContext);
    }

    private void EnsureStateTaskBinding(BehaviourState state)
    {
        if (state == null || runtime == null)
            return;

        if (!runtime.TryGetActionId(state, out var actionId))
        {
            activeActionId = null;
            return;
        }

        if (taskRunner != null && !taskRunner.IsComplete && string.Equals(activeActionId, actionId, StringComparison.Ordinal))
            return;

        if (TaskRegistry.Instance == null)
            return;

        if (!TaskRegistry.Instance.TryGet(actionId, out var taskDefinition) || taskDefinition == null)
        {
            if (loggedMissingTaskIds.Add(actionId))
                Debug.LogWarning($"[UnitBrain] Missing task definition for actionId '{actionId}'.");
            return;
        }

        StartTask(taskDefinition, actionId);
    }

    private void StartTask(TaskDefinition task, string actionId)
    {
        if (task == null)
            return;

        taskRunner = new TaskRunner(task, gameObject, runtimeContext, TaskSimulationServices.Defaults, this, taskBlackboard);
        activeActionId = actionId?.Trim();
    }

    private void OnRuntimeRefreshed(UnitRuntimeContext context, InterpreterSet set)
    {
        runtimeContext = context;
        interpreters = set;
        if (taskRunner != null && !taskRunner.IsComplete)
            taskRunner.SetRuntimeContext(context);
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

        EnsureStateTaskBinding(current);
    }
}

[Serializable]
public struct LegacyStateIdMapping
{
    public BehaviourState state;
    public string stateDefinitionId;
}
