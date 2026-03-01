using UnityEngine;

public abstract class BehaviourState : ScriptableObject
{
    public BehaviourState Parent;

    public virtual void OnEnter(UnitBrain brain) { }
    public virtual void OnExit(UnitBrain brain) { }
    public virtual void Tick(UnitBrain brain) { }
    public virtual bool HandleEvent(UnitBrain brain, string evt) => false;
}