using TarasK8.UI.Animations;
using UnityEditor;
using UnityEngine;

namespace TarasK8.UI.Editor.Animations
{
    [CustomPropertyDrawer(typeof(Easing))]
    public class EasingPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //base.OnGUI(position, property, label);
            EditorGUI.BeginProperty(position, label, property);

            var postion = EditorGUI.PrefixLabel(position, label);

            var curveProperty = property.FindPropertyRelative("_customCurve");
            var typeProperty = property.FindPropertyRelative("_type");

            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            if (typeProperty.enumValueIndex == 1)
            {
                float spacing = 3f;
                float typeWidth = 100f;
                Rect curvePosition = new Rect(postion.x, postion.y, postion.width - typeWidth, postion.height);
                Rect typePosition = new Rect(curvePosition.max.x + spacing, postion.y, typeWidth - spacing, postion.height);
                EditorGUI.PropertyField(curvePosition, curveProperty, GUIContent.none);
                EditorGUI.PropertyField(typePosition, typeProperty, GUIContent.none);
            }
            else
            {
                Rect typePosition = new Rect(postion.x, postion.y, postion.width, postion.height);
                EditorGUI.PropertyField(typePosition, typeProperty, GUIContent.none);
            }

            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }
    }
}