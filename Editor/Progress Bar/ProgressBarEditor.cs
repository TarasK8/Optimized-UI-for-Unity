using System;
using UnityEditor;
using UnityEngine;

namespace TarasK8.UI.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ProgressBar), true)]
    public class ProgressBarEditor : ProgressBarBaseEditor
    {
        private SerializedProperty _bar;
        private SerializedProperty _direction;
        private SerializedProperty _center;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            
            _bar = serializedObject.FindProperty("_fillBar");
            _direction = serializedObject.FindProperty("_direction");
            _center = serializedObject.FindProperty("_center");
            
            string[] excludes =
            {
                _bar.propertyPath,
                _direction.propertyPath,
                _center.propertyPath,
            };
            AddToExcludeList(excludes);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.Space();
            
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
            
            serializedObject.ApplyModifiedProperties();
            
            base.OnInspectorGUI();
        }
    }
}