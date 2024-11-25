using System.Linq;
using TarasK8.UI.Animations;
using TarasK8.UI.Editor.Animations;
using TarasK8.UI.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace TarasK8.UI.Editor
{
    public static class StateListDrawer
    {
        public const string NameFieldName = "<Name>k__BackingField";
        
        public static void Draw(SerializedProperty property, StateList stateList)
        {
            var listProperty = property.FindPropertyRelative("_states");
            
            EditorGUILayout.LabelField($"States ({stateList.Count})", EditorStyles.boldLabel);

            for (int i = 0; i < listProperty.arraySize; i++)
            {
                DrawState(listProperty, i);
            }

            DrawAddButton(property, stateList);
        }

        private static void DrawState(SerializedProperty listProperty, int index)
        {
            var property = listProperty.GetArrayElementAtIndex(index);
            EditorGUILayout.BeginVertical(GUI.skin.box);

            var nameProperty = property.FindPropertyRelative(NameFieldName);
            var dataListProperty = property.FindPropertyRelative("_dataList");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(nameProperty, GUIContent.none);
            bool remove = MyGuiUtility.DrawRemoveButton();
            EditorGUILayout.EndHorizontal();
            
            for (int i = 0; i < dataListProperty.arraySize; i++)
            {
                var dataProperty = dataListProperty.GetArrayElementAtIndex(i);
                var dataChildProperties = dataProperty.GetChildProperties();
                foreach (var prop in dataChildProperties)
                {
                    EditorGUILayout.PropertyField(prop);
                }
            }
            
            EditorGUILayout.EndVertical();
            
            if(remove)
                listProperty.DeleteArrayElementAtIndex(index);
        }

        private static void DrawAddButton(SerializedProperty property, StateList stateList)
        {
            if (MyGuiUtility.DrawAddButton("Add State"))
            {
                var name = GetUniqueName(stateList);
                stateList.TryAddState(name);
                EditorUtility.SetDirty(property.serializedObject.targetObject);
            }
        }

        private static string GetUniqueName(StateList stateList)
        {
            int nameIndex = 0;
            while (stateList.ContainsName($"State {nameIndex}"))
                nameIndex++;
            return $"State {nameIndex}";
        }
    }
}
