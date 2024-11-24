using TarasK8.UI.Animations;
using TarasK8.UI.Editor.Animations;
using UnityEditor;
using UnityEngine;

namespace TarasK8.UI.Editor
{
    public class StateListDrawer
    {
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

            var propertyList = property.GetChildProperties();
            foreach (var prop in propertyList)
            {
                EditorGUILayout.PropertyField(prop);
            }
            
            EditorGUILayout.EndVertical();
        }

        private static void DrawAddButton(SerializedProperty property, StateList stateList)
        {
            EditorGUILayout.BeginHorizontal();
            
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Add State", GUILayout.Height(24), GUILayout.Width(230)))
            {
                stateList.AddState($"State {stateList.Count}");
                EditorUtility.SetDirty(property.serializedObject.targetObject);
            }
            GUILayout.FlexibleSpace();
            
            EditorGUILayout.EndHorizontal();
        }
    }
}
