using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TarasK8.UI.Editor
{
    [CustomEditor(typeof(ProgressBarBase), true)]
    public class ProgressBarBaseEditor : UnityEditor.Editor
    {
        private SerializedProperty _script;
        private SerializedProperty _minValue;
        private SerializedProperty _maxValue;
        private SerializedProperty _value;
        private string[] _propertyPathToExcludeForChildClasses;

        protected virtual void OnEnable()
        {
            _script = serializedObject.FindProperty("m_Script");
            _minValue = serializedObject.FindProperty("_minValue");
            _maxValue = serializedObject.FindProperty("_maxValue");
            _value = serializedObject.FindProperty("_value");

            _propertyPathToExcludeForChildClasses = new[]
            {
                _script.propertyPath,
                _minValue.propertyPath,
                _maxValue.propertyPath,
                _value.propertyPath
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            DrawPropertiesExcluding(serializedObject, _propertyPathToExcludeForChildClasses);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Value", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_minValue);
            EditorGUILayout.PropertyField(_maxValue);
            EditorGUILayout.Slider(_value, _minValue.floatValue, _maxValue.floatValue);
            
            serializedObject.ApplyModifiedProperties();
        }

        protected void AddToExcludeList(string[] propertyPathToExcludeForChildClasses)
        {
            _propertyPathToExcludeForChildClasses = _propertyPathToExcludeForChildClasses.Concat(propertyPathToExcludeForChildClasses).ToArray();
        }
    }
}
