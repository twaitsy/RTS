using System;
using System.Collections.Generic;

public readonly struct DefinitionReference
{
    public DefinitionReference(string field, string targetType, string targetId)
    {
        Field = field;
        TargetType = targetType;
        TargetId = targetId;
    }

    public string Field { get; }
    public string TargetType { get; }
    public string TargetId { get; }
}

public readonly struct DefinitionInboundReference
{
    public DefinitionInboundReference(string sourceType, string sourceId, string field)
    {
        SourceType = sourceType;
        SourceId = sourceId;
        Field = field;
    }

    public string SourceType { get; }
    public string SourceId { get; }
    public string Field { get; }
}

public readonly struct MissingReference
{
    public MissingReference(string sourceType, string sourceId, string field, string targetType, string targetId)
    {
        SourceType = sourceType;
        SourceId = sourceId;
        Field = field;
        TargetType = targetType;
        TargetId = targetId;
    }

    public string SourceType { get; }
    public string SourceId { get; }
    public string Field { get; }
    public string TargetType { get; }
    public string TargetId { get; }
}

public sealed class DefinitionReferenceMap
{
    private readonly struct DefinitionNodeKey : IEquatable<DefinitionNodeKey>
    {
        public DefinitionNodeKey(string type, string id)
        {
            Type = type ?? string.Empty;
            Id = id ?? string.Empty;
        }

        public string Type { get; }
        public string Id { get; }

        public bool Equals(DefinitionNodeKey other)
        {
            return StringComparer.Ordinal.Equals(Type, other.Type)
                   && StringComparer.Ordinal.Equals(Id, other.Id);
        }

        public override bool Equals(object obj)
        {
            return obj is DefinitionNodeKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (StringComparer.Ordinal.GetHashCode(Type) * 397) ^ StringComparer.Ordinal.GetHashCode(Id);
            }
        }
    }

    private readonly Dictionary<DefinitionNodeKey, List<DefinitionReference>> outgoing = new();
    private readonly Dictionary<DefinitionNodeKey, List<DefinitionInboundReference>> incoming = new();
    private readonly HashSet<DefinitionNodeKey> knownDefinitions = new();

    public void AddDefinition(string definitionType, string definitionId)
    {
        if (string.IsNullOrWhiteSpace(definitionType) || string.IsNullOrWhiteSpace(definitionId))
            return;

        knownDefinitions.Add(new DefinitionNodeKey(definitionType, definitionId));
    }

    public void AddReference(string sourceType, string sourceId, string field, string targetType, string targetId)
    {
        if (string.IsNullOrWhiteSpace(sourceType)
            || string.IsNullOrWhiteSpace(sourceId)
            || string.IsNullOrWhiteSpace(field)
            || string.IsNullOrWhiteSpace(targetType)
            || string.IsNullOrWhiteSpace(targetId))
            return;

        var sourceKey = new DefinitionNodeKey(sourceType, sourceId);
        var targetKey = new DefinitionNodeKey(targetType, targetId);

        if (!outgoing.TryGetValue(sourceKey, out var outgoingRefs))
        {
            outgoingRefs = new List<DefinitionReference>();
            outgoing.Add(sourceKey, outgoingRefs);
        }

        outgoingRefs.Add(new DefinitionReference(field, targetType, targetId));

        if (!incoming.TryGetValue(targetKey, out var incomingRefs))
        {
            incomingRefs = new List<DefinitionInboundReference>();
            incoming.Add(targetKey, incomingRefs);
        }

        incomingRefs.Add(new DefinitionInboundReference(sourceType, sourceId, field));
    }

    public IReadOnlyList<DefinitionReference> GetOutgoing(string sourceType, string sourceId)
    {
        var key = new DefinitionNodeKey(sourceType, sourceId);
        return outgoing.TryGetValue(key, out var refs) ? refs : Array.Empty<DefinitionReference>();
    }

    public IReadOnlyList<DefinitionInboundReference> GetIncoming(string targetType, string targetId)
    {
        var key = new DefinitionNodeKey(targetType, targetId);
        return incoming.TryGetValue(key, out var refs) ? refs : Array.Empty<DefinitionInboundReference>();
    }

    public bool TryFindOrphans(out List<MissingReference> missingReferences, int maxCount = int.MaxValue)
    {
        missingReferences = new List<MissingReference>();

        foreach (var pair in outgoing)
        {
            foreach (var reference in pair.Value)
            {
                var targetKey = new DefinitionNodeKey(reference.TargetType, reference.TargetId);
                if (knownDefinitions.Contains(targetKey))
                    continue;

                missingReferences.Add(new MissingReference(pair.Key.Type, pair.Key.Id, reference.Field, reference.TargetType, reference.TargetId));

                if (missingReferences.Count >= maxCount)
                    return true;
            }
        }

        return missingReferences.Count > 0;
    }

    public bool CanDelete(string targetType, string targetId, out IReadOnlyList<DefinitionInboundReference> dependents)
    {
        var inbound = GetIncoming(targetType, targetId);
        dependents = inbound;
        return inbound.Count == 0;
    }

    public bool TryGetDependencyChain(string targetType, string targetId, out string chain, int maxDepth = 3)
    {
        chain = null;
        var visited = new HashSet<DefinitionNodeKey>();
        var currentKey = new DefinitionNodeKey(targetType, targetId);

        for (var depth = 0; depth < maxDepth; depth++)
        {
            if (!visited.Add(currentKey))
                break;

            if (!incoming.TryGetValue(currentKey, out var refs) || refs.Count == 0)
                break;

            var first = refs[0];
            chain = chain == null
                ? $"{first.SourceType}:{first.SourceId} ({first.Field})"
                : $"{first.SourceType}:{first.SourceId} ({first.Field}) <- {chain}";

            currentKey = new DefinitionNodeKey(first.SourceType, first.SourceId);
        }

        return !string.IsNullOrWhiteSpace(chain);
    }
}
