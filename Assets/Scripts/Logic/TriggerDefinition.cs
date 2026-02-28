using System;
using System.Collections.Generic;
using UnityEngine;

public enum TriggerEventType
{
    None,

    // Unit lifecycle
    OnUnitSpawned,
    OnUnitDeath,
    OnUnitDamaged,
    OnUnitHealed,
    OnUnitLevelUp,
    OnUnitStatusEffectApplied,
    OnUnitStatusEffectRemoved,

    // Combat
    OnCombatStarted,
    OnCombatEnded,
    OnEnemySpotted,
    OnEnemyLost,
    OnAllySpotted,
    OnAllyLost,
    OnLineOfSightGained,
    OnLineOfSightLost,

    // Buildings
    OnBuildingStarted,
    OnBuildingCompleted,
    OnBuildingDamaged,
    OnBuildingDestroyed,
    OnBuildingUpgraded,

    // Resources & economy
    OnResourceGathered,
    OnResourceDelivered,
    OnResourceConsumed,
    OnResourceDepleted,
    OnStorageFull,
    OnStorageAvailable,

    // Research & tech
    OnResearchStarted,
    OnResearchCompleted,

    // Time & environment
    OnDayStarted,
    OnNightStarted,
    OnSeasonChanged,
    OnWeatherChanged,
    OnTemperatureChanged,
    OnTimeOfDayChanged,
    OnBiomeChanged,

    // Zones & areas
    OnEntityEnteredZone,
    OnEntityExitedZone,
    OnZoneCaptured,
    OnZoneLost,

    // Squads / groups
    OnSquadCreated,
    OnSquadMemberAdded,
    OnSquadMemberRemoved,
    OnSquadDestroyed,

    // Factions
    OnFactionRelationChanged,

    // Timers & scripting
    OnTimerExpired,
    OnCustomEvent,
    OnConditionSatisfied,
    OnConditionFailed,

    // Global
    OnGameStarted,
    OnGamePaused,
    OnGameResumed,
    OnGameSaved,
    OnGameLoaded
}

[CreateAssetMenu(menuName = "DataDrivenRTS/Logic/Trigger")]
public class TriggerDefinition : ScriptableObject, IIdentifiable, IDefinitionMetadataProvider
{
    [SerializeField] private string id;
    public string Id => id;

    [SerializeField] private DefinitionMetadata metadata = DefinitionMetadata.Create(DefinitionCategory.Logic);
    public DefinitionMetadata Metadata => metadata;

    [SerializeField] private string displayName;
    public string DisplayName => displayName;

    [SerializeField] private Sprite icon;
    public Sprite Icon => icon;

    [Header("When this trigger fires")]
    public TriggerEventType eventType = TriggerEventType.None;

    [Header("Only apply effects if this condition is true")]
    public ConditionDefinition condition;

    [Header("Effects to apply when trigger fires")]
    public List<EffectDefinition> effects = new();

    [Header("Trigger behaviour")]
    [Tooltip("This trigger can only fire once (global or per-entity)")]
    public bool oneShot = false;

    [Tooltip("Minimum seconds between firings (0 = no cooldown)")]
    public float cooldown = 0f;

    [Header("Optional target filtering")]
    [Tooltip("Only entities with ANY of these tags will activate the trigger")]
    public List<string> requiredTags = new();

    // Runtime state (never saved)
    [NonSerialized] private readonly HashSet<object> oneShotFired = new();
    [NonSerialized] private readonly Dictionary<object, float> lastFiredTime = new();

    public bool TryFire(object target, IConditionContext conditionContext, IEffectContext effectContext)
    {
        if (eventType == TriggerEventType.None) return false;

        if (oneShot && oneShotFired.Contains(target ?? this))
            return false;

        if (cooldown > 0f)
        {
            float last = lastFiredTime.GetValueOrDefault(target ?? this, -999f);
            if (Time.time - last < cooldown)
                return false;
        }

        if (requiredTags.Count > 0)
        {
            if (target is not ITagHolder tagHolder)
                return false;

            bool matches = false;
            foreach (var tag in requiredTags)
            {
                if (tagHolder.HasTag(tag))
                {
                    matches = true;
                    break;
                }
            }
            if (!matches) return false;
        }

        if (condition != null && !condition.Evaluate(conditionContext))
            return false;

        foreach (var effect in effects)
        {
            if (effect != null)
                effect.Apply(effectContext, target);
        }

        if (oneShot)
            oneShotFired.Add(target ?? this);

        if (cooldown > 0f)
            lastFiredTime[target ?? this] = Time.time;

        return true;
    }

    public void ResetRuntimeState()
    {
        oneShotFired.Clear();
        lastFiredTime.Clear();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        DefinitionMetadataUtility.EnsureMetadata(ref metadata, DefinitionCategory.Logic);
    }
#endif
}