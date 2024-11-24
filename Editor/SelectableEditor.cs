using System;
using System.Linq;
using System.Reflection;
using TarasK8.UI;
using TarasK8.UI.Animations;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace TarasK8.UI.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Selectable), true)]
    public class SelectableEditor : UnityEditor.UI.SelectableEditor
    {
        #region For Visualize Navigation Button
        private static string s_ShowNavigationKey = "SelectableEditor.ShowNavigation";
        private FieldInfo s_ShowNavigation;
        private GUIContent m_VisualizeNavigationContent = new GUIContent("Visualize", "Show navigation flows between selectable UI elements.");
        #endregion

        private SerializedProperty _interactable;
        private SerializedProperty _navigation;

        private SerializedProperty _transitionType;
        private SerializedProperty _stateMachine;
        private SerializedProperty _normal, _hover, _pressed, _selected, _disabled;

        private GUIContent _transitionTypeContent = new GUIContent("AnimatedProperty");
        private AnimBool _showTransitionsFade;

        public string[] _propertyPathToExcludeForChildClasses;

        protected override void OnEnable()
        {
            base.OnEnable();
            _interactable = serializedObject.FindProperty("m_Interactable");
            _navigation = serializedObject.FindProperty("m_Navigation");

            _transitionType = serializedObject.FindProperty("_transitionType");
            _stateMachine = serializedObject.FindProperty("_stateMachine");
            _normal = serializedObject.FindProperty("_normal");
            _hover = serializedObject.FindProperty("_hover");
            _pressed = serializedObject.FindProperty("_pressed");
            _selected = serializedObject.FindProperty("_selected");
            _disabled = serializedObject.FindProperty("_disabled");

            s_ShowNavigation = typeof(UnityEditor.UI.SelectableEditor).GetField(nameof(s_ShowNavigation), BindingFlags.NonPublic | BindingFlags.Static);
            _showTransitionsFade = new AnimBool(_transitionType.enumValueIndex != 0);

            _propertyPathToExcludeForChildClasses = new[]
            {
                _interactable.propertyPath,
                _navigation.propertyPath,
                _transitionType.propertyPath,
                _stateMachine.propertyPath,
                _normal.propertyPath,
                _hover.propertyPath,
                _pressed.propertyPath,
                _selected.propertyPath,
                _disabled.propertyPath,
            };
            string[] baseExcludeProperties = (string[])typeof(UnityEditor.UI.SelectableEditor).GetField("m_PropertyPathToExcludeForChildClasses", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this);
            _propertyPathToExcludeForChildClasses = _propertyPathToExcludeForChildClasses.Concat(baseExcludeProperties).ToArray();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_interactable);
            EditorGUILayout.PropertyField(_transitionType, _transitionTypeContent);
            _showTransitionsFade.target = _transitionType.enumValueIndex != 0;

            if (EditorGUILayout.BeginFadeGroup(_showTransitionsFade.faded))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_stateMachine);
                StateMachine stateMachine = ((Selectable)target).GetStateMachine();
                if(stateMachine is not null)
                {
                    string[] options = stateMachine.States.GetAllNames().ToArray();
                    StateField(_normal, options);
                    StateField(_hover, options);
                    StateField(_pressed, options);
                    StateField(_selected, options);
                    StateField(_disabled, options);
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFadeGroup();
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_navigation);
            VizualizeNavigationButton();

            if (GetType() != typeof(SelectableEditor))
                return;

            DrawPropertiesExcluding(serializedObject, _propertyPathToExcludeForChildClasses);

            serializedObject.ApplyModifiedProperties();
        }

        private void StateField(SerializedProperty stateField, string[] options)
        {
            stateField.intValue = EditorGUILayout.Popup(stateField.displayName, stateField.intValue, options);
        }

        private void VizualizeNavigationButton()
        {
            EditorGUI.BeginChangeCheck();
            Rect toggleRect = EditorGUILayout.GetControlRect();
            toggleRect.xMin += EditorGUIUtility.labelWidth;
            bool showNav = GUI.Toggle(toggleRect, (bool)s_ShowNavigation.GetValue(this), m_VisualizeNavigationContent, EditorStyles.miniButton);
            s_ShowNavigation.SetValue(this, showNav);
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetBool(s_ShowNavigationKey, (bool)s_ShowNavigation.GetValue(this));
                SceneView.RepaintAll();
            }
        }
    }
}