using UnityEngine;
using System.Collections.Generic;

public class UnitBrain : MonoBehaviour
{
    public BehaviourState InitialState;
    public List<BehaviourTransition> Transitions;

    private BehaviourState current;
    private TaskRunner taskRunner;

    void Start()
    {
        current = InitialState;
        current.OnEnter(this);
        EmitEvent("OnOrderGather");

    }

    void Update()
    {
        current.Tick(this);

        if (taskRunner != null && !taskRunner.IsComplete)
            taskRunner.Tick();
    }

    public void StartTask(TaskDefinition task)
    {
        taskRunner = new TaskRunner(task, gameObject);
    }

    public void EmitEvent(string evt)
    {
        BehaviourState s = current;

        while (s != null)
        {
            if (s.HandleEvent(this, evt))
                return;

            s = s.Parent;
        }

        foreach (var t in Transitions)
        {
            if (t.From == current && t.EventName == evt)
            {
                current.OnExit(this);
                current = t.To;
                current.OnEnter(this);
                return;
            }
        }
    }
}