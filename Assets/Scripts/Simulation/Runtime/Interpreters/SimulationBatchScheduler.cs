using System.Collections.Generic;
using Unity.Profiling;

public interface ISimulationBatchTickable
{
    void TickBatch(float deltaTime);
}

public sealed class SimulationBatchScheduler
{
    private static readonly ProfilerMarker BatchTickMarker = new("Simulation.BatchScheduler.Tick");

    private readonly List<ISimulationBatchTickable> movementBatch = new();
    private readonly List<ISimulationBatchTickable> needsBatch = new();
    private readonly List<ISimulationBatchTickable> perceptionBatch = new();

    public void RegisterMovement(ISimulationBatchTickable tickable) => Register(movementBatch, tickable);
    public void RegisterNeeds(ISimulationBatchTickable tickable) => Register(needsBatch, tickable);
    public void RegisterPerception(ISimulationBatchTickable tickable) => Register(perceptionBatch, tickable);

    public void UnregisterMovement(ISimulationBatchTickable tickable) => movementBatch.Remove(tickable);
    public void UnregisterNeeds(ISimulationBatchTickable tickable) => needsBatch.Remove(tickable);
    public void UnregisterPerception(ISimulationBatchTickable tickable) => perceptionBatch.Remove(tickable);

    public void TickMovement(float deltaTime) => TickList(movementBatch, deltaTime);
    public void TickNeeds(float deltaTime) => TickList(needsBatch, deltaTime);
    public void TickPerception(float deltaTime) => TickList(perceptionBatch, deltaTime);

    public void Clear()
    {
        movementBatch.Clear();
        needsBatch.Clear();
        perceptionBatch.Clear();
    }

    private static void Register(List<ISimulationBatchTickable> bucket, ISimulationBatchTickable tickable)
    {
        if (tickable == null || bucket.Contains(tickable))
            return;

        bucket.Add(tickable);
    }

    private static void TickList(List<ISimulationBatchTickable> bucket, float deltaTime)
    {
        using var scope = BatchTickMarker.Auto();

        for (int i = 0; i < bucket.Count; i++)
            bucket[i]?.TickBatch(deltaTime);
    }
}
