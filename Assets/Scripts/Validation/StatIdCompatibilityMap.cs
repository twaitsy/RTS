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
        ["Weight"] = CanonicalStatIds.Item.Weight,

        // Legacy separator + lowercase variants produced by older normalization rules.
        ["core-maxhealth"] = CanonicalStatIds.Core.MaxHealth,
        ["core.maxhealth"] = CanonicalStatIds.Core.MaxHealth,
        ["core_visionrange"] = CanonicalStatIds.Core.VisionRange,
        ["core-visionrange"] = CanonicalStatIds.Core.VisionRange,
        ["core.visionrange"] = CanonicalStatIds.Core.VisionRange,

        ["movement-movespeed"] = CanonicalStatIds.Movement.MoveSpeed,
        ["movement.movespeed"] = CanonicalStatIds.Movement.MoveSpeed,
        ["movement-turnspeed"] = CanonicalStatIds.Movement.TurnSpeed,
        ["movement.turnspeed"] = CanonicalStatIds.Movement.TurnSpeed,

        ["combat-attackrange"] = CanonicalStatIds.Combat.AttackRange,
        ["combat.attackrange"] = CanonicalStatIds.Combat.AttackRange,
        ["combat-attackspeed"] = CanonicalStatIds.Combat.AttackSpeed,
        ["combat.attackspeed"] = CanonicalStatIds.Combat.AttackSpeed,
        ["combat-basedamage"] = CanonicalStatIds.Combat.BaseDamage,
        ["combat.basedamage"] = CanonicalStatIds.Combat.BaseDamage,

        ["production-carrycapacity"] = CanonicalStatIds.Production.CarryCapacity,
        ["production.carrycapacity"] = CanonicalStatIds.Production.CarryCapacity,
        ["production-storagecapacity"] = CanonicalStatIds.Production.StorageCapacity,
        ["production.storagecapacity"] = CanonicalStatIds.Production.StorageCapacity,
        ["production-workspeed"] = CanonicalStatIds.Production.WorkSpeed,
        ["production.workspeed"] = CanonicalStatIds.Production.WorkSpeed,

        ["building-buildtime"] = CanonicalStatIds.Building.BuildTime,
        ["building.buildtime"] = CanonicalStatIds.Building.BuildTime,
        ["building-comfortbonus"] = CanonicalStatIds.Building.ComfortBonus,
        ["building.comfortbonus"] = CanonicalStatIds.Building.ComfortBonus,
        ["building-constructiontime"] = CanonicalStatIds.Building.ConstructionTime,
        ["building.constructiontime"] = CanonicalStatIds.Building.ConstructionTime,
        ["building-hitpoints"] = CanonicalStatIds.Building.HitPoints,
        ["building.hitpoints"] = CanonicalStatIds.Building.HitPoints,
        ["building-housingcapacity"] = CanonicalStatIds.Building.HousingCapacity,
        ["building.housingcapacity"] = CanonicalStatIds.Building.HousingCapacity,
        ["building-upgradeslots"] = CanonicalStatIds.Building.UpgradeSlots,
        ["building.upgradeslots"] = CanonicalStatIds.Building.UpgradeSlots,

        ["item-weight"] = CanonicalStatIds.Item.Weight,
        ["item.weight"] = CanonicalStatIds.Item.Weight
    };
}
