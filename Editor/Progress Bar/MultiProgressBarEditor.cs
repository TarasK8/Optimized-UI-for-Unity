using UnityEditor;
using UnityEngine;

namespace TarasK8.UI.Editor
{
    [CustomEditor(typeof(MultiProgressBar), true)]
    public class MultiProgressBarEditor : ProgressBarBaseEditor
    {
        private SerializedProperty _bars;
        private SerializedProperty _ignoreLenght;
        private SerializedProperty _reversed;

        private MultiProgressBar _target;

        protected override void OnEnable()
        {
            base.OnEnable();
            
            _target = (MultiProgressBar)serializedObject.targetObject;

            _ignoreLenght = serializedObject.FindProperty("_ignoreLenght");
            _reversed = serializedObject.FindProperty("_reversed");
            _bars = serializedObject.FindProperty("_fillBars");
            
            AddToExcludeList(new[]{_bars.propertyPath, _ignoreLenght.propertyPath, _reversed.propertyPath});
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Visual", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_ignoreLenght);
            EditorGUILayout.PropertyField(_reversed);
            EditorGUILayout.PropertyField(_bars);
            DrawButtons();

            serializedObject.ApplyModifiedProperties();
            
            base.OnInspectorGUI();
        }

        private void DrawButtons()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Sort by X"))
                _target.SortListByX();
            
            if (GUILayout.Button("Sort by Y"))
                _target.SortListByY();
            
            if (GUILayout.Button("Reverse"))
                _target.ReverseList();
            
            EditorGUILayout.EndHorizontal();
        }
    }
}