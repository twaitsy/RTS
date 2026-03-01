using UnityEngine;

[CreateAssetMenu(menuName = "RTS/FSM/States/Gathering")]
public class GatheringState : BehaviourState
{
    public override bool HandleEvent(UnitBrain brain, string evt)
    {
        if (evt == "WorkComplete")
            return true;

        return false;
    }
}