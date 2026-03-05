using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public enum UnitRuntimeInvalidationReason
{
    StatChanged,
    EquipmentChanged,
    TechChanged,
    ProfileChanged,
}

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
        IReadOnlyDictionary<string, StatModifierRollup> rollups,
        DerivedRuntimeSnapshot derived)
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
        Derived = derived;

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
    public DerivedRuntimeSnapshot Derived { get; }

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
    private sealed class ContextCacheEntry
    {
        public string Signature;
        public UnitRuntimeContext Context;
    }

    private sealed class ProfileCacheEntry
    {
        public string Signature;
        public ResolvedProfiles Profiles;
        public IReadOnlyList<SerializedStatContainer> Containers;
    }

    private sealed class RollupCacheEntry
    {
        public string Signature;
        public IReadOnlyDictionary<string, StatModifierRollup> Rollups;
    }

    private sealed class DerivedCacheEntry
    {
        public string Signature;
        public DerivedRuntimeSnapshot Snapshot;
    }

    private sealed class ResolvedProfiles
    {
        public IReadOnlyList<WeaponDefinition> Weapons;
        public ArmorProfileDefinition ArmorProfile;
        public DefenseProfileDefinition DefenseProfile;
        public MovementProfileDefinition MovementProfile;
        public LocomotionProfileDefinition LocomotionProfile;
        public NeedsProfileDefinition NeedsProfile;
        public MoodDefinition MoodProfile;
        public BehaviourDefinition BehaviourProfile;
        public AIPerceptionDefinition PerceptionProfile;
        public ProductionProfileDefinition ProductionProfile;
        public UnitCategoryDefinition UnitCategory;
        public RoleDefinition Role;
        public IReadOnlyList<JobDefinition> JobProfiles;
    }

    private static readonly Dictionary<int, ContextCacheEntry> ContextCache = new();
    private static readonly Dictionary<int, ProfileCacheEntry> ProfileCache = new();
    private static readonly Dictionary<int, RollupCacheEntry> RollupCache = new();
    private static readonly Dictionary<int, DerivedCacheEntry> DerivedCache = new();

    public static UnitRuntimeContext Resolve(UnitDefinition unit, UnitRuntimeDefinitionResolver definitionResolver)
    {
        if (unit == null)
            return null;

        if (definitionResolver != null)
            return ResolveUncached(unit, definitionResolver);

        var key = unit.GetInstanceID();
        var signature = BuildSignature(unit);

        if (ContextCache.TryGetValue(key, out var cachedContext) &&
            string.Equals(cachedContext.Signature, signature, StringComparison.Ordinal) &&
            cachedContext.Context != null)
        {
            return cachedContext.Context;
        }

        var (profiles, containers) = ResolveProfilesAndContainersCached(key, signature, unit, definitionResolver);
        var rollups = ResolveRollupsCached(key, signature, unit, profiles);
        var derived = ResolveDerivedCached(key, signature, unit, profiles, containers, rollups);

        var context = BuildContext(unit, profiles, containers, rollups, derived);
        ContextCache[key] = new ContextCacheEntry
        {
            Signature = signature,
            Context = context,
        };

        return context;
    }

    public static void Invalidate(UnitDefinition unit)
    {
        Invalidate(unit, UnitRuntimeInvalidationReason.ProfileChanged);
    }

    public static void Invalidate(UnitDefinition unit, UnitRuntimeInvalidationReason reason)
    {
        if (unit == null)
            return;

        var key = unit.GetInstanceID();
        ContextCache.Remove(key);
        ProfileCache.Remove(key);
        RollupCache.Remove(key);
        DerivedCache.Remove(key);
    }

    public static void ClearCache()
    {
        ContextCache.Clear();
        ProfileCache.Clear();
        RollupCache.Clear();
        DerivedCache.Clear();
        UnitInterpreterRegistry.Clear();
    }

    private static UnitRuntimeContext ResolveUncached(UnitDefinition unit, UnitRuntimeDefinitionResolver definitionResolver)
    {
        var (profiles, containers) = ResolveProfilesAndContainersUncached(unit, definitionResolver);
        var rollups = CanonicalStatResolver.BuildRollups(CollectModifiers(unit, profiles));
        var preliminary = BuildContext(unit, profiles, containers, rollups, default);
        var derived = DerivedComputationModule.ComputeSnapshot(preliminary);
        return BuildContext(unit, profiles, containers, rollups, derived);
    }

    private static (ResolvedProfiles Profiles, IReadOnlyList<SerializedStatContainer> Containers) ResolveProfilesAndContainersCached(
        int key,
        string signature,
        UnitDefinition unit,
        UnitRuntimeDefinitionResolver definitionResolver)
    {
        if (ProfileCache.TryGetValue(key, out var entry) && string.Equals(entry.Signature, signature, StringComparison.Ordinal) && entry.Profiles != null)
            return (entry.Profiles, entry.Containers ?? Array.Empty<SerializedStatContainer>());

        var (profiles, containers) = ResolveProfilesAndContainersUncached(unit, definitionResolver);
        ProfileCache[key] = new ProfileCacheEntry
        {
            Signature = signature,
            Profiles = profiles,
            Containers = containers,
        };

        return (profiles, containers);
    }

    private static IReadOnlyDictionary<string, StatModifierRollup> ResolveRollupsCached(int key, string signature, UnitDefinition unit, ResolvedProfiles profiles)
    {
        if (RollupCache.TryGetValue(key, out var entry) && string.Equals(entry.Signature, signature, StringComparison.Ordinal) && entry.Rollups != null)
            return entry.Rollups;

        var rollups = CanonicalStatResolver.BuildRollups(CollectModifiers(unit, profiles));
        RollupCache[key] = new RollupCacheEntry
        {
            Signature = signature,
            Rollups = rollups,
        };

        return rollups;
    }

    private static DerivedRuntimeSnapshot ResolveDerivedCached(
        int key,
        string signature,
        UnitDefinition unit,
        ResolvedProfiles profiles,
        IReadOnlyList<SerializedStatContainer> containers,
        IReadOnlyDictionary<string, StatModifierRollup> rollups)
    {
        if (DerivedCache.TryGetValue(key, out var entry) && string.Equals(entry.Signature, signature, StringComparison.Ordinal))
            return entry.Snapshot;

        var preliminary = BuildContext(unit, profiles, containers, rollups, default);
        var snapshot = DerivedComputationModule.ComputeSnapshot(preliminary);

        DerivedCache[key] = new DerivedCacheEntry
        {
            Signature = signature,
            Snapshot = snapshot,
        };

        return snapshot;
    }

    private static UnitRuntimeContext BuildContext(
        UnitDefinition unit,
        ResolvedProfiles profiles,
        IReadOnlyList<SerializedStatContainer> containers,
        IReadOnlyDictionary<string, StatModifierRollup> rollups,
        DerivedRuntimeSnapshot derived)
    {
        return new UnitRuntimeContext(
            unit,
            profiles.Weapons,
            profiles.ArmorProfile,
            profiles.DefenseProfile,
            profiles.MovementProfile,
            profiles.LocomotionProfile,
            profiles.NeedsProfile,
            profiles.MoodProfile,
            profiles.BehaviourProfile,
            profiles.PerceptionProfile,
            profiles.ProductionProfile,
            profiles.UnitCategory,
            profiles.Role,
            profiles.JobProfiles,
            containers,
            rollups,
            derived);
    }

    private static (ResolvedProfiles Profiles, IReadOnlyList<SerializedStatContainer> Containers) ResolveProfilesAndContainersUncached(
        UnitDefinition unit,
        UnitRuntimeDefinitionResolver definitionResolver)
    {
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

        var profiles = new ResolvedProfiles
        {
            Weapons = ResolveWeapons(unit, weaponRegistry),
            ArmorProfile = ResolveDefinition(unit.ArmorProfileId, armorProfileRegistry),
            DefenseProfile = ResolveDefinition(unit.DefenseProfileId, defenseProfileRegistry),
            MovementProfile = ResolveDefinition(unit.MovementProfileId, movementProfileRegistry),
            LocomotionProfile = ResolveDefinition(unit.LocomotionProfileId, locomotionProfileRegistry),
            NeedsProfile = ResolveDefinition(unit.NeedsProfileId, needsProfileRegistry),
            MoodProfile = ResolveDefinition(unit.MoodProfileId, moodRegistry),
            BehaviourProfile = ResolveDefinition(unit.AIBehaviorProfileId, behaviourRegistry),
            PerceptionProfile = ResolveDefinition(unit.PerceptionProfileId, perceptionRegistry),
            ProductionProfile = ResolveDefinition(unit.ProductionProfileId, productionProfileRegistry),
            UnitCategory = ResolveDefinition(unit.UnitCategoryId, unitCategoryRegistry),
            Role = ResolveDefinition(unit.RoleId, roleRegistry),
            JobProfiles = ResolveJobs(unit, jobRegistry),
        };

        var containers = new List<SerializedStatContainer>
        {
            unit.Stats,
            profiles.ArmorProfile?.Stats,
            profiles.DefenseProfile?.Stats,
            profiles.MovementProfile?.Stats,
            profiles.LocomotionProfile?.Stats,
            profiles.NeedsProfile?.Stats,
            profiles.MoodProfile?.Stats,
            profiles.BehaviourProfile?.Stats,
            profiles.PerceptionProfile?.Stats,
            profiles.ProductionProfile?.Stats,
        };

        for (int i = 0; i < profiles.Weapons.Count; i++)
            containers.Add(profiles.Weapons[i]?.Stats);

        for (int i = 0; i < profiles.JobProfiles.Count; i++)
            containers.Add(profiles.JobProfiles[i]?.Stats);

        return (profiles, containers);
    }

    private static string BuildSignature(UnitDefinition unit)
    {
        var sb = new StringBuilder(512);
        sb.Append(unit.Id).Append('|');
        sb.Append(unit.SchemaModeId).Append('|');
        sb.Append(unit.ArmorProfileId).Append('|');
        sb.Append(unit.DefenseProfileId).Append('|');
        sb.Append(unit.MovementProfileId).Append('|');
        sb.Append(unit.LocomotionProfileId).Append('|');
        sb.Append(unit.NeedsProfileId).Append('|');
        sb.Append(unit.MoodProfileId).Append('|');
        sb.Append(unit.AIBehaviorProfileId).Append('|');
        sb.Append(unit.PerceptionProfileId).Append('|');
        sb.Append(unit.ProductionProfileId).Append('|');
        sb.Append(unit.UnitCategoryId).Append('|');
        sb.Append(unit.RoleId).Append('|');
        AppendList(sb, unit.WeaponIds);
        AppendList(sb, unit.JobProfileIds);
        AppendList(sb, unit.JobIds);
        AppendStats(sb, unit.Stats);
        AppendModifiers(sb, unit.StatModifiers);
        return sb.ToString();
    }

    private static void AppendList(StringBuilder builder, IReadOnlyList<string> values)
    {
        builder.Append("[");
        if (values != null)
        {
            for (int i = 0; i < values.Count; i++)
                builder.Append(values[i]).Append(';');
        }

        builder.Append(']');
    }

    private static void AppendStats(StringBuilder builder, SerializedStatContainer stats)
    {
        builder.Append("{stats:");
        if (stats?.Entries != null)
        {
            for (int i = 0; i < stats.Entries.Count; i++)
                builder.Append(stats.Entries[i].StatId).Append('=').Append(stats.Entries[i].Value).Append(';');
        }

        builder.Append('}');
    }

    private static void AppendModifiers(StringBuilder builder, IReadOnlyList<StatModifier> modifiers)
    {
        builder.Append("{mods:");
        if (modifiers != null)
        {
            for (int i = 0; i < modifiers.Count; i++)
                builder.Append(modifiers[i].targetStatId).Append('=').Append(modifiers[i].operation).Append(':').Append(modifiers[i].value).Append(';');
        }

        builder.Append('}');
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

    private static IEnumerable<StatModifier> CollectModifiers(UnitDefinition unit, ResolvedProfiles profiles)
    {
        if (unit?.StatModifiers != null)
        {
            for (int i = 0; i < unit.StatModifiers.Count; i++)
                yield return unit.StatModifiers[i];
        }

        if (profiles.ArmorProfile?.StatModifiers != null)
        {
            for (int i = 0; i < profiles.ArmorProfile.StatModifiers.Count; i++)
                yield return profiles.ArmorProfile.StatModifiers[i];
        }

        if (profiles.DefenseProfile?.StatModifiers != null)
        {
            for (int i = 0; i < profiles.DefenseProfile.StatModifiers.Count; i++)
                yield return profiles.DefenseProfile.StatModifiers[i];
        }

        if (profiles.MovementProfile?.StatModifiers != null)
        {
            for (int i = 0; i < profiles.MovementProfile.StatModifiers.Count; i++)
                yield return profiles.MovementProfile.StatModifiers[i];
        }

        if (profiles.LocomotionProfile?.StatModifiers != null)
        {
            for (int i = 0; i < profiles.LocomotionProfile.StatModifiers.Count; i++)
                yield return profiles.LocomotionProfile.StatModifiers[i];
        }

        if (profiles.NeedsProfile?.StatModifiers != null)
        {
            for (int i = 0; i < profiles.NeedsProfile.StatModifiers.Count; i++)
                yield return profiles.NeedsProfile.StatModifiers[i];
        }

        if (profiles.MoodProfile?.StatModifiers != null)
        {
            for (int i = 0; i < profiles.MoodProfile.StatModifiers.Count; i++)
                yield return profiles.MoodProfile.StatModifiers[i];
        }

        if (profiles.BehaviourProfile?.StatModifiers != null)
        {
            for (int i = 0; i < profiles.BehaviourProfile.StatModifiers.Count; i++)
                yield return profiles.BehaviourProfile.StatModifiers[i];
        }

        if (profiles.PerceptionProfile?.StatModifiers != null)
        {
            for (int i = 0; i < profiles.PerceptionProfile.StatModifiers.Count; i++)
                yield return profiles.PerceptionProfile.StatModifiers[i];
        }

        if (profiles.ProductionProfile?.StatModifiers != null)
        {
            for (int i = 0; i < profiles.ProductionProfile.StatModifiers.Count; i++)
                yield return profiles.ProductionProfile.StatModifiers[i];
        }

        if (profiles.Role?.StatModifiers != null)
        {
            for (int i = 0; i < profiles.Role.StatModifiers.Count; i++)
                yield return profiles.Role.StatModifiers[i];
        }

        if (profiles.Weapons != null)
        {
            for (int i = 0; i < profiles.Weapons.Count; i++)
            {
                var weapon = profiles.Weapons[i];
                if (weapon?.StatModifiers == null)
                    continue;

                for (int m = 0; m < weapon.StatModifiers.Count; m++)
                    yield return weapon.StatModifiers[m];
            }
        }

        if (profiles.JobProfiles == null)
            yield break;

        for (int i = 0; i < profiles.JobProfiles.Count; i++)
        {
            var job = profiles.JobProfiles[i];
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
