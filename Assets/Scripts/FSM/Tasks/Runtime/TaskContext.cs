using UnityEngine;

public class TaskContext
{
    public GameObject Actor;
    public object Target; // Resource node, drop-off, etc.
    public float WorkTimer;
    public int InventoryCount;
    public UnitRuntimeContext RuntimeContext;

    // Expand later with: needs, tools, reservations, etc.
}
