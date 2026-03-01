using UnityEngine;

[CreateAssetMenu(menuName = "RTS/FSM/Transition")]
public class BehaviourTransition : ScriptableObject
{
    public BehaviourState From;
    public string EventName;
    public BehaviourState To;
}