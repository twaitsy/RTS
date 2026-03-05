#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class UnitProfileDefinitionMigrationUtility
{
    [MenuItem("Tools/Data/Migrate Unit Canonical Profile Definitions")]
    public static void MigrateCanonicalUnitProfiles()
    {
        var migrationSpecs = new[]
        {
            new MigrationSpec(typeof(ArmorTypeDefinition), typeof(ArmorProfileDefinition)),
            new MigrationSpec(typeof(DamageTableDefinition), typeof(DefenseProfileDefinition)),
            new MigrationSpec(typeof(TerrainTypeDefinition), typeof(MovementProfileDefinition)),
            new MigrationSpec(typeof(AnimationDefinition), typeof(LocomotionProfileDefinition)),
            new MigrationSpec(typeof(ProductionDefinition), typeof(ProductionProfileDefinition)),
            new MigrationSpec(typeof(CivilianNeedsProfile), typeof(NeedsProfileDefinition)),
            new MigrationSpec(typeof(BuildingCategoryDefinition), typeof(UnitCategoryDefinition)),
        };

        var created = 0;
        var skipped = 0;

        foreach (var spec in migrationSpecs)
        {
            var guids = AssetDatabase.FindAssets($"t:{spec.SourceType.Name}");
            foreach (var guid in guids)
            {
                var sourcePath = AssetDatabase.GUIDToAssetPath(guid);
                var source = AssetDatabase.LoadAssetAtPath(sourcePath, spec.SourceType) as ScriptableObject;
                if (source == null)
                    continue;

                var targetPath = BuildTargetPath(sourcePath, spec.SourceType.Name, spec.TargetType.Name);
                var existing = AssetDatabase.LoadAssetAtPath(targetPath, spec.TargetType);
                if (existing != null)
                {
                    skipped++;
                    continue;
                }

                var target = ScriptableObject.CreateInstance(spec.TargetType);
                CopySharedSerializedFields(source, target);

                AssetDatabase.CreateAsset(target, targetPath);
                created++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[UnitProfileDefinitionMigration] Complete. Created {created} canonical assets. Skipped {skipped} existing targets.");
        EditorUtility.DisplayDialog("Unit Canonical Profile Migration", $"Created {created} canonical assets. Skipped {skipped} existing targets.", "OK");
    }

    private static string BuildTargetPath(string sourcePath, string sourceTypeName, string targetTypeName)
    {
        var directory = Path.GetDirectoryName(sourcePath)?.Replace('\\', '/') ?? "Assets";
        var baseName = Path.GetFileNameWithoutExtension(sourcePath);

        if (baseName.EndsWith(sourceTypeName, StringComparison.Ordinal))
            baseName = baseName[..^sourceTypeName.Length] + targetTypeName;

        var candidatePath = $"{directory}/{baseName}.asset";
        return AssetDatabase.GenerateUniqueAssetPath(candidatePath);
    }

    private static void CopySharedSerializedFields(ScriptableObject source, ScriptableObject target)
    {
        var json = EditorJsonUtility.ToJson(source);
        EditorJsonUtility.FromJsonOverwrite(json, target);
        EditorUtility.SetDirty(target);
    }

    private readonly struct MigrationSpec
    {
        public MigrationSpec(Type sourceType, Type targetType)
        {
            SourceType = sourceType;
            TargetType = targetType;
        }

        public Type SourceType { get; }
        public Type TargetType { get; }
    }
}
#endif
