using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TarasK8.UI.Animations;
using TarasK8.UI.Animations.AnimatedProperties;
using TarasK8.UI.Editor.Utils;
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
        private SerializedProperty _fullyComplete;
        private SerializedProperty _defaultState;
        private SerializedProperty _animatedProperties;
        private StateMachine _target;
        private SerializedProperty _states;

        private void OnEnable()
        {
            _target = target as StateMachine;
            _ignoreTimeScale = serializedObject.FindProperty("_ignoreTimeScale");
            _fullyComplete = serializedObject.FindProperty("_fullyCompleteTransition");
            _defaultState = serializedObject.FindProperty("_defaultState");
            _animatedProperties = serializedObject.FindProperty("_animatedProperties");
            _states = serializedObject.FindProperty("_states");

            // Initialize _propertiesTypes and _propertiesTypesOptions if not already done
            if (_propertiesTypes == null)
            {
                // Debug.Log("Init properties types option");
                _propertiesTypes = GetAnimatedPropertyTypes();
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
            
            DrawLabel();
            DrawAllAnimatedProperties();
            DrawAddPropertyButton();
            EditorGUILayout.Space(15f);
            //DrawCreateStateButton();
            //EditorGUILayout.PropertyField(_states);
            StateListDrawer.Draw(_states, _target.States);
            DrawAddStateButton();
            
            //DrawAllStates();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawOptions()
        {
            //EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_ignoreTimeScale);
            EditorGUILayout.PropertyField(_fullyComplete);
            EditorGUILayout.PropertyField(_defaultState);
        }

        private void DrawAllAnimatedProperties()
        {
            for (int i = 0; i < _animatedProperties.arraySize; i++)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);

                var animatedProperty = _animatedProperties.GetArrayElementAtIndex(i);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(animatedProperty.managedReferenceValue.GetType().Name, EditorStyles.boldLabel);
                if (MyGuiUtility.DrawRemoveButton())
                {
                    _target.RemoveAnimatedProperty(i);
                    EditorUtility.SetDirty(target);
                }
                EditorGUILayout.EndHorizontal();

                var childs = animatedProperty.GetChildProperties();
                foreach (var element in childs)
                {
                    //if (element.name == AnimatedProperty.STATES_FIELD_NAME)
                      //  continue;
                    EditorGUILayout.PropertyField(element);
                }
                EditorGUILayout.EndVertical();
            }
        }
        
        private void DrawLabel()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Animated Properties ({_animatedProperties.arraySize})", EditorStyles.boldLabel);
            /*
            _selectedTypeOption = EditorGUILayout.Popup(_selectedTypeOption, _propertiesTypesOptions);
            if (GUILayout.Button("Add", GUILayout.Width(60f)))
            {
                var type = _propertiesTypes[_selectedTypeOption];
                AnimatedProperty animatedProperty = (AnimatedProperty)Activator.CreateInstance(type);
                _target.AddAnimatedProperty(animatedProperty);
                EditorUtility.SetDirty(target);
            }
            */
            EditorGUILayout.EndHorizontal();
        }

        public void DrawAddPropertyButton()
        {
            if (MyGuiUtility.DrawAddButton("Add Animated Property"))
            {
                var type = _propertiesTypes[_selectedTypeOption];
                AnimatedProperty animatedProperty = (AnimatedProperty)Activator.CreateInstance(type);
                _target.AddAnimatedProperty(animatedProperty);
                EditorUtility.SetDirty(target);
            }
        }

        public void DrawAddStateButton()
        {
            if (MyGuiUtility.DrawAddButton("Add State"))
            {
                var name = StateListDrawer.GetUniqueName(_target.States);
                _target.AddState(name);
                EditorUtility.SetDirty(serializedObject.targetObject);
            }
        }

        private List<Type> GetAnimatedPropertyTypes()
        {
            Type baseType = typeof(AnimatedProperty);
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            List<Type> derivedTypes = new List<Type>();

            foreach (var assembly in assemblies)
            {
                Type[] types = assembly.GetTypes();
                derivedTypes.AddRange(types.Where(t => t.IsSubclassOf(baseType) && !t.IsAbstract));
            }
            return derivedTypes;
        }
        /*
        private void DrawCreateStateButton()
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField($"States ({_target.States.Count})", EditorStyles.boldLabel);
            _selectedStateName = EditorGUILayout.TextField(_selectedStateName);
            if (GUILayout.Button("Add", GUILayout.Width(60f)))
            {
                _target.States.AddState(_selectedStateName);
                EditorUtility.SetDirty(target);
            }

            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawAllStates()
        {
            if (_animatedProperties.arraySize == 0) return;
            var states = _animatedProperties.GetArrayElementAtIndex(0).FindPropertyRelative(AnimatedProperty.STATES_FIELD_NAME);

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
            for (int i = 0; i < _animatedProperties.arraySize; i++)
            {
                var drawState = _animatedProperties.GetArrayElementAtIndex(i).FindPropertyRelative(AnimatedProperty.STATES_FIELD_NAME).GetArrayElementAtIndex(index);
                foreach (var item in drawState.GetChildProperties())
                {
                    if (item.name == IAnimationData.NAME_FIELD_NAME) continue;
                    EditorGUILayout.PropertyField(item);
                }
            }
            EditorGUILayout.EndVertical();
        }

        private int GetStatesCount()
        {
            if (_animatedProperties.arraySize == 0) return 0;
            var states = _animatedProperties.GetArrayElementAtIndex(0).FindPropertyRelative(AnimatedProperty.STATES_FIELD_NAME);
            return states.arraySize;
        }
        */
    }
}
