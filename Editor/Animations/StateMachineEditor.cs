using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TarasK8.UI.Animations;
using UnityEditor;
using UnityEngine;

namespace TarasK8.UI.Editor.Animations
{
    [CustomEditor(typeof(StateMachine))]
    [CanEditMultipleObjects]
    public class StateMachineEditor : UnityEditor.Editor
    {
        private static string[] _propertiesTypesOptions;
        private static List<Type> _propertiesTypes;
        private int _selectedTypeOption;
        private string _selectedStateName;
        private SerializedProperty _ignoreTimeScale;
        private SerializedProperty _fullyComplate;
        private SerializedProperty _defaultState;
        private SerializedProperty _transitions;
        private StateMachine _target;

        private void OnEnable()
        {
            _target = target as StateMachine;
            _ignoreTimeScale = serializedObject.FindProperty("_ignoreTimeScale");
            _fullyComplate = serializedObject.FindProperty("_fullyComplateTransition");
            _defaultState = serializedObject.FindProperty("_defaultState");
            _transitions = serializedObject.FindProperty("_transitions");

            // Initialize _propertiesTypes and _propertiesTypesOptions if not already done
            if (_propertiesTypes == null)
            {
                Debug.Log("Init properties types option");
                _propertiesTypes = GetTransitionTypes();
                _propertiesTypesOptions = new string[_propertiesTypes.Count];
                for (int i = 0; i < _propertiesTypes.Count; i++)
                {
                    TransitionMenuNameAttribute attribute = (TransitionMenuNameAttribute)Attribute.GetCustomAttribute(_propertiesTypes[i], typeof(TransitionMenuNameAttribute));
                    _propertiesTypesOptions[i] = attribute?.MenuName ?? _propertiesTypes[i].Name;
                }
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawOptions();
            EditorGUILayout.Space(15f);
            DrawCreateTransitionButton();
            DrawAllTransitions();
            EditorGUILayout.Space(15f);
            DrawCreateStateButton();
            DrawAllStates();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawOptions()
        {
            EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_ignoreTimeScale);
            EditorGUILayout.PropertyField(_fullyComplate);
            EditorGUILayout.PropertyField(_defaultState);
        }

        private void DrawAllTransitions()
        {
            for (int i = 0; i < _transitions.arraySize; i++)
            {
                EditorGUILayout.BeginVertical("Box");

                var transition = _transitions.GetArrayElementAtIndex(i);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(transition.managedReferenceValue.GetType().Name, EditorStyles.boldLabel);
                if (GUILayout.Button("Delete"))
                {
                    _target.RemoveTransition(i);
                    EditorUtility.SetDirty(target);
                }
                EditorGUILayout.EndHorizontal();

                var childs = transition.GetChildProperties();
                foreach (var element in childs)
                {
                    if (element.name == Transition.STATES_FIELD_NAME) continue;
                    EditorGUILayout.PropertyField(element);
                }
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawAllStates()
        {
            if (_transitions.arraySize == 0) return;
            var states = _transitions.GetArrayElementAtIndex(0).FindPropertyRelative(Transition.STATES_FIELD_NAME);

            for (int j = 0; j < states.arraySize; j++)
            {
                var state = states.GetArrayElementAtIndex(j);
                DrawState(j, state, states);
            }
        }

        private void DrawState(int index, SerializedProperty state, SerializedProperty allStates)
        {
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.BeginHorizontal();
            string currentName = state.FindPropertyRelative(IAnimationData.NAME_FIELD_NAME).stringValue;
            string newName = EditorGUILayout.TextField(currentName);
            if (newName != currentName)
            {
                _target.RenameState(index, newName);
                EditorUtility.SetDirty(target);
            }
            if (GUILayout.Button("Delete"))
            {
                _target.RemoveState(index);
                EditorUtility.SetDirty(target);
            }
            EditorGUILayout.EndHorizontal();
            for (int i = 0; i < _transitions.arraySize; i++)
            {
                var drawState = _transitions.GetArrayElementAtIndex(i).FindPropertyRelative(Transition.STATES_FIELD_NAME).GetArrayElementAtIndex(index);
                foreach (var item in drawState.GetChildProperties())
                {
                    if (item.name == IAnimationData.NAME_FIELD_NAME) continue;
                    EditorGUILayout.PropertyField(item);
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawCreateTransitionButton()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Transitions ({_transitions.arraySize})", EditorStyles.boldLabel);
            _selectedTypeOption = EditorGUILayout.Popup(_selectedTypeOption, _propertiesTypesOptions);
            if (GUILayout.Button("Add", GUILayout.Width(60f)))
            {
                _target.AddTransition(_propertiesTypes[_selectedTypeOption]);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawCreateStateButton()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"States ({GetStatesCount()})", EditorStyles.boldLabel);
            _selectedStateName = EditorGUILayout.TextField(_selectedStateName);
            if (GUILayout.Button("Add", GUILayout.Width(60f)))
            {
                _target.AddState(_selectedStateName);
                EditorUtility.SetDirty(target);
            }
            EditorGUILayout.EndHorizontal();
        }

        private List<Type> GetTransitionTypes()
        {
            Type baseType = typeof(Transition);
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            List<Type> derivedTypes = new List<Type>();

            foreach (var assembly in assemblies)
            {
                Type[] types = assembly.GetTypes();
                derivedTypes.AddRange(types.Where(t => t.IsSubclassOf(baseType) && !t.IsAbstract));
            }
            return derivedTypes;
        }

        private int GetStatesCount()
        {
            if (_transitions.arraySize == 0) return 0;
            var states = _transitions.GetArrayElementAtIndex(0).FindPropertyRelative(Transition.STATES_FIELD_NAME);
            return states.arraySize;
        }
    }
}
