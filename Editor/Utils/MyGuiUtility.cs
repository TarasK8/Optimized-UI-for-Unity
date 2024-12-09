using UnityEditor;
using UnityEngine;

namespace TarasK8.UI.Editor.Utils
{
    public static class MyGuiUtility
    {
        public static readonly GUILayoutOption AddButtonWidth = GUILayout.Width(230f);
        public static readonly GUILayoutOption AddButtonHeight = GUILayout.Height(24f);
        
        public static readonly GUILayoutOption RemoveButtonWidth = GUILayout.Width(20f);
        public static readonly GUIContent RemoveButtonContent = new GUIContent("X", "Remove");
        
        public static bool DrawAddButton(string label)
        {
            EditorGUILayout.BeginHorizontal();
            
            GUILayout.FlexibleSpace();
            bool clicked = GUILayout.Button(label, AddButtonWidth, AddButtonHeight);
            GUILayout.FlexibleSpace();
            
            EditorGUILayout.EndHorizontal();
            
            return clicked;
        }

        public static bool DrawRemoveButton()
        {
            return GUILayout.Button(RemoveButtonContent, RemoveButtonWidth);
        }
    }
}
