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
            var baseStats = FindStatsEntriesProperty(so);
            var equipmentMods = so.FindProperty("equipmentStatModifiers");

            bool changed = false;
            changed |= AddStatEntryIfMissing(baseStats, CanonicalStatIds.MaxHealth, so.FindProperty("maxHealth").floatValue);
            changed |= AddStatEntryIfMissing(baseStats, CanonicalStatIds.MoveSpeed, so.FindProperty("moveSpeed").floatValue);
            changed |= AddStatEntryIfMissing(baseStats, CanonicalStatIds.TurnSpeed, so.FindProperty("turnSpeed").floatValue);
            changed |= AddStatEntryIfMissing(baseStats, CanonicalStatIds.VisionRange, so.FindProperty("visionRange").floatValue);
            changed |= AddStatEntryIfMissing(baseStats, CanonicalStatIds.WorkSpeed, so.FindProperty("workSpeed").floatValue);
            changed |= AddStatEntryIfMissing(baseStats, CanonicalStatIds.CarryCapacity, so.FindProperty("carryCapacity").floatValue);

            changed |= AddOverrideModifierIfMissing(equipmentMods, CanonicalStatIds.BaseDamage, so.FindProperty("baseDamage").floatValue);
            changed |= AddOverrideModifierIfMissing(equipmentMods, CanonicalStatIds.AttackSpeed, so.FindProperty("attackSpeed").floatValue);
            changed |= AddOverrideModifierIfMissing(equipmentMods, CanonicalStatIds.AttackRange, so.FindProperty("attackRange").floatValue);

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
            var baseStats = FindStatsEntriesProperty(so);
            bool changed = false;
            changed |= AddStatEntryIfMissing(baseStats, CanonicalStatIds.MoveSpeed, so.FindProperty("moveSpeed").floatValue);
            changed |= AddStatEntryIfMissing(baseStats, CanonicalStatIds.WorkSpeed, so.FindProperty("workSpeed").floatValue);

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
        string[] guids = AssetDatabase.FindAssets("t:WeaponTypeDefinition");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var obj = AssetDatabase.LoadAssetAtPath<WeaponTypeDefinition>(path);
            if (obj == null) continue;

            var so = new SerializedObject(obj);
            var modifiers = so.FindProperty("statModifiers");

            bool changed = false;
            changed |= AddOverrideModifierIfMissing(modifiers, CanonicalStatIds.BaseDamage, so.FindProperty("baseDamage").floatValue);
            changed |= AddOverrideModifierIfMissing(modifiers, CanonicalStatIds.AttackSpeed, so.FindProperty("attackSpeed").floatValue);

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
