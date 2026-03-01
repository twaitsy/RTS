using UnityEditor;

public static class SerializedPropertyExtensions
{
    public static SerializedProperty GetParentArray(this SerializedProperty property)
    {
        var path = property.propertyPath;
        var last = path.LastIndexOf('.');
        var arrayPath = path.Substring(0, last);
        return property.serializedObject.FindProperty(arrayPath);
    }

    public static int GetIndexInArray(this SerializedProperty property)
    {
        var path = property.propertyPath;
        int start = path.LastIndexOf('[') + 1;
        int end = path.LastIndexOf(']');
        var number = path.Substring(start, end - start);
        return int.Parse(number);
    }
}