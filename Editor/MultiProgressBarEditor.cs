using UnityEditor;
using UnityEngine;

namespace TarasK8.UI.Editor
{
    [CustomEditor(typeof(MultiProgressBar))]
    public class MultiProgressBarEditor : UnityEditor.Editor
    {
        private SerializedProperty _bars;
        private SerializedProperty _ignoreLenght;
        private SerializedProperty _reversed;

        private SerializedProperty _minValueMode;
        private SerializedProperty _minValue;
        private SerializedProperty _maxValueMode;
        private SerializedProperty _maxValue;
        private SerializedProperty _value;

        private MultiProgressBar _target;

        private void OnEnable()
        {
            _target = (MultiProgressBar)serializedObject.targetObject;

            _ignoreLenght = serializedObject.FindProperty("_ignoreLenght");
            _reversed = serializedObject.FindProperty("_reversed");
            _bars = serializedObject.FindProperty("_bars");
            
            _minValue = serializedObject.FindProperty("_minValue");
            _maxValue = serializedObject.FindProperty("_maxValue");
            _value = serializedObject.FindProperty("_value");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.LabelField("Visual", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_ignoreLenght);
            EditorGUILayout.PropertyField(_reversed);
            EditorGUILayout.PropertyField(_bars);
            DrawButtons();

            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Value", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_minValue);
            EditorGUILayout.PropertyField(_maxValue);
            EditorGUILayout.Slider(_value, _minValue.floatValue, _maxValue.floatValue);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawButtons()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Sort by X"))
                _target.SortListByX();
            
            if (GUILayout.Button("Sort by Y"))
                _target.SortListByY();
            
            if (GUILayout.Button("Reverse"))
                _target.ReverseList();
            
            EditorGUILayout.EndHorizontal();
        }
    }
}