using UnityEngine;

[CreateAssetMenu(menuName = "RTS/FSM/States/Gathering")]
public class GatheringState : BehaviourState
{
    public override void OnEnter(UnitBrain brain)
    {
        // Start the task associated with this state (task.worker.gathering)
        brain.StartCurrentStateTask();
    }

    public override bool HandleEvent(UnitBrain brain, string evt)
    {
        return false;
    }
}
