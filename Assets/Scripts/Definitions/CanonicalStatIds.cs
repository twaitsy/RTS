using System.Collections.Generic;

public static class CanonicalStatIds
{
    // Canonical IDs used by migration helpers and legacy field conversion.
    public const string MaxHealth = Core.MaxHealth;
    public const string MoveSpeed = Movement.MoveSpeed;
    public const string TurnSpeed = Movement.TurnSpeed;
    public const string VisionRange = Core.VisionRange;
    public const string WorkSpeed = Production.WorkSpeed;
    public const string CarryCapacity = Production.CarryCapacity;
    public const string BaseDamage = "combat.baseDamage";
    public const string AttackSpeed = "combat.attackSpeed";
    public const string AttackRange = "combat.attackRange";

    // Canonical stat catalog (domain.family.variant).
    public static class Core
    {
        public const string MaxHealth = "core.maxHealth";
        public const string HealthRegen = "core.healthRegen";
        public const string Stamina = "core.stamina";
        public const string StaminaRegen = "core.staminaRegen";
        public const string VisionRange = "core.visionRange";
        public const string HearingRange = "core.hearingRange";
        public const string SizeRadius = "core.sizeRadius";
        public const string Weight = "core.weight";
    }

    public static class Movement
    {
        public const string MoveSpeed = "movement.moveSpeed";
        public const string TurnSpeed = "movement.turnSpeed";
        public const string Acceleration = "movement.acceleration";
        public const string Deceleration = "movement.deceleration";
        public const string ClimbSpeed = "movement.climbSpeed";
        public const string SwimSpeed = "movement.swimSpeed";
        public const string FlightSpeed = "movement.flightSpeed";
        public const string PathfindingCostModifier = "movement.pathfindingCostModifier";
    }

    public static class Combat
    {
        public const string BaseDamage = "combat.baseDamage";
        public const string AttackSpeed = "combat.attackSpeed";
        public const string AttackRange = "combat.attackRange";
        public const string Accuracy = "combat.accuracy";
        public const string CritChance = "combat.critChance";
        public const string CritMultiplier = "combat.critMultiplier";
        public const string ReloadTime = "combat.reloadTime";
        public const string BurstCount = "combat.burstCount";
        public const string BurstInterval = "combat.burstInterval";
        public const string KnockbackForce = "combat.knockbackForce";
        public const string ThreatGeneration = "combat.threatGeneration";
        public const string AggroRange = "combat.aggroRange";

        public const string BaseDamageFire = "combat.baseDamage.fire";
        public const string BaseDamageCold = "combat.baseDamage.cold";
        public const string BaseDamagePoison = "combat.baseDamage.poison";
        public const string BaseDamageBleed = "combat.baseDamage.bleed";
        public const string BaseDamageBlunt = "combat.baseDamage.blunt";
        public const string BaseDamagePierce = "combat.baseDamage.pierce";
        public const string BaseDamageSlash = "combat.baseDamage.slash";
        public const string BaseDamageMagic = "combat.baseDamage.magic";
        public const string BaseDamageTrue = "combat.baseDamage.true";
    }

    public static class Defense
    {
        public const string ArmorValue = "defense.armorValue";
        public const string ArmorPenetrationResistance = "defense.armorPenetrationResistance";
        public const string BlockChance = "defense.blockChance";
        public const string DodgeChance = "defense.dodgeChance";
        public const string ParryChance = "defense.parryChance";
        public const string ShieldValue = "defense.shieldValue";
        public const string DamageReductionFlat = "defense.damageReductionFlat";
        public const string DamageReductionPercent = "defense.damageReductionPercent";
    }

    public static class Resistance
    {
        public const string FireFlat = "resistance.fire.flat";
        public const string FirePercent = "resistance.fire.percent";
        public const string ColdFlat = "resistance.cold.flat";
        public const string ColdPercent = "resistance.cold.percent";
        public const string PoisonFlat = "resistance.poison.flat";
        public const string PoisonPercent = "resistance.poison.percent";
        public const string ShockFlat = "resistance.shock.flat";
        public const string ShockPercent = "resistance.shock.percent";
        public const string BleedFlat = "resistance.bleed.flat";
        public const string BleedPercent = "resistance.bleed.percent";
        public const string BluntFlat = "resistance.blunt.flat";
        public const string PierceFlat = "resistance.pierce.flat";
        public const string SlashFlat = "resistance.slash.flat";
        public const string MagicPercent = "resistance.magic.percent";
        public const string TruePercent = "resistance.true.percent";
    }

    public static class Production
    {
        public const string WorkSpeed = "production.workSpeed";
        public const string CarryCapacity = "production.carryCapacity";
        public const string GatherSpeed = "production.gatherSpeed";
        public const string BuildSpeed = "production.buildSpeed";
        public const string RepairSpeed = "production.repairSpeed";
        public const string CraftSpeed = "production.craftSpeed";
        public const string HarvestYield = "production.harvestYield";
        public const string ProductionEfficiencyWood = "production.efficiency.wood";
        public const string ProductionEfficiencyStone = "production.efficiency.stone";
        public const string ProductionEfficiencyMetal = "production.efficiency.metal";
        public const string ProductionEfficiencyFood = "production.efficiency.food";
        public const string ProductionEfficiencyEnergy = "production.efficiency.energy";
        public const string StorageCapacity = "production.storageCapacity";
        public const string StorageAccessSpeed = "production.storageAccessSpeed";
    }

    public static class Economy
    {
        public const string ResourceIncomeRate = "economy.resourceIncomeRate";
        public const string ResourceConsumptionRate = "economy.resourceConsumptionRate";
        public const string ResourceCapacity = "economy.resourceCapacity";
        public const string ResourceDecayRate = "economy.resourceDecayRate";
        public const string ResourceRefineEfficiency = "economy.resourceRefineEfficiency";
        public const string ResourceTransportSpeed = "economy.resourceTransportSpeed";
    }

    public static class Needs
    {
        public const string HungerRateDay = "needs.hungerRate.day";
        public const string HungerRateNight = "needs.hungerRate.night";
        public const string HungerRateColdWeather = "needs.hungerRate.coldWeather";
        public const string HungerRateHeatWave = "needs.hungerRate.heatWave";
        public const string ThirstRate = "needs.thirstRate";
        public const string SleepRate = "needs.sleepRate";
        public const string HygieneRate = "needs.hygieneRate";
        public const string SocialNeedRate = "needs.socialNeedRate";
        public const string EntertainmentNeedRate = "needs.entertainmentNeedRate";
        public const string TemperatureComfortMin = "needs.temperatureComfortMin";
        public const string TemperatureComfortMax = "needs.temperatureComfortMax";
        public const string DiseaseResistance = "needs.diseaseResistance";
        public const string ImmunityRecoveryRate = "needs.immunityRecoveryRate";
        public const string StarvationDamageRate = "needs.starvationDamageRate";
        public const string DehydrationDamageRate = "needs.dehydrationDamageRate";
    }

    public static class Mood
    {
        public const string MoodBaseline = "mood.baseline";
        public const string MoodRecoveryRate = "mood.recoveryRate";
        public const string StressResistance = "mood.stressResistance";
        public const string StressGainRate = "mood.stressGainRate";
        public const string FearResistance = "mood.fearResistance";
        public const string Courage = "mood.courage";
        public const string Morale = "mood.morale";
        public const string Loyalty = "mood.loyalty";
        public const string PanicThreshold = "mood.panicThreshold";
        public const string Confidence = "mood.confidence";
    }

    public static class Skills
    {
        public const string XPGainRate = "skills.xpGainRate";
        public const string SkillLearningSpeed = "skills.learningSpeed";
        public const string LevelUpThreshold = "skills.levelUpThreshold";
        public const string MaxSkillLevel = "skills.maxSkillLevel";
        public const string SkillCooldownReduction = "skills.cooldownReduction";
        public const string SkillEfficiency = "skills.efficiency";
    }

    public static class AI
    {
        public const string PerceptionRadiusEnemy = "ai.perceptionRadius.enemy";
        public const string PerceptionRadiusAlly = "ai.perceptionRadius.ally";
        public const string PerceptionRadiusResource = "ai.perceptionRadius.resource";
        public const string PerceptionRadiusThreat = "ai.perceptionRadius.threat";
        public const string MemoryDurationCombat = "ai.memoryDuration.combat";
        public const string MemoryDurationSocial = "ai.memoryDuration.social";
        public const string MemoryDurationResource = "ai.memoryDuration.resource";
        public const string ReactionTime = "ai.reactionTime";
        public const string DecisionFrequency = "ai.decisionFrequency";
        public const string Aggression = "ai.aggression";
        public const string Caution = "ai.caution";
        public const string Curiosity = "ai.curiosity";
        public const string Obedience = "ai.obedience";
        public const string PatrolSpeedModifier = "ai.patrolSpeedModifier";
        public const string ChaseSpeedModifier = "ai.chaseSpeedModifier";
    }

    public static class Social
    {
        public const string FriendshipGainRate = "social.friendshipGainRate";
        public const string RomanceChance = "social.romanceChance";
        public const string RivalryChance = "social.rivalryChance";
        public const string CooperationLevel = "social.cooperationLevel";
        public const string PersuasionPower = "social.persuasionPower";
        public const string SocialInfluence = "social.socialInfluence";
    }

    public static class Building
    {
        public const string BuildTime = "building.buildTime";
        public const string ConstructionTime = "building.constructionTime";
        public const string HitPoints = "building.hitPoints";
        public const string ComfortBonus = "building.comfortBonus";
        public const string FireResistance = "building.fireResistance";
        public const string EnergyConsumption = "building.energyConsumption";
        public const string EnergyProduction = "building.energyProduction";
        public const string HousingCapacity = "building.housingCapacity";
        public const string UpgradeSlots = "building.upgradeSlots";
        public const string WorkerSlots = "building.workerSlots";
        public const string ProductionSlots = "building.productionSlots";
        public const string PollutionOutput = "building.pollutionOutput";
        public const string NoiseOutput = "building.noiseOutput";
        public const string MaintenanceCost = "building.maintenanceCost";
        public const string UpgradeCostMultiplier = "building.upgradeCostMultiplier";
    }

    public static class Weather
    {
        public const string TemperatureDay = "weather.temperature.day";
        public const string TemperatureNight = "weather.temperature.night";
        public const string TemperatureWinter = "weather.temperature.winter";
        public const string TemperatureSummer = "weather.temperature.summer";
        public const string Humidity = "weather.humidity";
        public const string WindSpeedStorm = "weather.windSpeed.storm";
        public const string WindSpeedBreeze = "weather.windSpeed.breeze";
        public const string VisibilityModifierFog = "weather.visibilityModifier.fog";
        public const string VisibilityModifierRain = "weather.visibilityModifier.rain";
        public const string MovementPenalty = "weather.movementPenalty";
        public const string CropGrowthModifier = "weather.cropGrowthModifier";
        public const string SolarPowerModifier = "weather.solarPowerModifier";
        public const string RainfallRate = "weather.rainfallRate";
        public const string StormIntensity = "weather.stormIntensity";
        public const string FogDensity = "weather.fogDensity";
    }

    public static class Biome
    {
        public const string ResourceSpawnRateFish = "biome.resourceSpawnRate.fish";
        public const string ResourceSpawnRateBerries = "biome.resourceSpawnRate.berries";
        public const string ResourceSpawnRateOre = "biome.resourceSpawnRate.ore";
        public const string WildlifeDensity = "biome.wildlifeDensity";
        public const string DangerLevel = "biome.dangerLevel";
        public const string FertilityForest = "biome.fertility.forest";
        public const string FertilityDesert = "biome.fertility.desert";
        public const string FertilityPlains = "biome.fertility.plains";
        public const string FertilitySwamp = "biome.fertility.swamp";
        public const string PollutionLevel = "biome.pollutionLevel";
        public const string TemperatureModifier = "biome.temperatureModifier";
        public const string WeatherFrequencyModifier = "biome.weatherFrequencyModifier";
        public const string ZoneControlValue = "biome.zoneControlValue";
        public const string ZoneDefenseBonus = "biome.zoneDefenseBonus";
    }

    public static class Item
    {
        public const string Durability = "item.durability";
        public const string DurabilityLossRate = "item.durabilityLossRate";
        public const string Weight = "item.weight";
        public const string EquipTime = "item.equipTime";
        public const string ToolEfficiency = "item.toolEfficiency";
        public const string WeaponDamage = "item.weaponDamage";
        public const string ArmorRating = "item.armorRating";
        public const string Warmth = "item.warmth";
        public const string StorageSize = "item.storageSize";
        public const string HandlingSpeed = "item.handlingSpeed";
    }

    public static class StatusEffects
    {
        public const string EffectDuration = "status.effectDuration";
        public const string EffectIntensity = "status.effectIntensity";
        public const string TickInterval = "status.tickInterval";
        public const string HealingPerTick = "status.healingPerTick";
        public const string DamagePerTick = "status.damagePerTick";
        public const string MovementPenaltyPercent = "status.movementPenaltyPercent";
        public const string AttackPenaltyPercent = "status.attackPenaltyPercent";
        public const string DefensePenaltyPercent = "status.defensePenaltyPercent";
        public const string SpeedBoostPercent = "status.speedBoostPercent";
        public const string PoisonStacks = "status.poisonStacks";
        public const string PoisonDamagePerTick = "status.poison.damagePerTick";
        public const string PoisonDuration = "status.poison.duration";
        public const string PoisonStacksMax = "status.poison.stacksMax";
        public const string BleedDamagePerTick = "status.bleed.damagePerTick";
        public const string BleedDuration = "status.bleed.duration";
        public const string StunDuration = "status.stun.duration";
        public const string SlowPercent = "status.slow.percent";
        public const string BurnDamagePerTick = "status.burn.damagePerTick";
        public const string BurnSpreadChance = "status.burn.spreadChance";
    }

    public static class Governance
    {
        public const string TaxRate = "governance.taxRate";
        public const string ProductionBonus = "governance.productionBonus";
        public const string MoraleBonus = "governance.moraleBonus";
        public const string CrimeRate = "governance.crimeRate";
        public const string LawEnforcementStrength = "governance.lawEnforcementStrength";
        public const string ImmigrationRate = "governance.immigrationRate";
        public const string FactionApproval = "governance.factionApproval";
        public const string PolicyCostReduction = "governance.policyCostReduction";
    }

    public static class Global
    {
        public const string PopulationCap = "global.populationCap";
        public const string FactionReputation = "global.factionReputation";
        public const string GlobalHappiness = "global.happiness";
        public const string GlobalTemperature = "global.temperature";
        public const string DifficultyModifier = "global.difficultyModifier";
        public const string GlobalResourceMultiplier = "global.resourceMultiplier";
    }

    public static IReadOnlyList<string> Catalog => _catalog;

    private static readonly string[] _catalog =
    {
        Core.MaxHealth, Core.HealthRegen, Core.Stamina, Core.StaminaRegen, Core.VisionRange, Core.HearingRange,
        Movement.MoveSpeed, Movement.TurnSpeed, Movement.Acceleration, Movement.Deceleration,
        Combat.BaseDamage, Combat.AttackSpeed, Combat.AttackRange, Combat.Accuracy, Combat.CritChance,
        Defense.ArmorValue, Defense.BlockChance, Defense.DodgeChance,
        Resistance.FireFlat, Resistance.FirePercent, Resistance.ColdFlat, Resistance.PoisonFlat,
        Production.WorkSpeed, Production.CarryCapacity, Production.GatherSpeed, Production.BuildSpeed,
        Economy.ResourceIncomeRate, Economy.ResourceConsumptionRate,
        Needs.HungerRateDay, Needs.ThirstRate, Needs.SleepRate,
        Mood.MoodBaseline, Mood.Morale,
        Skills.XPGainRate, Skills.SkillEfficiency,
        AI.PerceptionRadiusEnemy, AI.ReactionTime, AI.DecisionFrequency,
        Social.FriendshipGainRate, Social.SocialInfluence,
        Building.BuildTime, Building.ConstructionTime, Building.HitPoints, Building.ComfortBonus, Building.HousingCapacity, Building.UpgradeSlots,
        Weather.TemperatureDay, Weather.Humidity, Weather.WindSpeedBreeze,
        Biome.WildlifeDensity, Biome.FertilityForest,
        Item.Durability, Item.WeaponDamage,
        StatusEffects.EffectDuration, StatusEffects.DamagePerTick,
        Governance.TaxRate, Governance.ProductionBonus,
        Global.PopulationCap, Global.DifficultyModifier
    };
}
