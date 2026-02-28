using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class DefinitionDependencyGraphExporter
{
    public static DefinitionDependencyGraph BuildGraph(DefinitionReferenceMap referenceMap)
    {
        if (referenceMap == null)
            return DefinitionDependencyGraph.Empty;

        var nodes = referenceMap.GetAllDefinitions()
            .Select(node => new DefinitionDependencyNode(node.DefinitionType, node.DefinitionId))
            .OrderBy(node => node.Type)
            .ThenBy(node => node.Id)
            .ToList();

        var edges = referenceMap.GetAllEdges()
            .Select(edge => new DefinitionDependencyEdge(edge.SourceType, edge.SourceId, edge.Field, edge.TargetType, edge.TargetId))
            .OrderBy(edge => edge.SourceType)
            .ThenBy(edge => edge.SourceId)
            .ThenBy(edge => edge.Field)
            .ThenBy(edge => edge.TargetType)
            .ThenBy(edge => edge.TargetId)
            .ToList();

        return new DefinitionDependencyGraph(nodes, edges);
    }

    public static string ExportJson(DefinitionReferenceMap referenceMap, bool prettyPrint = true)
    {
        var graph = BuildGraph(referenceMap);
        return JsonUtility.ToJson(graph, prettyPrint);
    }

    public static void ExportToAsset(DefinitionReferenceMap referenceMap, DefinitionDependencyGraphAsset asset)
    {
        if (asset == null)
            return;

        var graph = BuildGraph(referenceMap);
        asset.SetData(graph);
    }
}

[Serializable]
public sealed class DefinitionDependencyGraph
{
    [SerializeField] private List<DefinitionDependencyNode> nodes;
    [SerializeField] private List<DefinitionDependencyEdge> edges;

    public static DefinitionDependencyGraph Empty => new(new List<DefinitionDependencyNode>(), new List<DefinitionDependencyEdge>());

    public DefinitionDependencyGraph(List<DefinitionDependencyNode> nodes, List<DefinitionDependencyEdge> edges)
    {
        this.nodes = nodes ?? new List<DefinitionDependencyNode>();
        this.edges = edges ?? new List<DefinitionDependencyEdge>();
    }

    public IReadOnlyList<DefinitionDependencyNode> Nodes => nodes;
    public IReadOnlyList<DefinitionDependencyEdge> Edges => edges;
}

[Serializable]
public struct DefinitionDependencyNode
{
    [SerializeField] private string type;
    [SerializeField] private string id;

    public DefinitionDependencyNode(string type, string id)
    {
        this.type = type;
        this.id = id;
    }

    public string Type => type;
    public string Id => id;
}

[Serializable]
public struct DefinitionDependencyEdge
{
    [SerializeField] private string sourceType;
    [SerializeField] private string sourceId;
    [SerializeField] private string field;
    [SerializeField] private string targetType;
    [SerializeField] private string targetId;

    public DefinitionDependencyEdge(string sourceType, string sourceId, string field, string targetType, string targetId)
    {
        this.sourceType = sourceType;
        this.sourceId = sourceId;
        this.field = field;
        this.targetType = targetType;
        this.targetId = targetId;
    }

    public string SourceType => sourceType;
    public string SourceId => sourceId;
    public string Field => field;
    public string TargetType => targetType;
    public string TargetId => targetId;
}

[CreateAssetMenu(menuName = "DataDrivenRTS/Debug/Definition Dependency Graph")]
public sealed class DefinitionDependencyGraphAsset : ScriptableObject
{
    [SerializeField] private List<DefinitionDependencyNode> nodes = new();
    [SerializeField] private List<DefinitionDependencyEdge> edges = new();

    public IReadOnlyList<DefinitionDependencyNode> Nodes => nodes;
    public IReadOnlyList<DefinitionDependencyEdge> Edges => edges;

    public void SetData(DefinitionDependencyGraph graph)
    {
        nodes = graph?.Nodes?.ToList() ?? new List<DefinitionDependencyNode>();
        edges = graph?.Edges?.ToList() ?? new List<DefinitionDependencyEdge>();
    }
}
