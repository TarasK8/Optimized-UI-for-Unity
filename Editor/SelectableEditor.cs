using System.Reflection;
using TarasK8.UI;
using TarasK8.UI.Animations;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace TarasK8.UIEditor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Selectable))]
    public class SelectableEditor : UnityEditor.UI.SelectableEditor
    {
        #region For Visualize Navigation Button
        private static string s_ShowNavigationKey = "SelectableEditor.ShowNavigation";
        private FieldInfo s_ShowNavigation;
        private GUIContent m_VisualizeNavigationContent = new GUIContent("Visualize", "Show navigation flows between selectable UI elements.");
        #endregion

        private SerializedProperty m_Interactable;
        private SerializedProperty m_Navigation;

        private SerializedProperty _transitionType;
        private SerializedProperty _stateMachine;
        private SerializedProperty _normal, _hover, _pressed, _selected, _disabled;

        private GUIContent _transitionTypeContent = new GUIContent("Transition");
        private AnimBool _showTransitionsFade;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_Interactable = serializedObject.FindProperty(nameof(m_Interactable));
            m_Navigation = serializedObject.FindProperty(nameof(m_Navigation));

            _transitionType = serializedObject.FindProperty(nameof(_transitionType));
            _stateMachine = serializedObject.FindProperty(nameof(_stateMachine));
            _normal = serializedObject.FindProperty(nameof(_normal));
            _hover = serializedObject.FindProperty(nameof(_hover));
            _pressed = serializedObject.FindProperty(nameof(_pressed));
            _selected = serializedObject.FindProperty(nameof(_selected));
            _disabled = serializedObject.FindProperty(nameof(_disabled));

            s_ShowNavigation = typeof(UnityEditor.UI.SelectableEditor).GetField(nameof(s_ShowNavigation), BindingFlags.NonPublic | BindingFlags.Static);
            _showTransitionsFade = new AnimBool(true);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Interactable);
            EditorGUILayout.PropertyField(_transitionType, _transitionTypeContent);
            _showTransitionsFade.target = _transitionType.enumValueIndex != 0;

            if (EditorGUILayout.BeginFadeGroup(_showTransitionsFade.faded))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_stateMachine);
                StateMachine stateMachine = ((Selectable)target).GetStateMachine();
                if(stateMachine is not null)
                {
                    string[] options = stateMachine.GetAllStateNames();
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

            EditorGUILayout.PropertyField(m_Navigation);
            VizualizeNavigationButton();

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