using System.Reflection;
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
            StateSelectorAttribute stateSelector = (StateSelectorAttribute)attribute;
            StateMachine stateMachine;
            if(stateSelector.StateMachineFieldName != null)
            {
                SerializedProperty stateMachineProperty = property.serializedObject.FindProperty(stateSelector.StateMachineFieldName);
                stateMachine = (StateMachine)stateMachineProperty.objectReferenceValue;
            }
            else
            {
                stateMachine = (StateMachine)property.serializedObject.targetObject;
            }
            string[] states = stateMachine.GetAllStateNames();


            if (property.propertyType == SerializedPropertyType.Integer)
            {
                int currentIndex;

                if (stateSelector.HasNone)
                {
                    ArrayUtility.Insert(ref states, 0, "<none>");
                    currentIndex = Mathf.Clamp(property.intValue, -1, states.Length - 1);
                    currentIndex++;
                }
                else
                {
                    currentIndex = Mathf.Clamp(property.intValue, 0, states.Length - 1);
                }

                int newIndex = EditorGUI.Popup(position, label.text, currentIndex, states);

                if (stateSelector.HasNone)
                    newIndex--;

                property.intValue = newIndex;
            }
            else
            {
                EditorGUI.LabelField(position, label.text, "Use [StateSelector] with int.");
            }
        }
    }
}