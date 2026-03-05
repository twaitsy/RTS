using UnityEngine;

public static class ProductionWorkSystem
{
    public static float ComputeWorkThroughput(UnitRuntimeContext context)
    {
        return DerivedComputationModule.ComputeProductionThroughput(context);
    }

    public static float TickWorkProgress(float currentProgress, float requiredProgress, UnitRuntimeContext context)
    {
        float throughput = ComputeWorkThroughput(context);
        float nextProgress = currentProgress + throughput * Time.deltaTime;
        return Mathf.Min(requiredProgress, nextProgress);
    }
}
