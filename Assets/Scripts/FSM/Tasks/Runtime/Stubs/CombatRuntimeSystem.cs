using UnityEngine;

public static class CombatRuntimeSystem
{
    public static float ComputeDps(UnitRuntimeContext context)
    {
        return DerivedComputationModule.ComputeDps(context);
    }

    public static float ComputeThreat(UnitRuntimeContext context)
    {
        return DerivedComputationModule.ComputeThreat(context);
    }

    public static float ComputeEffectiveHp(UnitRuntimeContext context)
    {
        return DerivedComputationModule.ComputeEffectiveHp(context);
    }
}
