using System;
using UnityEngine;

[Obsolete("Legacy direct-wired transitions are deprecated. Use StateMachineDefinition transitions via UnitBrain instead.")]
[CreateAssetMenu(menuName = "RTS/FSM/Transition")]
public class BehaviourTransition : ScriptableObject
{
    public BehaviourState From;
    public string EventName;
    public BehaviourState To;
}
