#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SceneDropdownAttribute))]
public class SceneDropdownDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.String)
        {
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        string[] scenePaths = EditorBuildSettings.scenes.Select(s => s.path).ToArray();

        if (scenePaths.Length == 0)
        {
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        string[] sceneNames = scenePaths.Select(Path.GetFileNameWithoutExtension).ToArray();

        int currentIndex = System.Array.FindIndex(scenePaths, p =>
            p == property.stringValue || Path.ChangeExtension(p, null) == property.stringValue);

        EditorGUI.BeginProperty(position, label, property);
        int selectedIndex = EditorGUI.Popup(position, label.text, currentIndex, sceneNames);

        if (selectedIndex >= 0 && selectedIndex != currentIndex)
        {
            property.stringValue = scenePaths[selectedIndex];
        }

        EditorGUI.EndProperty();
    }
}
#endif