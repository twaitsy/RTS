#if UNITY_EDITOR
using UnityEditor;

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
        return default;
    }

    [System.Obsolete("Deprecated menu path removed. Use Tools/Validation/Migrate Legacy Stat IDs.")]
    public static LegacyFieldLiftResult MigrateLegacyStatsToCanonicalEntriesDeprecated()
    {
        return RunLegacyFieldLiftPhase();
    }

    private static SerializedProperty FindStatsEntriesProperty(SerializedObject serializedObject)
    {
        var statsContainer = serializedObject.FindProperty("stats");
        return statsContainer?.FindPropertyRelative("entries");
    }

    private static SerializedProperty FindModifiersProperty(SerializedObject serializedObject)
    {
        return serializedObject.FindProperty("statModifiers");
    }
}
#endif
