using System;
using System.Collections.Generic;
using System.Linq;
using TarasK8.UI.Animations;
using UnityEditor;
using UnityEngine;

namespace TarasK8.UI.Editor.Animations
{
    [CustomPropertyDrawer(typeof(StateSelectorAttribute))]
    public class StateSelectorDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.Integer)
            {
                EditorGUI.LabelField(position, label.text, "Use [StateSelector] with int.");
                return;
            }
            
            StateSelectorAttribute stateSelector = (StateSelectorAttribute)attribute;
            var stateMachine = GetStateMachine(property, stateSelector);

            List<string> options = stateMachine.States.GetAllNames().ToList();
            
            int currentIndex;

            if (stateSelector.HasNone)
            {
                options.Insert(0, "<none>");
                currentIndex = Mathf.Clamp(property.intValue, -1, options.Count - 1);
                currentIndex++;
            }
            else
            {
                currentIndex = Mathf.Clamp(property.intValue, 0, options.Count - 1);
            }

            int newIndex = EditorGUI.Popup(position, label.text, currentIndex, options.ToArray());

            if (stateSelector.HasNone)
                newIndex--;

            property.intValue = newIndex;
        }

        private static StateMachine GetStateMachine(SerializedProperty property, StateSelectorAttribute stateSelector)
        {
            if(stateSelector.StateMachineFieldName != null)
            {
                SerializedProperty stateMachineProperty = property.serializedObject.FindProperty(stateSelector.StateMachineFieldName);
                return (StateMachine)stateMachineProperty.objectReferenceValue;
            }

            return (StateMachine)property.serializedObject.targetObject;
        }
    }
}