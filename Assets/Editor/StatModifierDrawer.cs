#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(StatModifier))]
public class StatModifierDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var statId = property.FindPropertyRelative("statId");
        var value = property.FindPropertyRelative("value");
        var op = property.FindPropertyRelative("operation");

        float w = position.width;
        float h = EditorGUIUtility.singleLineHeight;

        // Layout: [statId][value][op][X]
        var rStat = new Rect(position.x, position.y, w * 0.45f, h);
        var rValue = new Rect(rStat.xMax + 4, position.y, w * 0.25f, h);
        var rOp = new Rect(rValue.xMax + 4, position.y, w * 0.20f, h);
        var rRemove = new Rect(rOp.xMax + 4, position.y, w * 0.08f, h);

        EditorGUI.PropertyField(rStat, statId, GUIContent.none);
        EditorGUI.PropertyField(rValue, value, GUIContent.none);
        EditorGUI.PropertyField(rOp, op, GUIContent.none);

        if (GUI.Button(rRemove, "X"))
        {
            var list = property.GetParentArray();
            int index = property.GetIndexInArray();
            list.DeleteArrayElementAtIndex(index);
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight + 4;
    }
}
#endif