#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ShowIfAttribute))]
public class ShowIfDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
       
            ShowIfAttribute showIf = (ShowIfAttribute)attribute;
            SerializedProperty boolProp = property.serializedObject.FindProperty(showIf.boolName);

            if (boolProp != null && boolProp.propertyType == SerializedPropertyType.Boolean && boolProp.boolValue)
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
       
            ShowIfAttribute showIf = (ShowIfAttribute)attribute;
            SerializedProperty boolProp = property.serializedObject.FindProperty(showIf.boolName);

            if (boolProp != null && boolProp.propertyType == SerializedPropertyType.Boolean && boolProp.boolValue)
            {
                return EditorGUI.GetPropertyHeight(property, label, true);
            }

            return -EditorGUIUtility.standardVerticalSpacing;
        
       
    }

    private SerializedProperty GetConditionalProperty(SerializedProperty property, string boolName)
    {
        
            string path = property.propertyPath;
            string conditionPath = path.Replace(property.name, boolName);
            return property.serializedObject.FindProperty(conditionPath);
        
       
    }

    private bool ShouldShow(SerializedProperty boolProp)
    {
        
            return boolProp != null && boolProp.propertyType == SerializedPropertyType.Boolean && boolProp.boolValue;
        
    }
}
#endif