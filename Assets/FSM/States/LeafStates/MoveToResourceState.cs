using UnityEngine;

[CreateAssetMenu(menuName = "RTS/FSM/States/MoveToResource")]
public class MoveToResourceState : BehaviourState
{
    public override bool HandleEvent(UnitBrain brain, string evt)
    {
        if (evt == "Arrived")
            return true;

        return false;
    }
}