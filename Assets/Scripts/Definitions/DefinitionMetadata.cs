using System;
using System.Collections.Generic;
using UnityEngine;

public enum DefinitionCategory
{
    Uncategorized = 0,
    Unit,
    Building,
    Technology,
    Stat,
    Resource,
    Combat,
    AI,
    Economy,
    Production,
    World,
    Environment,
    Social,
    UI,
    Logic
}

[Serializable]
public class DefinitionMetadata
{
    [SerializeField] private DefinitionCategory category = DefinitionCategory.Uncategorized;
    [SerializeField] private List<string> tags = new();
    [SerializeField] private string version = "1.0.0";
    [SerializeField] private string schemaHash = "";
    [SerializeField] private string lastModifiedUtc = "";
    [SerializeField] private string author = "";

    public DefinitionCategory Category => category;
    public IReadOnlyList<string> Tags => tags;
    public string Version => version;
    public string SchemaHash => schemaHash;
    public string LastModifiedUtc => lastModifiedUtc;
    public string Author => author;

    public static DefinitionMetadata Create(DefinitionCategory defaultCategory)
    {
        var metadata = new DefinitionMetadata();
        metadata.EnsureInitialized(defaultCategory);
        return metadata;
    }

    public void EnsureInitialized(DefinitionCategory defaultCategory)
    {
        tags ??= new List<string>();

        if (category == DefinitionCategory.Uncategorized)
            category = defaultCategory;

        if (string.IsNullOrWhiteSpace(version))
            version = "1.0.0";

        schemaHash ??= string.Empty;
        author ??= string.Empty;

        if (string.IsNullOrWhiteSpace(lastModifiedUtc))
            lastModifiedUtc = DateTime.UtcNow.ToString("O");

        for (var i = tags.Count - 1; i >= 0; i--)
        {
            var normalizedTag = tags[i]?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedTag))
            {
                tags.RemoveAt(i);
                continue;
            }

            tags[i] = normalizedTag;
        }

        var unique = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var i = tags.Count - 1; i >= 0; i--)
        {
            if (!unique.Add(tags[i]))
                tags.RemoveAt(i);
        }
    }
}

public static class DefinitionMetadataUtility
{
    public static void EnsureMetadata(ref DefinitionMetadata metadata, DefinitionCategory defaultCategory)
    {
        metadata ??= DefinitionMetadata.Create(defaultCategory);
        metadata.EnsureInitialized(defaultCategory);
    }
}
