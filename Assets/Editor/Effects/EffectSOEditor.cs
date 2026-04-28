using Assets.Effects.Base;
using UnityEditor;

namespace Assets.Editor.Effects
{
    [CustomEditor(typeof(EffectSO), true)]
    public class EffectSOEditor : UnityEditor.Editor
    {
        private const string SCRIPT_PROPERTY = "m_Script";
        private const string HAS_VISUALS_PROPERTY = "_hasVisuals";
        private const string SPRITE_PROPERTY = "<Sprite>k__BackingField";
        private const string INITIAL_EFFECT_ANIMATOR_PROPERTY = "<InitialEffectAnimator>k__BackingField";

        private static readonly UnityEngine.GUIContent _hasVisualsLabel = new("Has Visuals");

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty hasVisualsProperty = serializedObject.FindProperty(HAS_VISUALS_PROPERTY);
            bool shouldDrawVisualFields = hasVisualsProperty == null ||
                hasVisualsProperty.hasMultipleDifferentValues ||
                hasVisualsProperty.boolValue;

            SerializedProperty property = serializedObject.GetIterator();
            bool enterChildren = true;

            while (property.NextVisible(enterChildren))
            {
                enterChildren = false;

                if (property.name == SCRIPT_PROPERTY)
                {
                    using (new EditorGUI.DisabledScope(true))
                    {
                        EditorGUILayout.PropertyField(property, true);
                    }

                    continue;
                }

                if (property.name == HAS_VISUALS_PROPERTY)
                {
                    EditorGUILayout.PropertyField(property, _hasVisualsLabel, true);
                    continue;
                }

                if (!shouldDrawVisualFields &&
                    (property.name == SPRITE_PROPERTY || property.name == INITIAL_EFFECT_ANIMATOR_PROPERTY))
                {
                    continue;
                }

                EditorGUILayout.PropertyField(property, true);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
