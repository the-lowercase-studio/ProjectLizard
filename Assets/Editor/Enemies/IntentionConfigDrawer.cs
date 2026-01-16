using Assets.Enemies.Base.Intentions;
using UnityEditor;
using UnityEngine;

namespace Assets.Editor.Enemies
{
    [CustomPropertyDrawer(typeof(IntentionConfig))]
    public class IntentionConfigDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            var typeRect = new Rect(position.x, position.y, 80, position.height);
            var probabilityRect = new Rect(position.x + 85, position.y, 60, position.height);
            var actionRect = new Rect(position.x + 150, position.y, position.width - 150, position.height);

            var intentionTypeProp = property.FindPropertyRelative("_intentionType");
            var probabilityProp = property.FindPropertyRelative("_probability");
            var actionProp = property.FindPropertyRelative("_action");

            EditorGUI.PropertyField(typeRect, intentionTypeProp, GUIContent.none);
            EditorGUI.PropertyField(probabilityRect, probabilityProp, GUIContent.none);
            EditorGUI.PropertyField(actionRect, actionProp, GUIContent.none);

            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }
}
