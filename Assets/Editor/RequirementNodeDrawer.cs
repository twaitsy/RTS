using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(RequirementNode))]
public class RequirementNodeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // If the reference is null, show a create button
        if (property.managedReferenceValue == null)
        {
            if (GUILayout.Button("Create Requirement Node"))
            {
                property.managedReferenceValue = new RequirementNode();
            }

            EditorGUI.EndProperty();
            return;
        }

        // Foldout
        property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, label);

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;

            // Draw simple fields safely
            DrawSafe(property, "op");
            DrawSafe(property, "leafType");
            DrawSafe(property, "stringValue");
            DrawSafe(property, "intValue");
            DrawSafe(property, "referencedRequirement");

            // Children
            var childrenProp = property.FindPropertyRelative("children");

            EditorGUILayout.LabelField("Children", EditorStyles.boldLabel);

            for (int i = 0; i < childrenProp.arraySize; i++)
            {
                var child = childrenProp.GetArrayElementAtIndex(i);

                EditorGUILayout.BeginHorizontal();

                if (child.managedReferenceValue == null)
                {
                    EditorGUILayout.LabelField($"Child {i}");

                    if (GUILayout.Button("Create"))
                        child.managedReferenceValue = new RequirementNode();

                    if (GUILayout.Button("X", GUILayout.Width(20)))
                        childrenProp.DeleteArrayElementAtIndex(i);
                }
                else
                {
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.PropertyField(child, new GUIContent($"Child {i}"), true);
                    continue;
                }

                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Add Child"))
            {
                childrenProp.arraySize++;
                childrenProp.GetArrayElementAtIndex(childrenProp.arraySize - 1).managedReferenceValue = new RequirementNode();
            }

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    private void DrawSafe(SerializedProperty parent, string name)
    {
        var prop = parent.FindPropertyRelative(name);
        if (prop != null)
            EditorGUILayout.PropertyField(prop);
    }
}