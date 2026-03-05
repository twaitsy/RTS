using UnityEngine;

public static class ProductionWorkSystem
{
    public static float ComputeWorkThroughput(UnitRuntimeContext context)
    {
        if (UnitInterpreterRegistry.TryGet(context, out var interpreters) && interpreters.Work != null)
            return Mathf.Max(0f, interpreters.Work.ComputeBuildSpeed());

        return DerivedComputationModule.ComputeProductionThroughput(context);
    }

    public static float TickWorkProgress(float currentProgress, float requiredProgress, UnitRuntimeContext context)
    {
        float throughput = ComputeWorkThroughput(context);
        float nextProgress = currentProgress + throughput * Time.deltaTime;
        return Mathf.Min(requiredProgress, nextProgress);
    }
}
