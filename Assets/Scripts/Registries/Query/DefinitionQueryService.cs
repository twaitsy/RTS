using System;
using System.Collections.Generic;
using System.Linq;

public sealed class DefinitionQueryService
{
    private readonly UnitRegistry unitRegistry;
    private readonly BuildingRegistry buildingRegistry;
    private readonly TechRegistry techRegistry;
    private readonly ProductionRegistry productionRegistry;


    public DefinitionQueryService(
        UnitRegistry unitRegistry,
        BuildingRegistry buildingRegistry,
        TechRegistry techRegistry,
        ProductionRegistry productionRegistry
    )
    {
        this.unitRegistry = unitRegistry;
        this.buildingRegistry = buildingRegistry;
        this.techRegistry = techRegistry;
        this.productionRegistry = productionRegistry;

    }

    // ---------------------------------------------------------
    // UNIT TAG QUERIES
    // ---------------------------------------------------------

    public IReadOnlyList<UnitDefinition> FindUnitsWithTag(string tag)
    {
        if (unitRegistry == null || string.IsNullOrWhiteSpace(tag))
            return Array.Empty<UnitDefinition>();

        var normalizedTag = tag.Trim();

        return unitRegistry
            .GetDefinitions()
            .Where(def => DefinitionQueryMetadataMatcher.HasTag(def, normalizedTag))
            .ToList();
    }

    // ---------------------------------------------------------
    // BUILDINGS PRODUCING RESOURCE
    // ---------------------------------------------------------

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

            if (DefinitionQueryMetadataMatcher.HasTag(building, $"produces:{normalizedResourceId}") ||
                DefinitionQueryMetadataMatcher.HasTag(building, $"resource:{normalizedResourceId}"))
            {
                resultById[building.Id] = building;
            }
        }


        return resultById.Values.ToList();
    }

    // ---------------------------------------------------------
    // TECHS AFFECTING STAT
    // ---------------------------------------------------------

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

            bool affectsByInlineStat =
                tech.Stats?.Entries != null &&
                tech.Stats.Entries.Any(stat => StringComparer.Ordinal.Equals(stat.StatId, normalizedStatId));

            bool affectsByInlineModifier =
                tech.StatModifiers != null &&
                tech.StatModifiers.Any(mod => StringComparer.Ordinal.Equals(mod.targetStatId, normalizedStatId));

            bool affectsByMetadata =
                DefinitionQueryMetadataMatcher.HasTag(tech, $"affects:{normalizedStatId}") ||
                DefinitionQueryMetadataMatcher.HasTag(tech, $"stat:{normalizedStatId}");

            if (affectsByInlineStat || affectsByInlineModifier || affectsByMetadata)
                result[tech.Id] = tech;
        }


        return result.Values.ToList();
    }

    // ---------------------------------------------------------
    // HELPERS
    // ---------------------------------------------------------
}

internal static class DefinitionQueryMetadataMatcher
{
    public static bool HasTag(object definition, string tag)
    {
        if (definition is not IDefinitionMetadataProvider provider ||
            provider.Metadata?.Tags == null ||
            string.IsNullOrWhiteSpace(tag))
            return false;

        return provider.Metadata.Tags.Any(existingTag =>
            StringComparer.OrdinalIgnoreCase.Equals(existingTag?.Trim(), tag.Trim()));
    }
}