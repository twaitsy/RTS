using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class UnitRuntimeContext
{
    private readonly List<SerializedStatContainer> statContainers;
    private readonly IReadOnlyDictionary<string, StatModifierRollup> statRollups;

    public UnitRuntimeContext(
        UnitDefinition unit,
        IReadOnlyList<WeaponDefinition> weapons,
        ArmorProfileDefinition armorProfile,
        DefenseProfileDefinition defenseProfile,
        MovementProfileDefinition movementProfile,
        LocomotionProfileDefinition locomotionProfile,
        NeedsProfileDefinition needsProfile,
        MoodDefinition moodProfile,
        BehaviourDefinition behaviourProfile,
        AIPerceptionDefinition perceptionProfile,
        ProductionProfileDefinition productionProfile,
        UnitCategoryDefinition unitCategory,
        RoleDefinition role,
        IReadOnlyList<JobDefinition> jobProfiles,
        IReadOnlyList<SerializedStatContainer> containers,
        IReadOnlyDictionary<string, StatModifierRollup> rollups)
    {
        Unit = unit;
        Weapons = weapons ?? Array.Empty<WeaponDefinition>();
        ArmorProfile = armorProfile;
        DefenseProfile = defenseProfile;
        MovementProfile = movementProfile;
        LocomotionProfile = locomotionProfile;
        NeedsProfile = needsProfile;
        MoodProfile = moodProfile;
        BehaviourProfile = behaviourProfile;
        PerceptionProfile = perceptionProfile;
        ProductionProfile = productionProfile;
        UnitCategory = unitCategory;
        Role = role;
        JobProfiles = jobProfiles ?? Array.Empty<JobDefinition>();

        statContainers = new List<SerializedStatContainer>();
        if (containers != null)
        {
            for (int i = 0; i < containers.Count; i++)
            {
                if (containers[i] != null)
                    statContainers.Add(containers[i]);
            }
        }

        statRollups = rollups ?? new Dictionary<string, StatModifierRollup>(StringComparer.Ordinal);
    }

    public UnitDefinition Unit { get; }
    public IReadOnlyList<WeaponDefinition> Weapons { get; }
    public ArmorProfileDefinition ArmorProfile { get; }
    public DefenseProfileDefinition DefenseProfile { get; }
    public MovementProfileDefinition MovementProfile { get; }
    public LocomotionProfileDefinition LocomotionProfile { get; }
    public NeedsProfileDefinition NeedsProfile { get; }
    public MoodDefinition MoodProfile { get; }
    public BehaviourDefinition BehaviourProfile { get; }
    public AIPerceptionDefinition PerceptionProfile { get; }
    public ProductionProfileDefinition ProductionProfile { get; }
    public UnitCategoryDefinition UnitCategory { get; }
    public RoleDefinition Role { get; }
    public IReadOnlyList<JobDefinition> JobProfiles { get; }

    public float ResolveStat(string statId, float defaultValue)
    {
        return CanonicalStatResolver.ResolveStatValue(statContainers, statRollups, statId, defaultValue);
    }
}

public sealed class UnitRuntimeDefinitionResolver
{
    public UnitRuntimeDefinitionResolver(
        WeaponRegistry weaponRegistry = null,
        ArmorProfileRegistry armorProfileRegistry = null,
        DefenseProfileRegistry defenseProfileRegistry = null,
        MovementProfileRegistry movementProfileRegistry = null,
        LocomotionProfileRegistry locomotionProfileRegistry = null,
        NeedsProfileRegistry needsProfileRegistry = null,
        MoodRegistry moodRegistry = null,
        BehaviourRegistry behaviourRegistry = null,
        AIPerceptionRegistry aiPerceptionRegistry = null,
        ProductionProfileRegistry productionProfileRegistry = null,
        UnitCategoryRegistry unitCategoryRegistry = null,
        RoleRegistry roleRegistry = null,
        JobRegistry jobRegistry = null)
    {
        WeaponRegistry = weaponRegistry;
        ArmorProfileRegistry = armorProfileRegistry;
        DefenseProfileRegistry = defenseProfileRegistry;
        MovementProfileRegistry = movementProfileRegistry;
        LocomotionProfileRegistry = locomotionProfileRegistry;
        NeedsProfileRegistry = needsProfileRegistry;
        MoodRegistry = moodRegistry;
        BehaviourRegistry = behaviourRegistry;
        AIPerceptionRegistry = aiPerceptionRegistry;
        ProductionProfileRegistry = productionProfileRegistry;
        UnitCategoryRegistry = unitCategoryRegistry;
        RoleRegistry = roleRegistry;
        JobRegistry = jobRegistry;
    }

    public WeaponRegistry WeaponRegistry { get; }
    public ArmorProfileRegistry ArmorProfileRegistry { get; }
    public DefenseProfileRegistry DefenseProfileRegistry { get; }
    public MovementProfileRegistry MovementProfileRegistry { get; }
    public LocomotionProfileRegistry LocomotionProfileRegistry { get; }
    public NeedsProfileRegistry NeedsProfileRegistry { get; }
    public MoodRegistry MoodRegistry { get; }
    public BehaviourRegistry BehaviourRegistry { get; }
    public AIPerceptionRegistry AIPerceptionRegistry { get; }
    public ProductionProfileRegistry ProductionProfileRegistry { get; }
    public UnitCategoryRegistry UnitCategoryRegistry { get; }
    public RoleRegistry RoleRegistry { get; }
    public JobRegistry JobRegistry { get; }
}

public static class UnitRuntimeContextResolver
{
    public static UnitRuntimeContext Resolve(UnitDefinition unit, UnitRuntimeDefinitionResolver definitionResolver)
    {
        if (unit == null)
            return null;

        var weaponRegistry = definitionResolver?.WeaponRegistry ?? WeaponRegistry.Instance;
        var armorProfileRegistry = definitionResolver?.ArmorProfileRegistry ?? ArmorProfileRegistry.Instance;
        var defenseProfileRegistry = definitionResolver?.DefenseProfileRegistry ?? DefenseProfileRegistry.Instance;
        var movementProfileRegistry = definitionResolver?.MovementProfileRegistry ?? MovementProfileRegistry.Instance;
        var locomotionProfileRegistry = definitionResolver?.LocomotionProfileRegistry ?? LocomotionProfileRegistry.Instance;
        var needsProfileRegistry = definitionResolver?.NeedsProfileRegistry ?? NeedsProfileRegistry.Instance;
        var moodRegistry = definitionResolver?.MoodRegistry ?? MoodRegistry.Instance;
        var behaviourRegistry = definitionResolver?.BehaviourRegistry ?? BehaviourRegistry.Instance;
        var perceptionRegistry = definitionResolver?.AIPerceptionRegistry ?? AIPerceptionRegistry.Instance;
        var productionProfileRegistry = definitionResolver?.ProductionProfileRegistry ?? ProductionProfileRegistry.Instance;
        var unitCategoryRegistry = definitionResolver?.UnitCategoryRegistry ?? UnitCategoryRegistry.Instance;
        var roleRegistry = definitionResolver?.RoleRegistry ?? RoleRegistry.Instance;
        var jobRegistry = definitionResolver?.JobRegistry ?? JobRegistry.Instance;

        var weapons = ResolveWeapons(unit, weaponRegistry);
        var armorProfile = ResolveDefinition(unit.ArmorProfileId, armorProfileRegistry);
        var defenseProfile = ResolveDefinition(unit.DefenseProfileId, defenseProfileRegistry);
        var movementProfile = ResolveDefinition(unit.MovementProfileId, movementProfileRegistry);
        var locomotionProfile = ResolveDefinition(unit.LocomotionProfileId, locomotionProfileRegistry);
        var needsProfile = ResolveDefinition(unit.NeedsProfileId, needsProfileRegistry);
        var moodProfile = ResolveDefinition(unit.MoodProfileId, moodRegistry);
        var behaviourProfile = ResolveDefinition(unit.AIBehaviorProfileId, behaviourRegistry);
        var perceptionProfile = ResolveDefinition(unit.PerceptionProfileId, perceptionRegistry);
        var productionProfile = ResolveDefinition(unit.ProductionProfileId, productionProfileRegistry);
        var unitCategory = ResolveDefinition(unit.UnitCategoryId, unitCategoryRegistry);
        var role = ResolveDefinition(unit.RoleId, roleRegistry);
        var jobProfiles = ResolveJobs(unit, jobRegistry);

        var containers = new List<SerializedStatContainer>
        {
            unit.Stats,
            armorProfile?.Stats,
            defenseProfile?.Stats,
            movementProfile?.Stats,
            locomotionProfile?.Stats,
            needsProfile?.Stats,
            moodProfile?.Stats,
            behaviourProfile?.Stats,
            perceptionProfile?.Stats,
            productionProfile?.Stats,
        };

        for (int i = 0; i < weapons.Count; i++)
            containers.Add(weapons[i]?.Stats);

        for (int i = 0; i < jobProfiles.Count; i++)
            containers.Add(jobProfiles[i]?.Stats);

        var rollups = CanonicalStatResolver.BuildRollups(CollectModifiers(
            unit,
            weapons,
            armorProfile,
            defenseProfile,
            movementProfile,
            locomotionProfile,
            needsProfile,
            moodProfile,
            behaviourProfile,
            perceptionProfile,
            productionProfile,
            role,
            jobProfiles));

        return new UnitRuntimeContext(
            unit,
            weapons,
            armorProfile,
            defenseProfile,
            movementProfile,
            locomotionProfile,
            needsProfile,
            moodProfile,
            behaviourProfile,
            perceptionProfile,
            productionProfile,
            unitCategory,
            role,
            jobProfiles,
            containers,
            rollups);
    }

    private static IReadOnlyList<WeaponDefinition> ResolveWeapons(UnitDefinition unit, WeaponRegistry registry)
    {
        var weapons = new List<WeaponDefinition>();
        if (unit?.WeaponIds == null)
            return weapons;

        for (int i = 0; i < unit.WeaponIds.Count; i++)
        {
            var weaponId = unit.WeaponIds[i];
            if (string.IsNullOrWhiteSpace(weaponId))
                continue;

            if (registry != null && registry.TryGet(weaponId, out var weaponDefinition) && weaponDefinition != null)
                weapons.Add(weaponDefinition);
            else
                Debug.LogWarning($"[UnitRuntimeContextResolver] Could not resolve weapon '{weaponId}' for unit '{unit.Id}'.");
        }

        return weapons;
    }

    private static IReadOnlyList<JobDefinition> ResolveJobs(UnitDefinition unit, JobRegistry registry)
    {
        var jobs = new List<JobDefinition>();
        if (unit == null)
            return jobs;

        if (unit.JobProfileIds != null)
        {
            for (int i = 0; i < unit.JobProfileIds.Count; i++)
            {
                var jobId = unit.JobProfileIds[i];
                if (string.IsNullOrWhiteSpace(jobId))
                    continue;

                if (registry != null && registry.TryGet(jobId, out var jobDefinition) && jobDefinition != null)
                    jobs.Add(jobDefinition);
            }
        }

        if (unit.JobIds != null)
        {
            for (int i = 0; i < unit.JobIds.Count; i++)
            {
                var jobId = unit.JobIds[i];
                if (string.IsNullOrWhiteSpace(jobId))
                    continue;

                if (registry != null && registry.TryGet(jobId, out var jobDefinition) && jobDefinition != null && !jobs.Contains(jobDefinition))
                    jobs.Add(jobDefinition);
            }
        }

        return jobs;
    }

    private static IEnumerable<StatModifier> CollectModifiers(
        UnitDefinition unit,
        IReadOnlyList<WeaponDefinition> weapons,
        ArmorProfileDefinition armorProfile,
        DefenseProfileDefinition defenseProfile,
        MovementProfileDefinition movementProfile,
        LocomotionProfileDefinition locomotionProfile,
        NeedsProfileDefinition needsProfile,
        MoodDefinition moodProfile,
        BehaviourDefinition behaviourProfile,
        AIPerceptionDefinition perceptionProfile,
        ProductionProfileDefinition productionProfile,
        RoleDefinition role,
        IReadOnlyList<JobDefinition> jobs)
    {
        if (unit?.StatModifiers != null)
        {
            for (int i = 0; i < unit.StatModifiers.Count; i++)
                yield return unit.StatModifiers[i];
        }

        if (armorProfile?.StatModifiers != null)
        {
            for (int i = 0; i < armorProfile.StatModifiers.Count; i++)
                yield return armorProfile.StatModifiers[i];
        }

        if (defenseProfile?.StatModifiers != null)
        {
            for (int i = 0; i < defenseProfile.StatModifiers.Count; i++)
                yield return defenseProfile.StatModifiers[i];
        }

        if (movementProfile?.StatModifiers != null)
        {
            for (int i = 0; i < movementProfile.StatModifiers.Count; i++)
                yield return movementProfile.StatModifiers[i];
        }

        if (locomotionProfile?.StatModifiers != null)
        {
            for (int i = 0; i < locomotionProfile.StatModifiers.Count; i++)
                yield return locomotionProfile.StatModifiers[i];
        }

        if (needsProfile?.StatModifiers != null)
        {
            for (int i = 0; i < needsProfile.StatModifiers.Count; i++)
                yield return needsProfile.StatModifiers[i];
        }

        if (moodProfile?.StatModifiers != null)
        {
            for (int i = 0; i < moodProfile.StatModifiers.Count; i++)
                yield return moodProfile.StatModifiers[i];
        }

        if (behaviourProfile?.StatModifiers != null)
        {
            for (int i = 0; i < behaviourProfile.StatModifiers.Count; i++)
                yield return behaviourProfile.StatModifiers[i];
        }

        if (perceptionProfile?.StatModifiers != null)
        {
            for (int i = 0; i < perceptionProfile.StatModifiers.Count; i++)
                yield return perceptionProfile.StatModifiers[i];
        }

        if (productionProfile?.StatModifiers != null)
        {
            for (int i = 0; i < productionProfile.StatModifiers.Count; i++)
                yield return productionProfile.StatModifiers[i];
        }

        if (role?.StatModifiers != null)
        {
            for (int i = 0; i < role.StatModifiers.Count; i++)
                yield return role.StatModifiers[i];
        }

        if (weapons != null)
        {
            for (int i = 0; i < weapons.Count; i++)
            {
                var weapon = weapons[i];
                if (weapon?.StatModifiers == null)
                    continue;

                for (int m = 0; m < weapon.StatModifiers.Count; m++)
                    yield return weapon.StatModifiers[m];
            }
        }

        if (jobs == null)
            yield break;

        for (int i = 0; i < jobs.Count; i++)
        {
            var job = jobs[i];
            if (job?.StatModifiers == null)
                continue;

            for (int m = 0; m < job.StatModifiers.Count; m++)
                yield return job.StatModifiers[m];
        }
    }

    private static T ResolveDefinition<T>(string id, DefinitionRegistry<T> registry)
        where T : ScriptableObject, IIdentifiable
    {
        if (registry == null || string.IsNullOrWhiteSpace(id))
            return null;

        return registry.TryGet(id, out var definition) ? definition : null;
    }
}
