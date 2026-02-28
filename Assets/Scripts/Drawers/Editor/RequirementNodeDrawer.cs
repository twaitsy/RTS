using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(RequirementNode), true)]
public class RequirementNodeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        if (property.managedReferenceValue == null)
        {
            var createRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            if (GUI.Button(createRect, "Create RequirementNode"))
                property.managedReferenceValue = new RequirementNode();

            EditorGUI.EndProperty();
            return;
        }

        var foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

        if (!property.isExpanded)
        {
            EditorGUI.EndProperty();
            return;
        }

        EditorGUI.indentLevel++;

        float y = foldoutRect.yMax + EditorGUIUtility.standardVerticalSpacing;
        var op = property.FindPropertyRelative("op");
        y = DrawField(position, y, op);

        if ((RequirementOperator)op.enumValueIndex == RequirementOperator.Leaf)
        {
            y = DrawField(position, y, property.FindPropertyRelative("leafType"));
            y = DrawField(position, y, property.FindPropertyRelative("leafData"));
            y = DrawField(position, y, property.FindPropertyRelative("referencedRequirement"));
        }
        else
        {
            y = DrawChildren(position, y, property.FindPropertyRelative("children"));
        }

        EditorGUI.indentLevel--;
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight;
        if (property.managedReferenceValue == null)
            return height;
        if (!property.isExpanded)
            return height;

        var op = property.FindPropertyRelative("op");
        height += EditorGUIUtility.standardVerticalSpacing + EditorGUI.GetPropertyHeight(op, true);

        if ((RequirementOperator)op.enumValueIndex == RequirementOperator.Leaf)
        {
            height += EditorGUIUtility.standardVerticalSpacing + EditorGUI.GetPropertyHeight(property.FindPropertyRelative("leafType"), true);
            height += EditorGUIUtility.standardVerticalSpacing + EditorGUI.GetPropertyHeight(property.FindPropertyRelative("leafData"), true);
            height += EditorGUIUtility.standardVerticalSpacing + EditorGUI.GetPropertyHeight(property.FindPropertyRelative("referencedRequirement"), true);
        }
        else
        {
            var children = property.FindPropertyRelative("children");
            height += EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;
            for (int i = 0; i < children.arraySize; i++)
            {
                var child = children.GetArrayElementAtIndex(i);
                height += EditorGUIUtility.standardVerticalSpacing + EditorGUI.GetPropertyHeight(child, true);
            }
            height += EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;
        }

        return height;
    }

    private static float DrawField(Rect totalRect, float y, SerializedProperty property)
    {
        float h = EditorGUI.GetPropertyHeight(property, true);
        var r = new Rect(totalRect.x, y, totalRect.width, h);
        EditorGUI.PropertyField(r, property, true);
        return r.yMax + EditorGUIUtility.standardVerticalSpacing;
    }

    private static float DrawChildren(Rect totalRect, float y, SerializedProperty children)
    {
        var header = new Rect(totalRect.x, y, totalRect.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(header, "Children", EditorStyles.boldLabel);
        y = header.yMax + EditorGUIUtility.standardVerticalSpacing;

        for (int i = 0; i < children.arraySize; i++)
        {
            var child = children.GetArrayElementAtIndex(i);
            float h = EditorGUI.GetPropertyHeight(child, true);
            var childRect = new Rect(totalRect.x, y, totalRect.width - 24f, h);
            EditorGUI.PropertyField(childRect, child, new GUIContent($"Child {i}"), true);

            var removeRect = new Rect(totalRect.x + totalRect.width - 20f, y, 20f, EditorGUIUtility.singleLineHeight);
            if (GUI.Button(removeRect, "X"))
            {
                children.DeleteArrayElementAtIndex(i);
                break;
            }

            y = childRect.yMax + EditorGUIUtility.standardVerticalSpacing;
        }

        var addRect = new Rect(totalRect.x, y, totalRect.width, EditorGUIUtility.singleLineHeight);
        if (GUI.Button(addRect, "Add Child"))
        {
            int index = children.arraySize;
            children.InsertArrayElementAtIndex(index);
            var child = children.GetArrayElementAtIndex(index);
            child.managedReferenceValue = new RequirementNode();
        }

        return addRect.yMax + EditorGUIUtility.standardVerticalSpacing;
    }
}
