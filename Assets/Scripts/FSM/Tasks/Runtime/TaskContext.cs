using System;
using System.Collections.Generic;
using UnityEngine;

public enum TaskTargetType
{
    None = 0,
    ResourceNode = 1,
    Dropoff = 2,
}

public readonly struct TaskRuntimeEvent
{
    public TaskRuntimeEvent(string eventId, string payload)
    {
        EventId = eventId;
        Payload = payload;
    }

    public string EventId { get; }
    public string Payload { get; }
}

public interface ITaskEventSink
{
    void EmitEvent(string eventId, string payload = null);
}

public interface IResourceLocatorService
{
    ResourceNodeRuntime FindNearest(string resourceTypeId, Vector3 position);
}

public interface IDropoffLocatorService
{
    BuildingRuntime FindNearest(Vector3 position, string resourceTypeId = null);
}

public interface IMovementService
{
    bool MoveTo(GameObject actor, Vector3 target, UnitRuntimeContext context, out string failureReason);
}

public sealed class TaskSimulationServices
{
    private static TaskSimulationServices defaults;

    public IResourceLocatorService ResourceLocator { get; set; }
    public IDropoffLocatorService DropoffLocator { get; set; }
    public IMovementService Movement { get; set; }

    public static TaskSimulationServices Defaults => defaults ??= new TaskSimulationServices
    {
        ResourceLocator = new ResourceLocatorService(),
        DropoffLocator = new DropoffLocatorService(),
        Movement = new MovementService(),
    };

    private sealed class ResourceLocatorService : IResourceLocatorService
    {
        public ResourceNodeRuntime FindNearest(string resourceTypeId, Vector3 position)
        {
            return global::ResourceLocator.FindNearest(resourceTypeId, position);
        }
    }

    private sealed class DropoffLocatorService : IDropoffLocatorService
    {
        public BuildingRuntime FindNearest(Vector3 position, string resourceTypeId = null)
        {
            return global::DropoffLocator.FindNearest(position, resourceTypeId);
        }
    }

    private sealed class MovementService : IMovementService
    {
        public bool MoveTo(GameObject actor, Vector3 target, UnitRuntimeContext context, out string failureReason)
        {
            if (UnitInterpreterRegistry.TryGet(context, out var interpreters) && interpreters.Movement != null)
            {
                return interpreters.Movement.TryMoveTo(actor, target, out failureReason);
            }

            return global::MovementSystem.MoveTo(actor, target, context, out failureReason);
        }
    }
}

public sealed class TaskBlackboard
{
    public ResourceNodeRuntime ResourceTarget;
    public BuildingRuntime DropoffTarget;
    public string CarriedResourceTypeId;
    public int InventoryCount;
    public float GatherProgress;
}

public class TaskContext
{
    private readonly Queue<TaskRuntimeEvent> pendingEvents = new();

    public GameObject Actor;
    public object Target; // legacy compatibility
    public float WorkTimer;
    public UnitRuntimeContext RuntimeContext;
    public TaskSimulationServices Services;
    public ITaskEventSink EventSink;
    public TaskBlackboard Blackboard;

    public ResourceNodeRuntime ResourceTarget
    {
        get => Blackboard?.ResourceTarget;
        set
        {
            if (Blackboard != null)
                Blackboard.ResourceTarget = value;
            Target = value;
        }
    }

    public BuildingRuntime DropoffTarget
    {
        get => Blackboard?.DropoffTarget;
        set
        {
            if (Blackboard != null)
                Blackboard.DropoffTarget = value;
            Target = value;
        }
    }

    public string CarriedResourceTypeId
    {
        get => Blackboard?.CarriedResourceTypeId;
        set
        {
            if (Blackboard != null)
                Blackboard.CarriedResourceTypeId = value;
        }
    }

    public int InventoryCount
    {
        get => Blackboard?.InventoryCount ?? 0;
        set
        {
            if (Blackboard != null)
                Blackboard.InventoryCount = Mathf.Max(0, value);
        }
    }

    public float GatherProgress
    {
        get => Blackboard?.GatherProgress ?? 0f;
        set
        {
            if (Blackboard != null)
                Blackboard.GatherProgress = Mathf.Max(0f, value);
        }
    }

    public void EnqueueEvent(string eventId, string payload = null)
    {
        if (string.IsNullOrWhiteSpace(eventId))
            return;

        pendingEvents.Enqueue(new TaskRuntimeEvent(eventId, payload));
    }

    public bool TryDequeueEvent(out TaskRuntimeEvent runtimeEvent)
    {
        if (pendingEvents.Count == 0)
        {
            runtimeEvent = default;
            return false;
        }

        runtimeEvent = pendingEvents.Dequeue();
        return true;
    }

    public void ApplyResult(TaskStepResult result)
    {
        if (!string.IsNullOrWhiteSpace(result.EventId))
            EnqueueEvent(result.EventId, result.EventPayload);

        if (!string.IsNullOrWhiteSpace(result.FailureReason))
            Debug.LogWarning($"Task failure: {result.FailureReason}");
    }

    public void FlushQueuedEvents()
    {
        if (EventSink == null)
            return;

        while (TryDequeueEvent(out var runtimeEvent))
            EventSink.EmitEvent(runtimeEvent.EventId, runtimeEvent.Payload);
    }
}
