using System;
using UnityEngine;
using UnityEngine.AI;

public interface IUnitInterpreter
{
    void Bind(UnitRuntimeContext context);
}

public interface IMovementInterpreter : IUnitInterpreter
{
    void MoveTo(GameObject actor, Vector3 target);
    bool TryMoveTo(GameObject actor, Vector3 target, out string failureReason);
    bool TryGetRemainingDistance(GameObject actor, out float remainingDistance);
}

public interface IPerceptionInterpreter : IUnitInterpreter
{
    bool IsTargetPerceivable(Vector3 observerPosition, Vector3 targetPosition, float targetStealth = 0f, float emittedNoise = 1f, Vector3? observerForward = null);
}

public interface INeedsInterpreter : IUnitInterpreter
{
    UnitNeedsState Tick(UnitNeedsState current, float deltaTime);
}

public interface ICombatInterpreter : IUnitInterpreter
{
    float ComputeDps();
    float ComputeThreat();
    float ComputeEffectiveHp();
}

public interface IWorkInterpreter : IUnitInterpreter
{
    float ComputeGatherRate();
    float ComputeCarryCapacity();
    float ComputeBuildSpeed();
    float ComputeGatherThroughput(ResourceNodeRuntime node);
}

public interface IDropoffInterpreter : IUnitInterpreter
{
    bool TryDeliver(BuildingRuntime dropoff, string resourceTypeId, int amount, out int deliveredAmount, out string failureReason);
}

public interface IAIInterpreter : IUnitInterpreter
{
    float ComputeDecisionScore();
}

public readonly struct UnitNeedsState
{
    public UnitNeedsState(float hunger, float thirst, float fatigue, float morale, float stress)
    {
        Hunger = hunger;
        Thirst = thirst;
        Fatigue = fatigue;
        Morale = morale;
        Stress = stress;
    }

    public float Hunger { get; }
    public float Thirst { get; }
    public float Fatigue { get; }
    public float Morale { get; }
    public float Stress { get; }
}

public sealed class MovementInterpreter : IMovementInterpreter
{
    private const float DefaultMoveSpeed = 2f;
    private const float DefaultAcceleration = 8f;
    private const float DefaultTurnRate = 360f;
    private UnitRuntimeContext context;

    public void Bind(UnitRuntimeContext context)
    {
        this.context = context;
    }

    public void MoveTo(GameObject actor, Vector3 target)
    {
        TryMoveTo(actor, target, out _);
    }

    public bool TryMoveTo(GameObject actor, Vector3 target, out string failureReason)
    {
        failureReason = null;
        if (actor == null)
        {
            failureReason = "Actor is null.";
            return false;
        }

        if (!CanTraverse(actor.transform.position, target))
        {
            failureReason = "Locomotion profile cannot traverse target terrain.";
            return false;
        }

        float fallbackSpeed = context?.MovementProfile?.MoveSpeedMultiplier ?? context?.LocomotionProfile?.Speed ?? DefaultMoveSpeed;
        float moveSpeed = context?.ResolveStat(CanonicalStatIds.Movement.MoveSpeed, fallbackSpeed) ?? fallbackSpeed;

        float fallbackAcceleration = context?.MovementProfile?.Acceleration ?? DefaultAcceleration;
        float acceleration = context?.ResolveStat(CanonicalStatIds.Movement.Acceleration, fallbackAcceleration) ?? fallbackAcceleration;

        float fallbackTurnRate = context?.MovementProfile?.TurnRate ?? DefaultTurnRate;
        float turnRate = context?.ResolveStat(CanonicalStatIds.Movement.TurnRate, fallbackTurnRate) ?? fallbackTurnRate;

        float stoppingDistance = context?.MovementProfile?.StoppingDistance ?? 0.1f;

        if (!TryEnsureAgent(actor, out var agent, out failureReason))
        {
            // Compatibility path for smoke tests or scenes without baked NavMesh.
            MoveTransformFallback(actor.transform, target, moveSpeed, acceleration, turnRate, stoppingDistance);
            failureReason = null;
            return true;
        }

        agent.speed = Mathf.Max(0.1f, moveSpeed);
        agent.acceleration = Mathf.Max(0.1f, acceleration);
        agent.angularSpeed = Mathf.Max(1f, turnRate);
        agent.stoppingDistance = Mathf.Max(0f, stoppingDistance);

        if (!agent.SetDestination(target))
        {
            failureReason = "NavMeshAgent rejected destination.";
            return false;
        }

        if (!agent.pathPending && agent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            failureReason = "Invalid NavMesh path to destination.";
            return false;
        }

        return true;
    }

    public bool TryGetRemainingDistance(GameObject actor, out float remainingDistance)
    {
        remainingDistance = float.MaxValue;
        if (actor == null)
            return false;

        var agent = actor.GetComponent<NavMeshAgent>();
        if (agent == null || !agent.isOnNavMesh)
            return false;

        if (agent.pathPending)
            return false;

        remainingDistance = agent.remainingDistance;
        return true;
    }

    private bool CanTraverse(Vector3 from, Vector3 to)
    {
        if (context?.LocomotionProfile == null)
            return true;

        var deltaY = Mathf.Abs(to.y - from.y);
        if (deltaY > 0.25f)
            return context.LocomotionProfile.CanTraverseAir;

        if (to.y < -0.1f)
            return context.LocomotionProfile.CanTraverseWater;

        return context.LocomotionProfile.CanTraverseGround;
    }

    private bool TryEnsureAgent(GameObject actor, out NavMeshAgent agent, out string failureReason)
    {
        failureReason = null;
        agent = actor.GetComponent<NavMeshAgent>();
        if (agent == null)
            agent = actor.AddComponent<NavMeshAgent>();

        if (agent == null)
        {
            failureReason = "Unable to create NavMeshAgent.";
            return false;
        }

        if (!agent.isOnNavMesh)
        {
            if (!NavMesh.SamplePosition(actor.transform.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                failureReason = "Actor is not on NavMesh and no nearby sample point was found.";
                return false;
            }

            if (!agent.Warp(hit.position))
            {
                failureReason = "NavMeshAgent warp failed while snapping to NavMesh.";
                return false;
            }
        }

        return true;
    }

    private static void MoveTransformFallback(Transform transform, Vector3 target, float moveSpeed, float acceleration, float turnRate, float stoppingDistance)
    {
        Vector3 direction = target - transform.position;
        float distance = direction.magnitude;
        if (distance <= Mathf.Max(0f, stoppingDistance))
            return;

        if (direction.sqrMagnitude > 0.0001f)
        {
            Vector3 forward = Vector3.RotateTowards(transform.forward, direction.normalized, Mathf.Deg2Rad * turnRate * Time.deltaTime, 0f);
            if (forward.sqrMagnitude > 0.0001f)
                transform.forward = forward;
        }

        float speed = Mathf.Max(0.1f, moveSpeed);
        speed = Mathf.MoveTowards(0f, speed, Mathf.Max(0.1f, acceleration) * Time.deltaTime);
        Vector3 step = direction.normalized * speed * Time.deltaTime;
        if (step.magnitude > distance)
            step = direction;

        transform.position += step;
    }
}

public sealed class PerceptionInterpreter : IPerceptionInterpreter
{
    private UnitRuntimeContext context;

    public void Bind(UnitRuntimeContext context)
    {
        this.context = context;
    }

    public bool IsTargetPerceivable(Vector3 observerPosition, Vector3 targetPosition, float targetStealth = 0f, float emittedNoise = 1f, Vector3? observerForward = null)
    {
        var profile = context?.PerceptionProfile;
        float profileRadius = profile?.HearingRadius ?? 0f;
        float perceptionRadius = context?.ResolveStat(CanonicalStatIds.Perception.PerceptionRadius, profileRadius) ?? profileRadius;
        float hearingRadius = context?.ResolveStat(CanonicalStatIds.Perception.HearingRadius, profile?.HearingRadius ?? 0f) ?? 0f;
        float visionArc = context?.ResolveStat(CanonicalStatIds.Perception.VisionArc, profile?.VisionArc ?? 360f) ?? 360f;
        float detection = Mathf.Clamp01(profile?.StealthDetection ?? 0f);

        float sqrDistance = (targetPosition - observerPosition).sqrMagnitude;
        if (sqrDistance <= (hearingRadius * hearingRadius * Mathf.Max(0f, emittedNoise)))
            return true;

        if (sqrDistance > (perceptionRadius * perceptionRadius))
            return false;

        if (targetStealth > detection)
            return false;

        if (!observerForward.HasValue || visionArc >= 360f)
            return true;

        var toTarget = targetPosition - observerPosition;
        if (toTarget.sqrMagnitude < 0.0001f)
            return true;

        var angle = Vector3.Angle(observerForward.Value.normalized, toTarget.normalized);
        return angle <= (visionArc * 0.5f);
    }
}

public sealed class NeedsInterpreter : INeedsInterpreter
{
    private UnitRuntimeContext context;

    public void Bind(UnitRuntimeContext context)
    {
        this.context = context;
    }

    public UnitNeedsState Tick(UnitNeedsState current, float deltaTime)
    {
        float hungerRate = Mathf.Max(0f, (context?.ResolveStat(CanonicalStatIds.Needs.HungerRate, 0f) ?? 0f) * (context?.NeedsProfile?.HungerCurve ?? 1f));
        float thirstRate = Mathf.Max(0f, (context?.ResolveStat(CanonicalStatIds.Needs.ThirstRate, 0f) ?? 0f) * (context?.NeedsProfile?.ThirstCurve ?? 1f));
        float fatigueRate = Mathf.Max(0f, (context?.ResolveStat(CanonicalStatIds.Needs.FatigueRate, 0f) ?? 0f) * (context?.NeedsProfile?.FatigueCurve ?? 1f));
        float moraleDecay = Mathf.Max(0f, (context?.ResolveStat(CanonicalStatIds.Needs.MoraleDecayRate, 0f) ?? 0f) * (context?.NeedsProfile?.MoraleCurve ?? 1f));
        float stressGain = Mathf.Max(0f, (context?.ResolveStat(CanonicalStatIds.Needs.StressGainRate, 0f) ?? 0f) * (context?.NeedsProfile?.StressCurve ?? 1f));

        return new UnitNeedsState(
            Mathf.Max(0f, current.Hunger - (hungerRate * deltaTime)),
            Mathf.Max(0f, current.Thirst - (thirstRate * deltaTime)),
            Mathf.Max(0f, current.Fatigue - (fatigueRate * deltaTime)),
            Mathf.Max(0f, current.Morale - (moraleDecay * deltaTime)),
            Mathf.Max(0f, current.Stress + (stressGain * deltaTime)));
    }
}

public sealed class CombatInterpreter : ICombatInterpreter
{
    private UnitRuntimeContext context;

    public void Bind(UnitRuntimeContext context)
    {
        this.context = context;
    }

    public float ComputeDps() => DerivedComputationModule.ComputeDps(context);
    public float ComputeThreat() => DerivedComputationModule.ComputeThreat(context);
    public float ComputeEffectiveHp() => DerivedComputationModule.ComputeEffectiveHp(context);
}

public sealed class WorkInterpreter : IWorkInterpreter
{
    private UnitRuntimeContext context;

    public void Bind(UnitRuntimeContext context)
    {
        this.context = context;
    }

    public float ComputeGatherRate()
    {
        return Mathf.Max(0f, context?.ResolveStat(CanonicalStatIds.Production.GatherRate, 0f) ?? 0f);
    }

    public float ComputeCarryCapacity()
    {
        return Mathf.Max(0f, context?.ResolveStat(CanonicalStatIds.Production.CarryCapacity, 0f) ?? 0f);
    }

    public float ComputeBuildSpeed()
    {
        return Mathf.Max(0f, context?.ResolveStat(CanonicalStatIds.Production.BuildSpeed, 1f) ?? 1f);
    }

    public float ComputeGatherThroughput(ResourceNodeRuntime node)
    {
        float gatherRate = Mathf.Max(0.01f, ComputeGatherRate());
        float workSpeed = Mathf.Max(0.01f, context?.ResolveStat(CanonicalStatIds.Production.WorkSpeed, 1f) ?? 1f);
        float difficulty = Mathf.Max(0.1f, node?.GatherDifficulty ?? 1f);
        float nodeMultiplier = Mathf.Max(0.05f, node?.ThroughputMultiplier ?? 1f);
        return (gatherRate * workSpeed * nodeMultiplier) / difficulty;
    }
}

public sealed class DropoffInterpreter : IDropoffInterpreter
{
    public void Bind(UnitRuntimeContext context)
    {
    }

    public bool TryDeliver(BuildingRuntime dropoff, string resourceTypeId, int amount, out int deliveredAmount, out string failureReason)
    {
        deliveredAmount = 0;
        failureReason = null;

        if (dropoff == null)
        {
            failureReason = "Dropoff runtime is null.";
            return false;
        }

        return dropoff.TryReceiveDelivery(resourceTypeId, amount, out deliveredAmount, out failureReason);
    }
}

public sealed class AIInterpreter : IAIInterpreter
{
    private UnitRuntimeContext context;

    public void Bind(UnitRuntimeContext context)
    {
        this.context = context;
    }

    public float ComputeDecisionScore()
    {
        if (context == null)
            return 0f;

        var derived = context.Derived;
        float aggression = Mathf.Max(0f, context.ResolveStat(CanonicalStatIds.AI.Aggression, 0f));
        float courage = Mathf.Max(0f, context.ResolveStat(CanonicalStatIds.AI.Courage, 0f));
        float obedience = Mathf.Max(0f, context.ResolveStat(CanonicalStatIds.AI.Obedience, 0f));
        float alertness = Mathf.Max(0f, context.ResolveStat(CanonicalStatIds.AI.Alertness, 0f));

        return (derived.ThreatRating * 0.35f) + (derived.MoraleStability * 0.25f) + (aggression * 0.15f) + (courage * 0.1f) + (obedience * 0.1f) + (alertness * 0.05f);
    }
}

public sealed class InterpreterSet
{
    public InterpreterSet(
        IMovementInterpreter movement,
        IPerceptionInterpreter perception,
        INeedsInterpreter needs,
        ICombatInterpreter combat,
        IWorkInterpreter work,
        IDropoffInterpreter dropoff,
        IAIInterpreter ai)
    {
        Movement = movement;
        Perception = perception;
        Needs = needs;
        Combat = combat;
        Work = work;
        Dropoff = dropoff;
        AI = ai;
    }

    public IMovementInterpreter Movement { get; }
    public IPerceptionInterpreter Perception { get; }
    public INeedsInterpreter Needs { get; }
    public ICombatInterpreter Combat { get; }
    public IWorkInterpreter Work { get; }
    public IDropoffInterpreter Dropoff { get; }
    public IAIInterpreter AI { get; }

    public static InterpreterSet Create(UnitRuntimeContext context)
    {
        var set = new InterpreterSet(
            new MovementInterpreter(),
            new PerceptionInterpreter(),
            new NeedsInterpreter(),
            new CombatInterpreter(),
            new WorkInterpreter(),
            new DropoffInterpreter(),
            new AIInterpreter());

        set.Bind(context);
        return set;
    }

    public void Bind(UnitRuntimeContext context)
    {
        Movement?.Bind(context);
        Perception?.Bind(context);
        Needs?.Bind(context);
        Combat?.Bind(context);
        Work?.Bind(context);
        Dropoff?.Bind(context);
        AI?.Bind(context);
    }
}
