#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class StatFieldMigrationUtility
{
    public readonly struct LegacyFieldLiftResult
    {
        public LegacyFieldLiftResult(int updatedAssets, int liftedFields)
        {
            UpdatedAssets = updatedAssets;
            LiftedFields = liftedFields;
        }

        public int UpdatedAssets { get; }
        public int LiftedFields { get; }

        public static LegacyFieldLiftResult operator +(LegacyFieldLiftResult left, LegacyFieldLiftResult right)
        {
            return new LegacyFieldLiftResult(left.UpdatedAssets + right.UpdatedAssets, left.LiftedFields + right.LiftedFields);
        }
    }

    public static LegacyFieldLiftResult RunLegacyFieldLiftPhase()
    {
        LegacyFieldLiftResult result = default;

        result += MigrateUnits();
        result += MigrateCivilians();
        result += MigrateWeaponTypes();
        result += MigrateWeapons();

        return result;
    }

    [System.Obsolete("Deprecated menu path removed. Use Tools/Validation/Migrate Legacy Stat IDs.")]
    public static LegacyFieldLiftResult MigrateLegacyStatsToCanonicalEntriesDeprecated()
    {
        return RunLegacyFieldLiftPhase();
    }

    private static LegacyFieldLiftResult MigrateUnits()
    {
        int updates = 0;
        int liftedFields = 0;
        string[] guids = AssetDatabase.FindAssets("t:UnitDefinition");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var obj = AssetDatabase.LoadAssetAtPath<UnitDefinition>(path);
            if (obj == null) continue;

            var so = new SerializedObject(obj);
            var stats = FindStatsEntriesProperty(so);
            var modifiers = FindModifiersProperty(so);

            int changes = 0;
            changes += TryMigrateLegacyStat(so, stats, "maxHealth", CanonicalStatIds.Core.MaxHealth);
            changes += TryMigrateLegacyStat(so, stats, "moveSpeed", CanonicalStatIds.Movement.MoveSpeed);
            changes += TryMigrateLegacyStat(so, stats, "turnSpeed", CanonicalStatIds.Movement.TurnSpeed);
            changes += TryMigrateLegacyStat(so, stats, "visionRange", CanonicalStatIds.Core.VisionRange);
            changes += TryMigrateLegacyStat(so, stats, "workSpeed", CanonicalStatIds.Production.WorkSpeed);
            changes += TryMigrateLegacyStat(so, stats, "carryCapacity", CanonicalStatIds.Production.CarryCapacity);

            changes += TryMigrateLegacyOverride(so, modifiers, "baseDamage", CanonicalStatIds.Combat.BaseDamage);
            changes += TryMigrateLegacyOverride(so, modifiers, "attackSpeed", CanonicalStatIds.Combat.AttackSpeed);
            changes += TryMigrateLegacyOverride(so, modifiers, "attackRange", CanonicalStatIds.Combat.AttackRange);

            if (changes == 0) continue;

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(obj);
            updates++;
            liftedFields += changes;
        }

        return new LegacyFieldLiftResult(updates, liftedFields);
    }

    private static LegacyFieldLiftResult MigrateCivilians()
    {
        int updates = 0;
        int liftedFields = 0;
        string[] guids = AssetDatabase.FindAssets("t:CivilianDefinition");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var obj = AssetDatabase.LoadAssetAtPath<CivilianDefinition>(path);
            if (obj == null) continue;

            var so = new SerializedObject(obj);
            var stats = FindStatsEntriesProperty(so);

            int changes = 0;
            changes += TryMigrateLegacyStat(so, stats, "moveSpeed", CanonicalStatIds.Movement.MoveSpeed);
            changes += TryMigrateLegacyStat(so, stats, "workSpeed", CanonicalStatIds.Production.WorkSpeed);

            if (changes == 0) continue;

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(obj);
            updates++;
            liftedFields += changes;
        }

        return new LegacyFieldLiftResult(updates, liftedFields);
    }

    private static LegacyFieldLiftResult MigrateWeaponTypes()
    {
        int updates = 0;
        int liftedFields = 0;
        string[] guids = AssetDatabase.FindAssets("t:WeaponTypeDefinition");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var obj = AssetDatabase.LoadAssetAtPath<WeaponTypeDefinition>(path);
            if (obj == null) continue;

            var so = new SerializedObject(obj);
            var modifiers = FindModifiersProperty(so);

            int changes = 0;
            changes += TryMigrateLegacyOverride(so, modifiers, "baseDamage", CanonicalStatIds.Combat.BaseDamage);
            changes += TryMigrateLegacyOverride(so, modifiers, "attackSpeed", CanonicalStatIds.Combat.AttackSpeed);

            if (changes == 0) continue;

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(obj);
            updates++;
            liftedFields += changes;
        }

        return new LegacyFieldLiftResult(updates, liftedFields);
    }

    private static LegacyFieldLiftResult MigrateWeapons()
    {
        int updates = 0;
        int liftedFields = 0;
        string[] guids = AssetDatabase.FindAssets("t:WeaponDefinition");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var obj = AssetDatabase.LoadAssetAtPath<WeaponDefinition>(path);
            if (obj == null) continue;

            var so = new SerializedObject(obj);
            var modifiers = FindModifiersProperty(so);

            int changes = 0;
            changes += TryMigrateLegacyOverride(so, modifiers, "baseDamage", CanonicalStatIds.Combat.BaseDamage);
            changes += TryMigrateLegacyOverride(so, modifiers, "attackSpeed", CanonicalStatIds.Combat.AttackSpeed);

            if (changes == 0) continue;

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(obj);
            updates++;
            liftedFields += changes;
        }

        return new LegacyFieldLiftResult(updates, liftedFields);
    }

    private static SerializedProperty FindStatsEntriesProperty(SerializedObject serializedObject)
    {
        var statsContainer = serializedObject.FindProperty("stats");
        if (statsContainer != null)
        {
            var entries = statsContainer.FindPropertyRelative("entries");
            if (entries != null)
                return entries;
        }

        return serializedObject.FindProperty("baseStats");
    }

    private static SerializedProperty FindModifiersProperty(SerializedObject serializedObject)
    {
        return serializedObject.FindProperty("statModifiers")
            ?? serializedObject.FindProperty("equipmentStatModifiers");
    }

    private static int TryMigrateLegacyStat(SerializedObject source, SerializedProperty targetEntries, string legacyFieldName, string statId)
    {
        var legacyProperty = source.FindProperty(legacyFieldName);
        if (legacyProperty == null || targetEntries == null)
            return 0;

        return AddStatEntryIfMissing(targetEntries, statId, legacyProperty.floatValue) ? 1 : 0;
    }

    private static int TryMigrateLegacyOverride(SerializedObject source, SerializedProperty modifiers, string legacyFieldName, string statId)
    {
        var legacyProperty = source.FindProperty(legacyFieldName);
        if (legacyProperty == null || modifiers == null)
            return 0;

        return AddOverrideModifierIfMissing(modifiers, statId, legacyProperty.floatValue) ? 1 : 0;
    }

    private static bool AddStatEntryIfMissing(SerializedProperty arrayProp, string statId, float value)
    {
        if (HasStatEntry(arrayProp, statId)) return false;

        int index = arrayProp.arraySize;
        arrayProp.InsertArrayElementAtIndex(index);
        SerializedProperty entry = arrayProp.GetArrayElementAtIndex(index);
        entry.FindPropertyRelative("statId").stringValue = statId;
        entry.FindPropertyRelative("value").floatValue = value;
        return true;
    }

    private static bool AddOverrideModifierIfMissing(SerializedProperty arrayProp, string statId, float value)
    {
        if (HasOverrideModifier(arrayProp, statId)) return false;

        int index = arrayProp.arraySize;
        arrayProp.InsertArrayElementAtIndex(index);
        SerializedProperty entry = arrayProp.GetArrayElementAtIndex(index);
        entry.FindPropertyRelative("targetStatId").stringValue = statId;
        entry.FindPropertyRelative("value").floatValue = value;
        entry.FindPropertyRelative("operation").enumValueIndex = (int)StatOperation.Override;
        return true;
    }

    private static bool HasStatEntry(SerializedProperty arrayProp, string statId)
    {
        for (int i = 0; i < arrayProp.arraySize; i++)
        {
            SerializedProperty entry = arrayProp.GetArrayElementAtIndex(i);
            if (entry.FindPropertyRelative("statId").stringValue == statId)
                return true;
        }

        return false;
    }

    private static bool HasOverrideModifier(SerializedProperty arrayProp, string statId)
    {
        for (int i = 0; i < arrayProp.arraySize; i++)
        {
            SerializedProperty entry = arrayProp.GetArrayElementAtIndex(i);
            if (entry.FindPropertyRelative("targetStatId").stringValue != statId)
                continue;

            if (entry.FindPropertyRelative("operation").enumValueIndex == (int)StatOperation.Override)
                return true;
        }

        return false;
    }
}
#endif
