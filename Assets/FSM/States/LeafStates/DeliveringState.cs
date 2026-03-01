using UnityEngine;

[CreateAssetMenu(menuName = "RTS/FSM/States/Delivering")]
public class DeliveringState : BehaviourState
{
    public override bool HandleEvent(UnitBrain brain, string evt)
    {
        if (evt == "DeliveryComplete")
            return true;

        return false;
    }
}