using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializedStatContainer
{
    [SerializeField] private List<StatEntry> entries = new();
    public IReadOnlyList<StatEntry> Entries => entries;

    public bool TryGetValue(string statId, out float value)
    {
        for (int i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            if (!string.Equals(entry.StatId, statId, StringComparison.Ordinal))
                continue;

            value = entry.Value;
            return true;
        }

        value = default;
        return false;
    }

    public IEnumerable<string> FindDuplicateStatIds()
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var duplicates = new HashSet<string>(StringComparer.Ordinal);

        for (int i = 0; i < entries.Count; i++)
        {
            var statId = entries[i].StatId;
            if (string.IsNullOrWhiteSpace(statId))
                continue;

            if (!seen.Add(statId))
                duplicates.Add(statId);
        }

        return duplicates;
    }
}
