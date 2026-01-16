using Assets.Enemies.Intentions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Assets.Editor.Enemies
{
    [CustomPropertyDrawer(typeof(IntentionConfig))]
    public class IntentionConfigPropertyDrawer : PropertyDrawer
    {
        private const float LineHeight = 18f;
        private const float Spacing = 2f;
        private Dictionary<string, IntentionType> _lastIntentionTypes = new Dictionary<string, IntentionType>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Draw foldout
            property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, LineHeight), property.isExpanded, label, true);

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                float yOffset = position.y + LineHeight + Spacing;

                // Draw IntentionType
                var intentionTypeProp = property.FindPropertyRelative("_intentionType");

                // Store the old value before drawing
                IntentionType oldIntentionType = (IntentionType)intentionTypeProp.enumValueIndex;

                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(new Rect(position.x, yOffset, position.width, LineHeight), intentionTypeProp);

                // Check if intention type changed
                if (EditorGUI.EndChangeCheck())
                {
                    IntentionType newIntentionType = (IntentionType)intentionTypeProp.enumValueIndex;

                    // Clear the action when intention type changes
                    var actionProperty = property.FindPropertyRelative("_action");
                    if (oldIntentionType != newIntentionType && !string.IsNullOrEmpty(actionProperty.managedReferenceFullTypename))
                    {
                        actionProperty.managedReferenceValue = null;
                        property.serializedObject.ApplyModifiedProperties();
                    }
                }

                yOffset += LineHeight + Spacing;

                // Draw Probability
                var probabilityProp = property.FindPropertyRelative("_probability");
                EditorGUI.PropertyField(new Rect(position.x, yOffset, position.width, LineHeight), probabilityProp);
                yOffset += LineHeight + Spacing;

                // Draw Action with custom type selector
                var actionProp = property.FindPropertyRelative("_action");
                yOffset = DrawActionField(new Rect(position.x, yOffset, position.width, LineHeight), actionProp, intentionTypeProp, yOffset);

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        private float DrawActionField(Rect position, SerializedProperty actionProp, SerializedProperty intentionTypeProp, float currentY)
        {
            Rect labelRect = new Rect(position.x, currentY, EditorGUIUtility.labelWidth, position.height);
            Rect buttonRect = new Rect(position.x + EditorGUIUtility.labelWidth, currentY, position.width - EditorGUIUtility.labelWidth, position.height);

            EditorGUI.LabelField(labelRect, "Action");

            // Get current type and selected intention type
            string currentType = actionProp.managedReferenceFullTypename;
            string displayName = string.IsNullOrEmpty(currentType) ? "None" : GetShortTypeName(currentType);

            // Create dropdown button
            if (EditorGUI.DropdownButton(buttonRect, new GUIContent(displayName), FocusType.Keyboard))
            {
                IntentionType selectedIntentionType = (IntentionType)intentionTypeProp.enumValueIndex;
                ShowActionTypeMenu(actionProp, selectedIntentionType);
            }

            currentY += LineHeight + Spacing;

            // Draw action properties if one is selected
            if (!string.IsNullOrEmpty(currentType))
            {
                EditorGUI.indentLevel++;

                // Iterate through child properties and draw them
                SerializedProperty iterator = actionProp.Copy();
                SerializedProperty endProperty = iterator.GetEndProperty();
                bool enterChildren = true;

                while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, endProperty))
                {
                    float propertyHeight = EditorGUI.GetPropertyHeight(iterator, true);
                    EditorGUI.PropertyField(new Rect(position.x, currentY, position.width, propertyHeight), iterator, true);
                    currentY += propertyHeight + Spacing;
                    enterChildren = false;
                }

                EditorGUI.indentLevel--;
            }

            return currentY;
        }

        private void ShowActionTypeMenu(SerializedProperty property, IntentionType selectedIntentionType)
        {
            var menu = new GenericMenu();

            // Add "None" option
            menu.AddItem(new GUIContent("None"), false, () =>
            {
                property.managedReferenceValue = null;
                property.serializedObject.ApplyModifiedProperties();
            });

            menu.AddSeparator("");

            // Get all types that match the selected intention type
            var actionTypes = GetActionTypesForIntention(selectedIntentionType);

            if (actionTypes.Count == 0)
            {
                menu.AddDisabledItem(new GUIContent("No actions available for this intention type"));
            }
            else
            {
                foreach (var type in actionTypes)
                {
                    string typeName = type.Name;
                    menu.AddItem(new GUIContent(typeName), false, () =>
                    {
                        var instance = Activator.CreateInstance(type);
                        property.managedReferenceValue = instance;
                        property.serializedObject.ApplyModifiedProperties();
                    });
                }
            }

            menu.ShowAsContext();
        }

        private List<Type> GetActionTypesForIntention(IntentionType intentionType)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => !type.IsAbstract && !type.IsInterface && typeof(IEnemyAction).IsAssignableFrom(type))
                .Where(type => HasMatchingIntentionType(type, intentionType))
                .OrderBy(type => type.Name)
                .ToList();
        }

        private bool HasMatchingIntentionType(Type type, IntentionType intentionType)
        {
            var attribute = type.GetCustomAttributes(typeof(IntentionTypeAttribute), false)
                .Cast<IntentionTypeAttribute>()
                .FirstOrDefault();

            return attribute != null && attribute.IntentionType == intentionType;
        }

        private string GetShortTypeName(string fullTypeName)
        {
            if (string.IsNullOrEmpty(fullTypeName))
                return "None";

            // Format: "Assembly.Namespace.TypeName Assembly"
            int lastDot = fullTypeName.LastIndexOf('.');
            int spaceIndex = fullTypeName.IndexOf(' ');

            if (lastDot >= 0 && spaceIndex > lastDot)
            {
                return fullTypeName.Substring(lastDot + 1, spaceIndex - lastDot - 1);
            }

            return fullTypeName;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded)
                return LineHeight;

            float height = LineHeight + Spacing; // Foldout
            height += LineHeight + Spacing; // IntentionType
            height += LineHeight + Spacing; // Probability
            height += LineHeight + Spacing; // Action dropdown

            // Add height for action properties if action is set
            var actionProp = property.FindPropertyRelative("_action");
            if (!string.IsNullOrEmpty(actionProp.managedReferenceFullTypename))
            {
                SerializedProperty iterator = actionProp.Copy();
                SerializedProperty endProperty = iterator.GetEndProperty();
                bool enterChildren = true;

                while (iterator.NextVisible(enterChildren) && !SerializedProperty.EqualContents(iterator, endProperty))
                {
                    height += EditorGUI.GetPropertyHeight(iterator, true) + Spacing;
                    enterChildren = false;
                }
            }

            return height;
        }
    }
}
