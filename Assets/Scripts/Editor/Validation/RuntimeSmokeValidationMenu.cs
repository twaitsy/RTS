#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class RuntimeSmokeValidationMenu
{
    private const string WorkerTaskId = "task.worker.gather.loop";

    [MenuItem("Tools/Validation/Run Runtime Smoke Tests")]
    public static void RunRuntimeSmokeTestsMenu()
    {
        RunRuntimeSmokeTests();
        EditorUtility.DisplayDialog("Runtime Smoke", "Runtime smoke tests passed.", "OK");
    }

    public static void ValidateRuntimeSmokeForCI()
    {
        RunRuntimeSmokeTests();
        Debug.Log("[Validation] Runtime smoke tests passed.");
    }

    private static void RunRuntimeSmokeTests()
    {
        RunInterpreterSmoke();
        RunFsmTaskSmoke();
    }

    private static void RunInterpreterSmoke()
    {
        var unit = ScriptableObject.CreateInstance<UnitDefinition>();
        var serialized = new SerializedObject(unit);
        serialized.FindProperty("id").stringValue = "unit.smoke";
        serialized.FindProperty("schemaModeId").stringValue = "worker";
        AddStat(serialized, CanonicalStatIds.Movement.MoveSpeed, 6f);
        AddStat(serialized, CanonicalStatIds.Movement.Acceleration, 12f);
        AddStat(serialized, CanonicalStatIds.Movement.TurnRate, 360f);
        AddStat(serialized, CanonicalStatIds.Needs.HungerRate, 0.5f);
        AddStat(serialized, CanonicalStatIds.Production.WorkSpeed, 1f);
        AddStat(serialized, CanonicalStatIds.Production.BuildSpeed, 1f);
        AddStat(serialized, CanonicalStatIds.Combat.AttackDamage, 10f);
        AddStat(serialized, CanonicalStatIds.Combat.AttackSpeed, 1f);
        AddStat(serialized, CanonicalStatIds.Combat.Health, 100f);
        serialized.ApplyModifiedPropertiesWithoutUndo();

        var context = UnitRuntimeContextResolver.Resolve(unit, definitionResolver: null);
        var interpreters = InterpreterSet.Create(context);

        var actor = new GameObject("smoke-actor");
        actor.transform.position = Vector3.zero;
        interpreters.Movement.MoveTo(actor, new Vector3(2f, 0f, 0f));
        if (actor.transform.position.x <= 0f)
            throw new Exception("Interpreter smoke failed: movement did not update actor position.");

        var needs = new UnitNeedsState(100f, 100f, 100f, 100f, 0f);
        var nextNeeds = interpreters.Needs.Tick(needs, 1f);
        if (nextNeeds.Hunger >= needs.Hunger)
            throw new Exception("Interpreter smoke failed: needs tick did not decay hunger.");

        var canPerceive = interpreters.Perception.IsTargetPerceivable(actor.transform.position, actor.transform.position + Vector3.forward * 0.5f);
        if (!canPerceive)
            throw new Exception("Interpreter smoke failed: perception should detect near target.");

        if (interpreters.Combat.ComputeEffectiveHp() <= 0f)
            throw new Exception("Interpreter smoke failed: combat EHP must be positive.");

        UnityEngine.Object.DestroyImmediate(actor);
        UnityEngine.Object.DestroyImmediate(unit);
    }

    private static void RunFsmTaskSmoke()
    {
        var actor = new GameObject("fsm-smoke-actor");
        var resourceObject = new GameObject("fsm-smoke-resource");
        var dropoffObject = new GameObject("fsm-smoke-dropoff");

        actor.transform.position = Vector3.zero;
        resourceObject.transform.position = new Vector3(0.2f, 0f, 0f);
        dropoffObject.transform.position = new Vector3(0.4f, 0f, 0f);

        var resourceNode = resourceObject.AddComponent<ResourceNodeRuntime>();
        resourceNode.SetFallbackResourceType("resource.wood");
        var dropoffReceiver = dropoffObject.AddComponent<BuildingRuntime>();

        var unit = ScriptableObject.CreateInstance<UnitDefinition>();
        var unitSerialized = new SerializedObject(unit);
        unitSerialized.FindProperty("id").stringValue = "unit.fsm-smoke";
        unitSerialized.FindProperty("schemaModeId").stringValue = "worker";
        AddStat(unitSerialized, CanonicalStatIds.Movement.MoveSpeed, 10f);
        AddStat(unitSerialized, CanonicalStatIds.Movement.Acceleration, 30f);
        AddStat(unitSerialized, CanonicalStatIds.Movement.TurnRate, 720f);
        AddStat(unitSerialized, CanonicalStatIds.Production.CarryCapacity, 5f);
        AddStat(unitSerialized, CanonicalStatIds.Production.WorkSpeed, 3f);
        AddStat(unitSerialized, CanonicalStatIds.Production.BuildSpeed, 3f);
        unitSerialized.ApplyModifiedPropertiesWithoutUndo();

        var context = UnitRuntimeContextResolver.Resolve(unit, definitionResolver: null);
        UnitInterpreterRegistry.Register(context, InterpreterSet.Create(context));

        var task = BuildSmokeTask();
        var machine = BuildSmokeStateMachine();

        var idle = ScriptableObject.CreateInstance<IdleState>();
        var moveToResource = ScriptableObject.CreateInstance<MoveToResourceState>();
        var gathering = ScriptableObject.CreateInstance<GatheringState>();
        var moveToDropoff = ScriptableObject.CreateInstance<MoveToDropoffState>();
        var delivering = ScriptableObject.CreateInstance<DeliveringState>();

        var mappings = new List<LegacyStateIdMapping>
        {
            new() { state = idle, stateDefinitionId = "state.worker.idle" },
            new() { state = moveToResource, stateDefinitionId = "state.worker.moveToResource" },
            new() { state = gathering, stateDefinitionId = "state.worker.gathering" },
            new() { state = moveToDropoff, stateDefinitionId = "state.worker.moveToDropoff" },
            new() { state = delivering, stateDefinitionId = "state.worker.delivering" },
        };

        var runtime = new StateMachineRuntime();
        if (!runtime.Initialize(machine, null, mappings, idle, null))
            throw new Exception("FSM smoke failed: runtime initialization failed.");

        var current = runtime.GetInitialRuntimeState();
        var sink = new SmokeEventSink();
        var runner = new TaskRunner(task, actor, context, TaskSimulationServices.Defaults, sink);

        var seenEvents = new List<string>();
        bool delivered = false;

        for (int i = 0; i < 300 && !delivered; i++)
        {
            runner.Tick();

            while (sink.TryDequeue(out var evt))
            {
                seenEvents.Add(evt);
                if (runtime.TryResolveTransition(current, evt, DummyConditionContext.Instance, out var target))
                    current = target;

                if (string.Equals(evt, "task.deliver.completed", StringComparison.Ordinal))
                {
                    delivered = true;
                    break;
                }
            }
        }

        if (!delivered)
            throw new Exception("FSM smoke failed: task loop never reached deliver completion event.");

        AssertContains(seenEvents, "task.query.succeeded");
        AssertContains(seenEvents, "task.move.arrived.resource");
        AssertContains(seenEvents, "task.gather.completed");
        AssertContains(seenEvents, "task.move.arrived.dropoff");
        AssertContains(seenEvents, "task.deliver.completed");

        UnitInterpreterRegistry.Clear();

        UnityEngine.Object.DestroyImmediate(task);
        UnityEngine.Object.DestroyImmediate(machine);
        UnityEngine.Object.DestroyImmediate(idle);
        UnityEngine.Object.DestroyImmediate(moveToResource);
        UnityEngine.Object.DestroyImmediate(gathering);
        UnityEngine.Object.DestroyImmediate(moveToDropoff);
        UnityEngine.Object.DestroyImmediate(delivering);
        UnityEngine.Object.DestroyImmediate(unit);
        UnityEngine.Object.DestroyImmediate(dropoffObject);
        UnityEngine.Object.DestroyImmediate(resourceObject);
        UnityEngine.Object.DestroyImmediate(actor);
    }

    private static TaskDefinition BuildSmokeTask()
    {
        var task = ScriptableObject.CreateInstance<TaskDefinition>();
        var query = CreateStep<QueryStep>(step =>
        {
            step.FindProperty("resourceType").stringValue = "resource.wood";
            step.FindProperty("querySucceededEventId").stringValue = "task.query.succeeded";
            step.FindProperty("queryFailedEventId").stringValue = "task.failed";
        });
        var moveToResource = CreateStep<MoveToStep>(step =>
        {
            step.FindProperty("targetType").intValue = (int)TaskTargetType.ResourceNode;
            step.FindProperty("arrivedEventId").stringValue = "task.move.arrived.resource";
            step.FindProperty("failedEventId").stringValue = "task.failed";
        });
        var work = CreateStep<WorkStep>(step =>
        {
            step.FindProperty("duration").floatValue = 0.1f;
            step.FindProperty("gatherAmount").intValue = 1;
            step.FindProperty("resourceTypeId").stringValue = "resource.wood";
            step.FindProperty("completedEventId").stringValue = "task.gather.completed";
            step.FindProperty("failedEventId").stringValue = "task.failed";
        });
        var queryDropoff = CreateStep<QueryDropoffStep>(step =>
        {
            step.FindProperty("querySucceededEventId").stringValue = "task.query.dropoff.succeeded";
            step.FindProperty("queryFailedEventId").stringValue = "task.failed";
        });
        var moveToDropoff = CreateStep<MoveToStep>(step =>
        {
            step.FindProperty("targetType").intValue = (int)TaskTargetType.Dropoff;
            step.FindProperty("arrivedEventId").stringValue = "task.move.arrived.dropoff";
            step.FindProperty("failedEventId").stringValue = "task.failed";
        });
        var deliver = CreateStep<DeliverStep>(step =>
        {
            step.FindProperty("deliveredEventId").stringValue = "task.deliver.completed";
            step.FindProperty("failedEventId").stringValue = "task.failed";
        });

        var serialized = new SerializedObject(task);
        serialized.FindProperty("id").stringValue = WorkerTaskId;
        var steps = serialized.FindProperty("steps");
        steps.arraySize = 6;
        steps.GetArrayElementAtIndex(0).objectReferenceValue = query;
        steps.GetArrayElementAtIndex(1).objectReferenceValue = moveToResource;
        steps.GetArrayElementAtIndex(2).objectReferenceValue = work;
        steps.GetArrayElementAtIndex(3).objectReferenceValue = queryDropoff;
        steps.GetArrayElementAtIndex(4).objectReferenceValue = moveToDropoff;
        steps.GetArrayElementAtIndex(5).objectReferenceValue = deliver;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        return task;
    }

    private static StateMachineDefinition BuildSmokeStateMachine()
    {
        var machine = ScriptableObject.CreateInstance<StateMachineDefinition>();
        var serialized = new SerializedObject(machine);
        serialized.FindProperty("id").stringValue = "machine.worker.gather.loop";
        serialized.FindProperty("initialStateId").stringValue = "state.worker.idle";

        var states = serialized.FindProperty("states");
        states.arraySize = 5;
        SetState(states, 0, "state.worker.idle", WorkerTaskId);
        SetState(states, 1, "state.worker.moveToResource", WorkerTaskId);
        SetState(states, 2, "state.worker.gathering", WorkerTaskId);
        SetState(states, 3, "state.worker.moveToDropoff", WorkerTaskId);
        SetState(states, 4, "state.worker.delivering", WorkerTaskId);

        var transitions = serialized.FindProperty("transitions");
        transitions.arraySize = 5;
        SetTransition(transitions, 0, "state.worker.idle", "state.worker.moveToResource", "task.query.succeeded");
        SetTransition(transitions, 1, "state.worker.moveToResource", "state.worker.gathering", "task.move.arrived.resource");
        SetTransition(transitions, 2, "state.worker.gathering", "state.worker.moveToDropoff", "task.gather.completed");
        SetTransition(transitions, 3, "state.worker.moveToDropoff", "state.worker.delivering", "task.move.arrived.dropoff");
        SetTransition(transitions, 4, "state.worker.delivering", "state.worker.idle", "task.deliver.completed");

        serialized.ApplyModifiedPropertiesWithoutUndo();
        return machine;
    }

    private static TStep CreateStep<TStep>(Action<SerializedObject> configure)
        where TStep : TaskStepDefinition
    {
        var step = ScriptableObject.CreateInstance<TStep>();
        var serialized = new SerializedObject(step);
        configure(serialized);
        serialized.ApplyModifiedPropertiesWithoutUndo();
        return step;
    }

    private static void SetState(SerializedProperty states, int index, string stateId, string actionId)
    {
        var element = states.GetArrayElementAtIndex(index);
        element.FindPropertyRelative("stateId").stringValue = stateId;
        element.FindPropertyRelative("actionId").stringValue = actionId;
    }

    private static void SetTransition(SerializedProperty transitions, int index, string from, string to, string eventName)
    {
        var element = transitions.GetArrayElementAtIndex(index);
        element.FindPropertyRelative("fromStateId").stringValue = from;
        element.FindPropertyRelative("toStateId").stringValue = to;
        element.FindPropertyRelative("eventName").stringValue = eventName;
        element.FindPropertyRelative("conditionId").stringValue = string.Empty;
    }

    private static void AddStat(SerializedObject serialized, string statId, float value)
    {
        var entries = serialized.FindProperty("stats").FindPropertyRelative("entries");
        var index = entries.arraySize;
        entries.arraySize++;
        var element = entries.GetArrayElementAtIndex(index);
        element.FindPropertyRelative("statId").stringValue = statId;
        element.FindPropertyRelative("value").floatValue = value;
    }

    private static void AssertContains(List<string> values, string expected)
    {
        if (!values.Contains(expected))
            throw new Exception($"FSM smoke failed: missing event '{expected}'.");
    }

    private sealed class SmokeEventSink : ITaskEventSink
    {
        private readonly Queue<string> events = new();

        public void EmitEvent(string eventId, string payload = null)
        {
            if (!string.IsNullOrWhiteSpace(eventId))
                events.Enqueue(eventId);
        }

        public bool TryDequeue(out string evt)
        {
            if (events.Count == 0)
            {
                evt = null;
                return false;
            }

            evt = events.Dequeue();
            return true;
        }
    }

    private sealed class DummyConditionContext : IStateMachineConditionContext
    {
        public static readonly DummyConditionContext Instance = new();

        public bool TryGetElapsedSecondsSinceEvent(string eventName, out float seconds)
        {
            seconds = 0f;
            return false;
        }
    }
}
#endif
