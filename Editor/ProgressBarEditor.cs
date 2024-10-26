using System;
using UnityEditor;
using UnityEngine;

namespace TarasK8.UI.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ProgressBar))]
    public class ProgressBarEditor : UnityEditor.Editor
    {
        private SerializedProperty _bar;
        private SerializedProperty _direction;
        private SerializedProperty _center;

        private SerializedProperty _minValueMode;
        private SerializedProperty _minValue;
        private SerializedProperty _maxValueMode;
        private SerializedProperty _maxValue;
        private SerializedProperty _value;

        private ProgressBar _target;

        private void OnEnable()
        {
            _target = (ProgressBar)serializedObject.targetObject;

            _bar = serializedObject.FindProperty("_bar");
            _direction = serializedObject.FindProperty("_direction");
            _center = serializedObject.FindProperty("_center");

            _minValueMode = serializedObject.FindProperty("_minValueMode");
            _minValue = serializedObject.FindProperty("_minValue");
            _maxValueMode = serializedObject.FindProperty("_maxValueMode");
            _maxValue = serializedObject.FindProperty("_maxValue");
            _value = serializedObject.FindProperty("_value");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.LabelField("Visual", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_bar);
            EditorGUILayout.PropertyField(_direction);
            if (_direction.enumValueIndex == 2)
            {
                EditorGUI.indentLevel++;
                GUILayout.BeginHorizontal();

                EditorGUILayout.PropertyField(_center);
                if (GUILayout.Button("0.5", GUILayout.Width(35f)))
                    _center.floatValue = 0.5f;

                GUILayout.EndHorizontal();
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Value", EditorStyles.boldLabel);
            float minValue = _target.MinValue;
            float maxValue = _target.MaxValue;
            DrawRangeValueProperty(_minValue, _minValueMode, minValue);
            DrawRangeValueProperty(_maxValue, _maxValueMode, maxValue);
            EditorGUILayout.Slider(_value, minValue, maxValue);
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawRangeValueProperty(SerializedProperty property, SerializedProperty modeProperty, float value)
        {
            GUILayout.BeginHorizontal();

            if(modeProperty.enumValueIndex == 0)
            {
                EditorGUILayout.PropertyField(property);
            }
            else
            {
                GUI.enabled = false;
                EditorGUILayout.FloatField(new GUIContent(property.displayName), value);
                GUI.enabled = true;
            }
            EditorGUILayout.PropertyField(modeProperty, new GUIContent(string.Empty), GUILayout.Width(100f));

            GUILayout.EndHorizontal();
        }
    }
}