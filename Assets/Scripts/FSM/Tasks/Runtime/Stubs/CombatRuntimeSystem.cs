using UnityEngine;

public static class CombatRuntimeSystem
{
    public static float ComputeDps(UnitRuntimeContext context)
    {
        if (UnitInterpreterRegistry.TryGet(context, out var interpreters) && interpreters.Combat != null)
            return interpreters.Combat.ComputeDps();

        return DerivedComputationModule.ComputeDps(context);
    }

    public static float ComputeThreat(UnitRuntimeContext context)
    {
        if (UnitInterpreterRegistry.TryGet(context, out var interpreters) && interpreters.Combat != null)
            return interpreters.Combat.ComputeThreat();

        return DerivedComputationModule.ComputeThreat(context);
    }

    public static float ComputeEffectiveHp(UnitRuntimeContext context)
    {
        if (UnitInterpreterRegistry.TryGet(context, out var interpreters) && interpreters.Combat != null)
            return interpreters.Combat.ComputeEffectiveHp();

        return DerivedComputationModule.ComputeEffectiveHp(context);
    }
}
