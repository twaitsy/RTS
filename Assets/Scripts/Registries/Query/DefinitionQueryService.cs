using System;
using System.Collections.Generic;
using System.Linq;

public sealed class DefinitionQueryService
{
    private readonly UnitRegistry unitRegistry;
    private readonly BuildingRegistry buildingRegistry;
    private readonly TechRegistry techRegistry;
    private readonly ProductionRegistry productionRegistry;
    private readonly DefinitionReferenceMap referenceMap;

    public DefinitionQueryService(
        UnitRegistry unitRegistry,
        BuildingRegistry buildingRegistry,
        TechRegistry techRegistry,
        ProductionRegistry productionRegistry,
        DefinitionReferenceMap referenceMap)
    {
        this.unitRegistry = unitRegistry;
        this.buildingRegistry = buildingRegistry;
        this.techRegistry = techRegistry;
        this.productionRegistry = productionRegistry;
        this.referenceMap = referenceMap;
    }

    public IReadOnlyList<UnitDefinition> FindUnitsWithTag(string tag)
    {
        if (unitRegistry == null || string.IsNullOrWhiteSpace(tag))
            return Array.Empty<UnitDefinition>();

        var normalizedTag = tag.Trim();
        return unitRegistry
            .GetDefinitions()
            .Where(definition => DefinitionQueryMetadataMatcher.HasTag(definition, normalizedTag))
            .ToList();
    }

    public IReadOnlyList<BuildingDefinition> FindBuildingsProducing(string resourceId)
    {
        if (buildingRegistry == null || string.IsNullOrWhiteSpace(resourceId))
            return Array.Empty<BuildingDefinition>();

        var normalizedResourceId = resourceId.Trim();
        var resultById = new Dictionary<string, BuildingDefinition>(StringComparer.Ordinal);

        foreach (var building in buildingRegistry.GetDefinitions())
        {
            if (building == null || string.IsNullOrWhiteSpace(building.Id))
                continue;

            if (DefinitionQueryMetadataMatcher.HasTag(building, $"produces:{normalizedResourceId}")
                || DefinitionQueryMetadataMatcher.HasTag(building, $"resource:{normalizedResourceId}"))
            {
                resultById[building.Id] = building;
            }
        }

        if (referenceMap == null)
            return resultById.Values.ToList();

        foreach (var inbound in referenceMap.GetIncoming(nameof(ResourceRegistry), normalizedResourceId))
        {
            if (StringComparer.Ordinal.Equals(inbound.SourceType, nameof(BuildingRegistry))
                && buildingRegistry.TryGet(inbound.SourceId, out var building))
            {
                resultById[building.Id] = building;
                continue;
            }

            if (!StringComparer.Ordinal.Equals(inbound.SourceType, nameof(ProductionRegistry)))
                continue;

            if (productionRegistry == null || !productionRegistry.TryGet(inbound.SourceId, out var production))
                continue;

            if (string.IsNullOrWhiteSpace(production.BuildingId))
                continue;

            if (!IsLikelyProductionOutputField(inbound.Field))
                continue;

            if (buildingRegistry.TryGet(production.BuildingId, out var producerBuilding))
                resultById[producerBuilding.Id] = producerBuilding;
        }

        return resultById.Values.ToList();
    }

    public IReadOnlyList<TechDefinition> FindTechsAffectingStat(string statId)
    {
        if (techRegistry == null || string.IsNullOrWhiteSpace(statId))
            return Array.Empty<TechDefinition>();

        var normalizedStatId = statId.Trim();
        var result = new Dictionary<string, TechDefinition>(StringComparer.Ordinal);

        foreach (var tech in techRegistry.GetDefinitions())
        {
            if (tech == null || string.IsNullOrWhiteSpace(tech.Id))
                continue;

            var affectsByInlineStat = tech.Stats?.Entries != null
                && tech.Stats.Entries.Any(stat => StringComparer.Ordinal.Equals(stat.StatId, normalizedStatId));

            var affectsByInlineModifier = tech.StatModifiers != null
                && tech.StatModifiers.Any(modifier => StringComparer.Ordinal.Equals(modifier.targetStatId, normalizedStatId));

            var affectsByMetadata = DefinitionQueryMetadataMatcher.HasTag(tech, $"affects:{normalizedStatId}")
                                    || DefinitionQueryMetadataMatcher.HasTag(tech, $"stat:{normalizedStatId}");

            if (affectsByInlineStat || affectsByInlineModifier || affectsByMetadata)
                result[tech.Id] = tech;
        }

        if (referenceMap == null)
            return result.Values.ToList();

        foreach (var inbound in referenceMap.GetIncoming(nameof(StatRegistry), normalizedStatId))
        {
            if (StringComparer.Ordinal.Equals(inbound.SourceType, nameof(TechRegistry))
                && techRegistry.TryGet(inbound.SourceId, out var directTech))
            {
                result[directTech.Id] = directTech;
                continue;
            }

            if (!StringComparer.Ordinal.Equals(inbound.SourceType, nameof(StatModifierRegistry)))
                continue;

            foreach (var modifierInbound in referenceMap.GetIncoming(nameof(StatModifierRegistry), inbound.SourceId))
            {
                if (!StringComparer.Ordinal.Equals(modifierInbound.SourceType, nameof(TechRegistry)))
                    continue;

                if (techRegistry.TryGet(modifierInbound.SourceId, out var linkedTech))
                    result[linkedTech.Id] = linkedTech;
            }
        }

        return result.Values.ToList();
    }

    private static bool IsLikelyProductionOutputField(string field)
    {
        if (string.IsNullOrWhiteSpace(field))
            return false;

        return field.IndexOf("output", StringComparison.OrdinalIgnoreCase) >= 0
               || field.IndexOf("produce", StringComparison.OrdinalIgnoreCase) >= 0
               || field.IndexOf("yield", StringComparison.OrdinalIgnoreCase) >= 0;
    }
}

internal static class DefinitionQueryMetadataMatcher
{
    public static bool HasTag(object definition, string tag)
    {
        if (definition is not IDefinitionMetadataProvider provider
            || provider.Metadata?.Tags == null
            || string.IsNullOrWhiteSpace(tag))
            return false;

        return provider.Metadata.Tags.Any(existingTag =>
            StringComparer.OrdinalIgnoreCase.Equals(existingTag?.Trim(), tag.Trim()));
    }
}
