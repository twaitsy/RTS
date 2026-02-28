using UnityEngine;

public static class MovementSystem
{
    private const float SPEED = 2f;

    public static void MoveTo(GameObject actor, Vector3 target)
    {
        Transform t = actor.transform;

        Vector3 direction = target - t.position;
        float distance = direction.magnitude;

        if (distance <= 0f)
            return;

        // Normalized movement
        Vector3 step = direction.normalized * SPEED * Time.deltaTime;

        // Prevent overshooting
        if (step.magnitude > distance)
            step = direction;

        t.position += step;

        Debug.Log($"MovementSystem: Moving '{actor.name}' to {target}, newPos={t.position}");
    }
}