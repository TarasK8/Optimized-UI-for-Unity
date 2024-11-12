using System.Collections.Generic;
using TarasK8.UI.Animations.Tweening;
using UnityEditor;
using UnityEngine;

namespace TarasK8.UI.Editor.Animations.Tweening
{
    [CustomEditor(typeof(TweenManager))]
    public class TweenManagerEditor : UnityEditor.Editor
    {
        private const string EditorHelpBox = "Active tweens will be displayed here when you enter the playmode";
        
        public override bool RequiresConstantRepaint() => true;

        public override void OnInspectorGUI()
        {
            if (Application.isPlaying == false)
            {
                EditorGUILayout.HelpBox(EditorHelpBox, MessageType.Info);
                return;
            }
            
            DrawTweenList("Active Tweens Unscaled Time", TweenManager.GetActiveTweensUnscaledTime());
            EditorGUILayout.Space();
            DrawTweenList("Active Tweens", TweenManager.GetActiveTweens());
        }

        private void DrawTweenList(string label, IReadOnlyCollection<Tween> tweens)
        {
            EditorGUILayout.LabelField($"{label} ({tweens.Count})", EditorStyles.boldLabel);
            foreach (var item in tweens)
            {
                DrawTween(item);
            }
        }

        private void DrawTween(Tween tween)
        {
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.LabelField($"{tween.GetType().Name} Tween", EditorStyles.boldLabel);
            string elapsedIndicator = string.Format("Elapsed time: {0:F2}", tween.ElapsedTime);
            Rect r = EditorGUILayout.BeginVertical();
            EditorGUI.ProgressBar(r, tween.Progress, elapsedIndicator);
            GUILayout.Space(16);
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndVertical();
        }
    }
}