using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TarasK8.UI.Animations;
using TarasK8.UI.Animations.AnimatedProperties;
using TarasK8.UI.Editor.Utils;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace TarasK8.UI.Editor.Animations
{
    [CustomEditor(typeof(StateMachine))]
    [CanEditMultipleObjects]
    public class StateMachineEditor : UnityEditor.Editor
    {
        private static string[] _propertiesTypesOptions;
        private static List<Type> _propertiesTypes;
        private static AnimatedPropertiesDropdown _addPropertyDropdown;
        private int _selectedTypeOption;
        private string _selectedStateName;
        private SerializedProperty _ignoreTimeScale;
        private SerializedProperty _fullyComplete;
        private SerializedProperty _defaultState;
        private SerializedProperty _animatedProperties;
        private SerializedProperty _states;
        private static bool _showProperties = true;
        private static bool _showStates = true;
        
        private StateMachine _target;

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
                    string option = attribute?.MenuName ?? _propertiesTypes[i].Name;
                    _propertiesTypesOptions[i] = option;
                }
                var state = new AdvancedDropdownState();
                _addPropertyDropdown = new AnimatedPropertiesDropdown(state, _propertiesTypesOptions);
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawOptions();
            
            EditorGUILayout.Space();

            _showProperties = EditorGUILayout.BeginFoldoutHeaderGroup(_showProperties,
                $"Animated Properties ({_animatedProperties.arraySize})");
            if (_showProperties)
            {
                DrawAllAnimatedProperties();
                DrawAddPropertyButton();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            
            EditorGUILayout.Space();

            _showStates = EditorGUILayout.BeginFoldoutHeaderGroup(_showStates, $"States ({_target.States.Count})");
            if (_showStates)
            {
                StateListDrawer.Draw(_states);
                DrawAddStateButton();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawOptions()
        {
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
                var childs = animatedProperty.GetChildProperties().ToArray();
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(childs[0], GUIContent.none, GUILayout.Width(16f));
                EditorGUILayout.LabelField(animatedProperty.managedReferenceValue.GetType().Name, EditorStyles.boldLabel);
                if (MyGuiUtility.DrawRemoveButton())
                {
                    _target.RemoveAnimatedProperty(i);
                    EditorUtility.SetDirty(target);
                }

                EditorGUILayout.EndHorizontal();
                for (int j = 1; j < childs.Length; j++)
                {
                    var element = childs[j];
                    EditorGUILayout.PropertyField(element);
                }

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawAddPropertyButton()
        {
            var lastRect = GUILayoutUtility.GetLastRect();
            if (MyGuiUtility.DrawAddButton("Add Animated Property"))
            {
                _addPropertyDropdown.OnItemSelected = AddProperty;
                _addPropertyDropdown.Show(CalculateDropdownRect(lastRect));
            }
        }

        private void DrawAddStateButton()
        {
            if (MyGuiUtility.DrawAddButton("Add State"))
            {
                var name = StateListDrawer.GetUniqueName(_target.States);
                _target.AddState(name);
                EditorUtility.SetDirty(serializedObject.targetObject);
            }
        }

        private void AddProperty(int index)
        {
            var type = _propertiesTypes[index];
            AnimatedProperty animatedProperty = (AnimatedProperty)Activator.CreateInstance(type);
            
            _target.AddAnimatedProperty(animatedProperty);
            EditorUtility.SetDirty(target);
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

        private Rect CalculateDropdownRect(Rect lastRect)
        {
            const float width = 230f;
            const float buttonSpacing = 27f;
            const float xOffset = 18f;
            
            float x = (lastRect.width - width) * 0.5f + xOffset;
            float y = lastRect.y + buttonSpacing;
            var rect = new Rect(x, y, width, lastRect.height);
            
            return rect;
        }
    }
}
