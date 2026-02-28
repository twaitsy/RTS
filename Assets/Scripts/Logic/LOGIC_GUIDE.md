# Logic System Guide (Conditions, Requirements, Triggers)

This project uses data-driven ScriptableObject assets to define runtime logic:

- `ConditionDefinition`: runtime boolean checks (combat, weather, tags, timers)
- `RequirementDefinition`: unlock/build/tech constraints
- `TriggerDefinition`: event + optional condition + effects
- `ModifierGroupDefinition`: bundles of stat modifiers

## 1) Create a Condition

1. Create asset: `Create > DataDrivenRTS > Logic > Condition`
2. Open the root `Condition Tree` in inspector.
3. Set `op`:
   - `Leaf`: single check (`leafType`, `floatValue`, `stringValue`)
   - `And` / `Or`: add child nodes
   - `Not`: first child is negated
4. Optional: set `referencedCondition` on a leaf to reuse another condition asset.

### Common condition patterns

- **Health below threshold**
  - `op = Leaf`
  - `leafType = HealthBelow`
  - `floatValue = 25`
- **Night ambush condition**
  - Root `And`
  - Child A: `IsNight`
  - Child B: `HasTag` + `stringValue = enemy`

## 2) Create a Requirement

1. Create asset: `Create > DataDrivenRTS > Logic > Requirement`
2. Set root `op` (`Leaf`, `And`, `Or`).
3. For leaf nodes fill:
   - `leafType`
   - `leafData.targetId`
   - `leafData.value`
   - `leafData.comparison`
4. Optional: set `referencedRequirement` to reuse another requirement.

### Common requirement patterns

- **Has tech**
  - `leafType = HasTech`
  - `leafData.targetId = tech.bronze_working`
- **Resource threshold**
  - `leafType = HasResourceAmount`
  - `targetId = resource.wood`
  - `value = 200`
  - `comparison = GreaterThanOrEqual`

## 3) Create a Trigger

1. Create asset: `Create > DataDrivenRTS > Logic > Trigger`
2. Set:
   - `eventType` (example: `OnUnitDamaged`)
   - optional `condition`
   - `effects` list
   - `oneShot` / `cooldown`
   - optional `requiredTags`
3. Fire it from game code with:

```csharp
trigger.TryFire(target, conditionContext, effectContext);
```

## 4) Standard logic functions (recommended baseline)

Use `Assets/Scripts/Logic/StandardLogicFunctions.cs` and `Assets/Scripts/Logic/Examples/StandardLogicContextExamples.cs` as the default implementation layer.

- `LogicMath.Compare(...)`: one shared comparison helper
- `StandardConditionFunctions.Evaluate(...)`: central switch for leaf behavior
- `StandardRequirementFunctions.Evaluate(...)`: central switch for requirement leaves
- `StandardConditionContext` / `StandardRequirementContext`: easy dictionary-backed contexts

This keeps runtime systems simple: game code fills context values and logic assets evaluate themselves.

## 5) How to add new logic safely

When adding a new `ConditionLeafType` or `RequirementLeafType`:

1. Add enum value in `ConditionDefinition.cs` or `RequirementDefinition.cs`
2. Implement behavior in `StandardConditionFunctions` or `StandardRequirementFunctions`
3. Add any needed context keys (flag/value/text/id)
4. Create one example asset and test path in gameplay
5. Keep string keys centralized (constants in your gameplay module) to avoid typos

## 6) Drawer location and editor workflow

Custom drawers now live in:

- `Assets/Scripts/Drawers/Editor/ConditionNodeDrawer.cs`
- `Assets/Scripts/Drawers/Editor/RequirementNodeDrawer.cs`
- `Assets/Scripts/Drawers/Editor/StatModifierDrawer.cs`

The condition and requirement drawers are dynamic and context-aware:
- leaf nodes show leaf fields
- composite nodes show child controls
- inspectors stay compact and easier to edit
