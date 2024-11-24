using TarasK8.UI.Animations;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TarasK8.UI.Editor.Animations
{
    [CustomPropertyDrawer(typeof(State))]
    public class StateDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = new VisualElement();
            
            var nameProperty = property.FindPropertyRelative("<Name>k__BackingField");
            var dataProperty = property.FindPropertyRelative("_data");
            
            var nameField = new PropertyField(nameProperty);
            var dataField = new PropertyField(dataProperty);
            
            container.Add(nameField);
            container.Add(new Label("Label 123"));
            //container.Add(dataField);
            
            return container;
        }

        /*
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            
            EditorGUI.BeginProperty(position, label, property);
            
            //base.OnGUI(position, property, label);
            var nameProperty = property.FindPropertyRelative("<Name>k__BackingField");
            var dataProperty = property.FindPropertyRelative("_data");
            EditorGUI.PropertyField(position, nameProperty, GUIContent.none);
            var rect = new Rect(position.x, position.y + position.height, position.width, position.height);
            EditorGUI.PropertyField(rect, nameProperty, GUIContent.none);
            
            EditorGUI.EndProperty();
        }
        */
    }
}
