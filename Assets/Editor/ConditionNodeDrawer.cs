using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ConditionNode), true)]
public class ConditionNodeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        if (property.managedReferenceValue == null)
        {
            if (GUILayout.Button("Create Condition Node"))
                property.managedReferenceValue = new ConditionNode();

            EditorGUI.EndProperty();
            return;
        }

        property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, label);

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;

            DrawSafe(property, "op");
            DrawSafe(property, "leafType");
            DrawSafe(property, "floatValue");
            DrawSafe(property, "stringValue");
            DrawSafe(property, "referencedCondition");

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
                        child.managedReferenceValue = new ConditionNode();

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
                childrenProp.GetArrayElementAtIndex(childrenProp.arraySize - 1).managedReferenceValue = new ConditionNode();
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