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
        MovementProfileDefinition movementProfile,
        NeedsProfileDefinition needsProfile,
        AIPerceptionDefinition perceptionProfile,
        ProductionProfileDefinition productionProfile,
        IReadOnlyList<JobDefinition> jobProfiles,
        IReadOnlyList<SerializedStatContainer> containers,
        IReadOnlyDictionary<string, StatModifierRollup> rollups)
    {
        Unit = unit;
        Weapons = weapons ?? Array.Empty<WeaponDefinition>();
        MovementProfile = movementProfile;
        NeedsProfile = needsProfile;
        PerceptionProfile = perceptionProfile;
        ProductionProfile = productionProfile;
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
    public MovementProfileDefinition MovementProfile { get; }
    public NeedsProfileDefinition NeedsProfile { get; }
    public AIPerceptionDefinition PerceptionProfile { get; }
    public ProductionProfileDefinition ProductionProfile { get; }
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
        MovementProfileRegistry movementProfileRegistry = null,
        NeedsProfileRegistry needsProfileRegistry = null,
        AIPerceptionRegistry aiPerceptionRegistry = null,
        ProductionProfileRegistry productionProfileRegistry = null,
        JobRegistry jobRegistry = null)
    {
        WeaponRegistry = weaponRegistry;
        MovementProfileRegistry = movementProfileRegistry;
        NeedsProfileRegistry = needsProfileRegistry;
        AIPerceptionRegistry = aiPerceptionRegistry;
        ProductionProfileRegistry = productionProfileRegistry;
        JobRegistry = jobRegistry;
    }

    public WeaponRegistry WeaponRegistry { get; }
    public MovementProfileRegistry MovementProfileRegistry { get; }
    public NeedsProfileRegistry NeedsProfileRegistry { get; }
    public AIPerceptionRegistry AIPerceptionRegistry { get; }
    public ProductionProfileRegistry ProductionProfileRegistry { get; }
    public JobRegistry JobRegistry { get; }
}

public static class UnitRuntimeContextResolver
{
    public static UnitRuntimeContext Resolve(UnitDefinition unit, UnitRuntimeDefinitionResolver definitionResolver)
    {
        if (unit == null)
            return null;

        var weapons = ResolveWeapons(unit, definitionResolver?.WeaponRegistry);
        var movementProfile = ResolveDefinition(unit.MovementProfileId, definitionResolver?.MovementProfileRegistry);
        var needsProfile = ResolveDefinition(unit.NeedsProfileId, definitionResolver?.NeedsProfileRegistry);
        var perceptionProfile = ResolveDefinition(unit.PerceptionProfileId, definitionResolver?.AIPerceptionRegistry);
        var productionProfile = ResolveDefinition(unit.ProductionProfileId, definitionResolver?.ProductionProfileRegistry);
        var jobProfiles = ResolveJobs(unit, definitionResolver?.JobRegistry);

        var containers = new List<SerializedStatContainer>
        {
            unit.Stats,
            movementProfile?.Stats,
            perceptionProfile?.Stats,
            productionProfile?.Stats,
        };

        for (int i = 0; i < weapons.Count; i++)
            containers.Add(weapons[i]?.Stats);

        for (int i = 0; i < jobProfiles.Count; i++)
            containers.Add(jobProfiles[i]?.Stats);

        var rollups = CanonicalStatResolver.BuildRollups(unit.StatModifiers);

        return new UnitRuntimeContext(
            unit,
            weapons,
            movementProfile,
            needsProfile,
            perceptionProfile,
            productionProfile,
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

    private static T ResolveDefinition<T>(string id, DefinitionRegistry<T> registry)
        where T : ScriptableObject, IIdentifiable
    {
        if (registry == null || string.IsNullOrWhiteSpace(id))
            return null;

        return registry.TryGet(id, out var definition) ? definition : null;
    }
}
