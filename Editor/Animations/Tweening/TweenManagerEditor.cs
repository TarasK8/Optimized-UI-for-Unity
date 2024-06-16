using TarasK8.UI.Animations.Tweening;
using UnityEditor;
using UnityEngine;

namespace TarasK8.UIEditor.Animations.Tweening
{
    [CustomEditor(typeof(TweenManager))]
    public class TweenManagerEditor : Editor
    {
        public override bool RequiresConstantRepaint() => true;

        public override void OnInspectorGUI()
        {
            var tweens = TweenManager.GetActiveTweens();
            EditorGUILayout.LabelField($"Active Tweens ({tweens.Count})", EditorStyles.boldLabel);
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