#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class StatFieldMigrationUtility
{
    [MenuItem("Tools/DataDrivenRTS/Migrate Legacy Stats To Canonical Entries")]
    public static void Migrate()
    {
        int migratedAssets = 0;

        migratedAssets += MigrateUnits();
        migratedAssets += MigrateCivilians();
        migratedAssets += MigrateWeaponTypes();
        migratedAssets += MigrateWeapons();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Legacy stat migration complete. Updated {migratedAssets} assets.");
    }

    private static int MigrateUnits()
    {
        int updates = 0;
        string[] guids = AssetDatabase.FindAssets("t:UnitDefinition");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var obj = AssetDatabase.LoadAssetAtPath<UnitDefinition>(path);
            if (obj == null) continue;

            var so = new SerializedObject(obj);
            var stats = FindStatsEntriesProperty(so);
            var modifiers = FindModifiersProperty(so);

            bool changed = false;
            changed |= TryMigrateLegacyStat(so, stats, "maxHealth", CanonicalStatIds.MaxHealth);
            changed |= TryMigrateLegacyStat(so, stats, "moveSpeed", CanonicalStatIds.MoveSpeed);
            changed |= TryMigrateLegacyStat(so, stats, "turnSpeed", CanonicalStatIds.TurnSpeed);
            changed |= TryMigrateLegacyStat(so, stats, "visionRange", CanonicalStatIds.VisionRange);
            changed |= TryMigrateLegacyStat(so, stats, "workSpeed", CanonicalStatIds.WorkSpeed);
            changed |= TryMigrateLegacyStat(so, stats, "carryCapacity", CanonicalStatIds.CarryCapacity);

            changed |= TryMigrateLegacyOverride(so, modifiers, "baseDamage", CanonicalStatIds.BaseDamage);
            changed |= TryMigrateLegacyOverride(so, modifiers, "attackSpeed", CanonicalStatIds.AttackSpeed);
            changed |= TryMigrateLegacyOverride(so, modifiers, "attackRange", CanonicalStatIds.AttackRange);

            if (!changed) continue;

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(obj);
            updates++;
        }

        return updates;
    }

    private static int MigrateCivilians()
    {
        int updates = 0;
        string[] guids = AssetDatabase.FindAssets("t:CivilianDefinition");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var obj = AssetDatabase.LoadAssetAtPath<CivilianDefinition>(path);
            if (obj == null) continue;

            var so = new SerializedObject(obj);
            var stats = FindStatsEntriesProperty(so);

            bool changed = false;
            changed |= TryMigrateLegacyStat(so, stats, "moveSpeed", CanonicalStatIds.MoveSpeed);
            changed |= TryMigrateLegacyStat(so, stats, "workSpeed", CanonicalStatIds.WorkSpeed);

            if (!changed) continue;

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(obj);
            updates++;
        }

        return updates;
    }

    private static int MigrateWeaponTypes()
    {
        int updates = 0;
        string[] guids = AssetDatabase.FindAssets("t:WeaponTypeDefinition");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var obj = AssetDatabase.LoadAssetAtPath<WeaponTypeDefinition>(path);
            if (obj == null) continue;

            var so = new SerializedObject(obj);
            var modifiers = FindModifiersProperty(so);

            bool changed = false;
            changed |= TryMigrateLegacyOverride(so, modifiers, "baseDamage", CanonicalStatIds.BaseDamage);
            changed |= TryMigrateLegacyOverride(so, modifiers, "attackSpeed", CanonicalStatIds.AttackSpeed);

            if (!changed) continue;

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(obj);
            updates++;
        }

        return updates;
    }

    private static int MigrateWeapons()
    {
        int updates = 0;
        string[] guids = AssetDatabase.FindAssets("t:WeaponDefinition");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var obj = AssetDatabase.LoadAssetAtPath<WeaponDefinition>(path);
            if (obj == null) continue;

            var so = new SerializedObject(obj);
            var modifiers = FindModifiersProperty(so);

            bool changed = false;
            changed |= TryMigrateLegacyOverride(so, modifiers, "baseDamage", CanonicalStatIds.BaseDamage);
            changed |= TryMigrateLegacyOverride(so, modifiers, "attackSpeed", CanonicalStatIds.AttackSpeed);

            if (!changed) continue;

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(obj);
            updates++;
        }

        return updates;
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

    private static bool TryMigrateLegacyStat(SerializedObject source, SerializedProperty targetEntries, string legacyFieldName, string statId)
    {
        var legacyProperty = source.FindProperty(legacyFieldName);
        if (legacyProperty == null || targetEntries == null)
            return false;

        return AddStatEntryIfMissing(targetEntries, statId, legacyProperty.floatValue);
    }

    private static bool TryMigrateLegacyOverride(SerializedObject source, SerializedProperty modifiers, string legacyFieldName, string statId)
    {
        var legacyProperty = source.FindProperty(legacyFieldName);
        if (legacyProperty == null || modifiers == null)
            return false;

        return AddOverrideModifierIfMissing(modifiers, statId, legacyProperty.floatValue);
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
