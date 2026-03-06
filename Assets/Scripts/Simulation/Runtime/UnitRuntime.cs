using System;
using UnityEngine;

[DisallowMultipleComponent]
public class UnitRuntime : MonoBehaviour
{
    [SerializeField] private UnitDefinition unitDefinition;

    private readonly InterpreterSetPool interpreterPool = new();

    public event Action<UnitRuntimeContext, InterpreterSet> RuntimeRefreshed;

    public UnitDefinition UnitDefinition
    {
        get => unitDefinition;
        set => unitDefinition = value;
    }

    public UnitRuntimeContext Context { get; private set; }
    public InterpreterSet Interpreters { get; private set; }

    private void OnEnable()
    {
        Refresh(UnitRuntimeInvalidationReason.ProfileChanged);
    }

    private void OnDisable()
    {
        if (Context != null)
            UnitInterpreterRegistry.Unregister(Context);

        if (Interpreters != null)
            interpreterPool.Return(Interpreters);

        Context = null;
        Interpreters = null;
    }

    public void Refresh(UnitRuntimeInvalidationReason reason)
    {
        if (unitDefinition == null)
            return;

        UnitRuntimeContextResolver.Invalidate(unitDefinition, reason);

        if (Context != null)
            UnitInterpreterRegistry.Unregister(Context);

        var nextContext = UnitRuntimeContextResolver.Resolve(unitDefinition, definitionResolver: null);
        if (Interpreters != null)
            interpreterPool.Return(Interpreters);

        Context = nextContext;
        Interpreters = interpreterPool.Rent(Context);
        UnitInterpreterRegistry.Register(Context, Interpreters);
        RuntimeRefreshed?.Invoke(Context, Interpreters);
    }

    public void NotifyStatChanged() => Refresh(UnitRuntimeInvalidationReason.StatChanged);
    public void NotifyEquipmentChanged() => Refresh(UnitRuntimeInvalidationReason.EquipmentChanged);
    public void NotifyTechChanged() => Refresh(UnitRuntimeInvalidationReason.TechChanged);
    public void NotifyProfileChanged() => Refresh(UnitRuntimeInvalidationReason.ProfileChanged);
}
