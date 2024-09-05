using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace TarasK8.UI.Editor
{
    [CustomEditor(typeof(ScrollRect))]
    public class ScrollRectEditor : UnityEditor.Editor
    {
        private SerializedProperty _content;
        private SerializedProperty _horizontal;
        private SerializedProperty _vertical;
        private SerializedProperty _movementType;
        private SerializedProperty _elasticity;
        private SerializedProperty _inertia;
        private SerializedProperty _decelerationRate;
        private SerializedProperty _scrollSensitivity;
        private SerializedProperty _viewport;
        private SerializedProperty _horizontalScrollbar;
        private SerializedProperty _verticalScrollbar;
        private SerializedProperty _horizontalScrollbarVisibility;
        private SerializedProperty _verticalScrollbarVisibility;
        private SerializedProperty _horizontalScrollbarSpacing;
        private SerializedProperty _verticalScrollbarSpacing;
        private SerializedProperty _onValueChanged;
        private AnimBool _showElasticity;
        private AnimBool _showDecelerationRate;
        private bool _viewportIsNotChild, _hScrollbarIsNotChild, _vScrollbarIsNotChild;
        private static string _hError = "For this visibility mode, the Viewport property and the Horizontal Scrollbar property both needs to be set to a Rect Transform that is a child to the Scroll Rect.";
        private static string _vError = "For this visibility mode, the Viewport property and the Vertical Scrollbar property both needs to be set to a Rect Transform that is a child to the Scroll Rect.";
        private GUIContent _emptyContent = new GUIContent(string.Empty);
        private GUIContent _scrollbarsHeader = new GUIContent("Scrollbars");
        private GUIContent _scrollingHeader = new GUIContent("Scrolling");

        protected virtual void OnEnable()
        {
            _content = serializedObject.FindProperty("_content");
            _horizontal = serializedObject.FindProperty("_horizontal");
            _vertical = serializedObject.FindProperty("_vertical");
            _movementType = serializedObject.FindProperty("_movementType");
            _elasticity = serializedObject.FindProperty("_elasticity");
            _inertia = serializedObject.FindProperty("_inertia");
            _decelerationRate = serializedObject.FindProperty("_decelerationRate");
            _scrollSensitivity = serializedObject.FindProperty("_scrollSensitivity");
            _viewport = serializedObject.FindProperty("_viewport");
            _horizontalScrollbar = serializedObject.FindProperty("_horizontalScrollbar");
            _verticalScrollbar = serializedObject.FindProperty("_verticalScrollbar");
            _horizontalScrollbarVisibility = serializedObject.FindProperty("_horizontalScrollbarVisibility");
            _verticalScrollbarVisibility = serializedObject.FindProperty("_verticalScrollbarVisibility");
            _horizontalScrollbarSpacing = serializedObject.FindProperty("_horizontalScrollbarSpacing");
            _verticalScrollbarSpacing = serializedObject.FindProperty("_verticalScrollbarSpacing");
            _onValueChanged = serializedObject.FindProperty("_onValueChanged");

            _showElasticity = new AnimBool(Repaint);
            _showDecelerationRate = new AnimBool(Repaint);
            SetAnimBools(true);
        }

        protected virtual void OnDisable()
        {
            _showElasticity.valueChanged.RemoveListener(Repaint);
            _showDecelerationRate.valueChanged.RemoveListener(Repaint);
        }

        void SetAnimBools(bool instant)
        {
            SetAnimBool(_showElasticity, !_movementType.hasMultipleDifferentValues && _movementType.enumValueIndex == (int)ScrollRect.ClampindType.Elastic, instant);
            SetAnimBool(_showDecelerationRate, !_inertia.hasMultipleDifferentValues && _inertia.boolValue == true, instant);
        }

        void SetAnimBool(AnimBool a, bool value, bool instant)
        {
            if (instant)
                a.value = value;
            else
                a.target = value;
        }

        void CalculateCachedValues()
        {
            _viewportIsNotChild = false;
            _hScrollbarIsNotChild = false;
            _vScrollbarIsNotChild = false;
            if (targets.Length == 1)
            {
                Transform transform = ((ScrollRect)target).transform;
                if (_viewport.objectReferenceValue == null || ((RectTransform)_viewport.objectReferenceValue).transform.parent != transform)
                    _viewportIsNotChild = true;
                if (_horizontalScrollbar.objectReferenceValue == null || ((Scrollbar)_horizontalScrollbar.objectReferenceValue).transform.parent != transform)
                    _hScrollbarIsNotChild = true;
                if (_verticalScrollbar.objectReferenceValue == null || ((Scrollbar)_verticalScrollbar.objectReferenceValue).transform.parent != transform)
                    _vScrollbarIsNotChild = true;
            }
        }

        public override void OnInspectorGUI()
        {
            SetAnimBools(false);

            serializedObject.Update();
            // Once we have a reliable way to know if the object changed, only re-cache in that case.
            CalculateCachedValues();

            EditorGUILayout.PropertyField(_content);
            EditorGUILayout.PropertyField(_viewport);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField(_scrollbarsHeader, EditorStyles.boldLabel);
            // Horizontal Scroll
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_horizontal);
            if (_horizontal.boolValue)
            {
                EditorGUILayout.PropertyField(_horizontalScrollbar, _emptyContent);
                EditorGUILayout.EndHorizontal();

                if (_horizontalScrollbar.objectReferenceValue && !_horizontalScrollbar.hasMultipleDifferentValues)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(_horizontalScrollbarVisibility, EditorGUIUtility.TrTextContent("Visibility"));

                    if ((ScrollRect.ScrollbarVisibility)_horizontalScrollbarVisibility.enumValueIndex == ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport
                        && !_horizontalScrollbarVisibility.hasMultipleDifferentValues)
                    {
                        if (_viewportIsNotChild || _hScrollbarIsNotChild)
                            EditorGUILayout.HelpBox(_hError, MessageType.Error);
                        EditorGUILayout.PropertyField(_horizontalScrollbarSpacing, EditorGUIUtility.TrTextContent("Spacing"));
                    }

                    EditorGUI.indentLevel--;
                }
            }
            else
            {
                EditorGUILayout.EndHorizontal();
            }

            // Vertical Scroll
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_vertical);
            if (_vertical.boolValue)
            {
                EditorGUILayout.PropertyField(_verticalScrollbar, _emptyContent);
                EditorGUILayout.EndHorizontal();

                if (_verticalScrollbar.objectReferenceValue && !_verticalScrollbar.hasMultipleDifferentValues)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(_verticalScrollbarVisibility, EditorGUIUtility.TrTextContent("Visibility"));

                    if ((ScrollRect.ScrollbarVisibility)_verticalScrollbarVisibility.enumValueIndex == ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport
                        && !_verticalScrollbarVisibility.hasMultipleDifferentValues)
                    {
                        if (_viewportIsNotChild || _vScrollbarIsNotChild)
                            EditorGUILayout.HelpBox(_vError, MessageType.Error);
                        EditorGUILayout.PropertyField(_verticalScrollbarSpacing, EditorGUIUtility.TrTextContent("Spacing"));
                    }

                    EditorGUI.indentLevel--;
                }
            }
            else
            {
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField(_scrollingHeader, EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_movementType);
            if (EditorGUILayout.BeginFadeGroup(_showElasticity.faded))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_elasticity);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFadeGroup();

            EditorGUILayout.PropertyField(_inertia);
            if (EditorGUILayout.BeginFadeGroup(_showDecelerationRate.faded))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_decelerationRate);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFadeGroup();

            EditorGUILayout.PropertyField(_scrollSensitivity);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_onValueChanged);

            serializedObject.ApplyModifiedProperties();
        }
    }
}