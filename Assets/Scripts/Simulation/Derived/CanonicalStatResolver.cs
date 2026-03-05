using System;
using System.Collections.Generic;

public static class CanonicalStatResolver
{
    public static Dictionary<string, StatModifierRollup> BuildRollups(IEnumerable<StatModifier> modifiers)
    {
        var rollups = new Dictionary<string, StatModifierRollup>(StringComparer.Ordinal);
        if (modifiers == null)
            return rollups;

        foreach (var modifier in modifiers)
        {
            if (string.IsNullOrWhiteSpace(modifier.targetStatId))
                continue;

            if (!rollups.TryGetValue(modifier.targetStatId, out var rollup))
                rollup = StatModifierRollup.Identity(modifier.targetStatId);

            rollup = rollup.Apply(modifier);
            rollups[modifier.targetStatId] = rollup;
        }

        return rollups;
    }

    public static float ResolveStatValue(
        SerializedStatContainer stats,
        IEnumerable<StatModifier> modifiers,
        string statId,
        float defaultValue)
    {
        var rollups = BuildRollups(modifiers);
        var baseValue = GetBaseStatValue(stats, statId, defaultValue);

        if (rollups.TryGetValue(statId, out var rollup))
            return rollup.ApplyTo(baseValue);

        return baseValue;
    }

    public static float ResolveStatValue(
        IReadOnlyList<SerializedStatContainer> containers,
        IReadOnlyDictionary<string, StatModifierRollup> rollups,
        string statId,
        float defaultValue)
    {
        var baseValue = GetBaseStatValue(containers, statId, defaultValue);

        if (rollups != null && rollups.TryGetValue(statId, out var rollup))
            return rollup.ApplyTo(baseValue);

        return baseValue;
    }

    private static float GetBaseStatValue(IReadOnlyList<SerializedStatContainer> containers, string statId, float defaultValue)
    {
        if (containers == null || string.IsNullOrWhiteSpace(statId))
            return defaultValue;

        for (int i = containers.Count - 1; i >= 0; i--)
        {
            var container = containers[i];
            if (container == null)
                continue;

            if (container.TryGetValue(statId, out var value))
                return value;
        }

        return defaultValue;
    }

    private static float GetBaseStatValue(SerializedStatContainer stats, string statId, float defaultValue)
    {
        if (stats?.Entries == null || string.IsNullOrWhiteSpace(statId))
            return defaultValue;

        foreach (var stat in stats.Entries)
        {
            if (StringComparer.Ordinal.Equals(stat.StatId, statId))
                return stat.Value;
        }

        return defaultValue;
    }
}
