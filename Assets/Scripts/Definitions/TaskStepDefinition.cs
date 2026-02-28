using NUnit.Framework;
using UnityEngine;

public abstract class TaskStepDefinition : ScriptableObject
{
    // Called by the task runner to execute this step.
    public abstract TaskStepResult Execute(TaskContext context);
}