using UnityEngine;

[CreateAssetMenu(menuName = "RTS/FSM/States/Working")]
public class WorkingState : BehaviourState
{
    public TaskDefinition gatherTask;

    public override void OnEnter(UnitBrain brain)
    {
        if (gatherTask != null)
            brain.StartTask(gatherTask);
    }

    public override bool HandleEvent(UnitBrain brain, string evt)
    {
        if (evt == "Attacked")
            return true;

        return false;
    }
}