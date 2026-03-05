using System;
using System.Collections.Generic;

public static class UnitInterpreterRegistry
{
    private static readonly Dictionary<int, InterpreterSet> InterpretersByUnitId = new();

    public static void Register(UnitRuntimeContext context, InterpreterSet interpreters)
    {
        if (context?.Unit == null || interpreters == null)
            return;

        InterpretersByUnitId[context.Unit.GetInstanceID()] = interpreters;
    }

    public static bool TryGet(UnitRuntimeContext context, out InterpreterSet interpreters)
    {
        interpreters = null;
        if (context?.Unit == null)
            return false;

        return InterpretersByUnitId.TryGetValue(context.Unit.GetInstanceID(), out interpreters) && interpreters != null;
    }

    public static void Unregister(UnitRuntimeContext context)
    {
        if (context?.Unit == null)
            return;

        InterpretersByUnitId.Remove(context.Unit.GetInstanceID());
    }

    public static void Clear()
    {
        InterpretersByUnitId.Clear();
    }
}

public sealed class InterpreterSetPool
{
    private readonly Stack<InterpreterSet> pool = new();

    public InterpreterSet Rent(UnitRuntimeContext context)
    {
        if (pool.Count == 0)
            return InterpreterSet.Create(context);

        var set = pool.Pop();
        set.Bind(context);
        return set;
    }

    public void Return(InterpreterSet set)
    {
        if (set == null)
            return;

        pool.Push(set);
    }

    public void Clear()
    {
        pool.Clear();
    }
}
