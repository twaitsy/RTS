using System;
using System.Collections.Generic;
using UnityEngine;

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
    object FindNearest(string resourceTypeId, Vector3 position);
}

public interface IDropoffLocatorService
{
    DropoffReceiver FindNearest(Vector3 position);
}

public interface IMovementService
{
    void MoveTo(GameObject actor, Vector3 target, UnitRuntimeContext context);
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
        public object FindNearest(string resourceTypeId, Vector3 position)
        {
            return global::ResourceLocator.FindNearest(resourceTypeId, position);
        }
    }

    private sealed class DropoffLocatorService : IDropoffLocatorService
    {
        public DropoffReceiver FindNearest(Vector3 position)
        {
            return global::DropoffLocator.FindNearest(position);
        }
    }

    private sealed class MovementService : IMovementService
    {
        public void MoveTo(GameObject actor, Vector3 target, UnitRuntimeContext context)
        {
            if (UnitInterpreterRegistry.TryGet(context, out var interpreters) && interpreters.Movement != null)
            {
                interpreters.Movement.MoveTo(actor, target);
                return;
            }

            global::MovementSystem.MoveTo(actor, target, context);
        }
    }
}

public class TaskContext
{
    private readonly Queue<TaskRuntimeEvent> pendingEvents = new();

    public GameObject Actor;
    public object Target; // Resource node, drop-off, etc.
    public float WorkTimer;
    public int InventoryCount;
    public UnitRuntimeContext RuntimeContext;
    public TaskSimulationServices Services;
    public ITaskEventSink EventSink;

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
