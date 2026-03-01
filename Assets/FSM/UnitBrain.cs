using UnityEngine;
using System.Collections.Generic;
using System;

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
                TransitionTo(t.To);
                return;
            }
        }
    }

    private static List<BehaviourState> BuildAncestryChain(BehaviourState state)
    {
        List<BehaviourState> chain = new List<BehaviourState>();

        while (state != null)
        {
            chain.Add(state);
            state = state.Parent;
        }

        return chain;
    }

    private void TransitionTo(BehaviourState target)
    {
        if (target == null)
            throw new ArgumentNullException(nameof(target));

        if (current == target)
            return;

        List<BehaviourState> oldChain = BuildAncestryChain(current);
        List<BehaviourState> newChain = BuildAncestryChain(target);

        int oldIndex = oldChain.Count - 1;
        int newIndex = newChain.Count - 1;

        while (oldIndex >= 0 && newIndex >= 0 && oldChain[oldIndex] == newChain[newIndex])
        {
            oldIndex--;
            newIndex--;
        }

        for (int i = 0; i <= oldIndex; i++)
            oldChain[i].OnExit(this);

        current = target;

        for (int i = newIndex; i >= 0; i--)
            newChain[i].OnEnter(this);
    }
}
