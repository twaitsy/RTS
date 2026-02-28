using UnityEngine;

public class UnitTaskController : MonoBehaviour
{
    [SerializeField] private TaskDefinition startingTask;

    private TaskRunner runner;

    private void Start()
    {
        if (startingTask != null)
            runner = new TaskRunner(startingTask, gameObject);
    }

    private void Update()
    {
        runner?.Tick();
    }
}