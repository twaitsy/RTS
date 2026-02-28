using System.Collections.Generic;

public static class StatIdCompatibilityMap
{
    public static IReadOnlyDictionary<string, string> LegacyToCanonical => _legacyToCanonical;

    private static readonly Dictionary<string, string> _legacyToCanonical = new(System.StringComparer.Ordinal)
    {
        ["combat.maxHealth"] = CanonicalStatIds.Core.MaxHealth,
        ["locomotion.moveSpeed"] = CanonicalStatIds.Movement.MoveSpeed,
        ["locomotion.turnSpeed"] = CanonicalStatIds.Movement.TurnSpeed,
        ["ai.visionRange"] = CanonicalStatIds.Core.VisionRange,
        ["economy.workSpeed"] = CanonicalStatIds.Production.WorkSpeed,
        ["economy.carryCapacity"] = CanonicalStatIds.Production.CarryCapacity,

        ["MaxHitPoints"] = CanonicalStatIds.Building.HitPoints,
        ["BuildTime"] = CanonicalStatIds.Building.BuildTime,
        ["ConstructionTime"] = CanonicalStatIds.Building.ConstructionTime,
        ["HousingCapacity"] = CanonicalStatIds.Building.HousingCapacity,
        ["ComfortBonus"] = CanonicalStatIds.Building.ComfortBonus,
        ["StorageCapacity"] = CanonicalStatIds.Production.StorageCapacity,
        ["UpgradeSlots"] = CanonicalStatIds.Building.UpgradeSlots,
        ["Weight"] = CanonicalStatIds.Item.Weight
    };
}
