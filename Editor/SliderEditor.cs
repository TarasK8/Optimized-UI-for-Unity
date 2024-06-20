using TarasK8.UI;
using UnityEditor;
using UnityEngine;

namespace TarasK8.UIEditor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Slider))]
    public class SliderEditor : SelectableEditor
    {
        // Handle
        private SerializedProperty _handleRect;
        private SerializedProperty _fillRect;
        private SerializedProperty _direction;
        private SerializedProperty _magnetToCursor;
        // Value
        private SerializedProperty _minValue;
        private SerializedProperty _maxValue;
        private SerializedProperty _step;
        private SerializedProperty _curve;
        private SerializedProperty _value;
        // Display
        private SerializedProperty _text;
        private SerializedProperty _format;
        // Events
        private SerializedProperty _onValueChanged;

        protected override void OnEnable()
        {
            base.OnEnable();
            _handleRect = serializedObject.FindProperty(Slider.HANDLE_RECT_FIELD);
            _fillRect = serializedObject.FindProperty(Slider.FILL_RECT_FIELD);
            _direction = serializedObject.FindProperty(Slider.DIRECTION_FIELD);
            _magnetToCursor = serializedObject.FindProperty(Slider.MAGNET_TO_CURSOR_FIELD);
            _minValue = serializedObject.FindProperty(Slider.MIN_VALUE_FIELD);
            _maxValue = serializedObject.FindProperty(Slider.MAX_VALUE_FIELD);
            _step = serializedObject.FindProperty(Slider.STEP_FIELD);
            _curve = serializedObject.FindProperty(Slider.CURVE_FIELD);
            _value = serializedObject.FindProperty(Slider.VALUE_FIELD);
            _text = serializedObject.FindProperty(Slider.TEXT_FIELD);
            _format = serializedObject.FindProperty(Slider.FORMAT_FIELD);
            _onValueChanged = serializedObject.FindProperty(Slider.ON_VALUE_CHANGED_FIELD);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

           // EditorGUILayout.LabelField("Handle", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_handleRect);
            EditorGUILayout.PropertyField(_fillRect);
            EditorGUILayout.PropertyField(_direction);
            EditorGUILayout.PropertyField(_magnetToCursor);
            //EditorGUILayout.LabelField("Value", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_minValue);
            EditorGUILayout.PropertyField(_maxValue);
            EditorGUILayout.PropertyField(_step);
            EditorGUILayout.PropertyField(_curve);
            _value.floatValue = EditorGUILayout.Slider(_value.floatValue, _minValue.floatValue, _maxValue.floatValue);
            //EditorGUILayout.LabelField("Display", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_text);
            EditorGUILayout.PropertyField(_format);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_onValueChanged);

            serializedObject.ApplyModifiedProperties();
        }
    }
}