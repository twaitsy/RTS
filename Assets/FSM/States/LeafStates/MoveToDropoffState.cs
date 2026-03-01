using UnityEngine;

[CreateAssetMenu(menuName = "RTS/FSM/States/MoveToDropoff")]
public class MoveToDropoffState : BehaviourState
{
    public override bool HandleEvent(UnitBrain brain, string evt)
    {
        if (evt == "Arrived")
            return true;

        return false;
    }
}