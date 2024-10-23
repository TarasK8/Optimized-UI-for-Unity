using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TarasK8.UI.Animations.Tweening
{
    public class TweenManager : MonoBehaviour
    {
        [SerializeField] private bool _ignoreTimeScale = true;

        private static TweenManager _instance;
        private List<Tween> _activeTweens = new List<Tween>();

        public static bool IgnoreTimeScale
        {
            get => GetOrCreateInstance()._ignoreTimeScale;
            set => GetOrCreateInstance()._ignoreTimeScale = value;
        }

        private void Awake()
        {
            _instance = GetOrCreateInstance();
        }

        private void Update()
        {
            float delta = _ignoreTimeScale ? Time.unscaledDeltaTime : Time.timeScale;

            _activeTweens.RemoveAll(TryRemove);
            foreach (var tween in _activeTweens)
            {
                tween.Update(delta);
            }
        }

        public static List<Tween> GetActiveTweens()
        {
            return GetOrCreateInstance()._activeTweens;
        }

        public static void StartTween(Tween tween, bool instantly = false)
        {
            bool isZeroDuration = Mathf.Approximately(tween.Duration, 0f) && Mathf.Approximately(tween.Delay, 0f);
            if(instantly || isZeroDuration)
            {
                tween.Start();
                tween.Complete();
                return;
            }

            var tweenManager = GetOrCreateInstance();
            var activeTweens = tweenManager._activeTweens;

            if (activeTweens.Contains(tween))
                return;

            activeTweens.Add(tween);
        }

        private bool TryRemove(Tween tween)
        {
            if (tween.IsCompleted)
            {
                tween.Reset();
                return true;
            }

            return false;
        }

#if UNITY_EDITOR
        [MenuItem("GameObject/UI Optimazed/UI Tween Manager")]
#endif
        private static TweenManager GetOrCreateInstance()
        {
            /*
#if UNITY_EDITOR
            if (Application.isPlaying == false)
            {
                Debug.Log("is not playing");
                return null;
            }
#endif
            */

            if (_instance != null)
            {
                return _instance;
            }

            var tweenManagers = FindObjectsByType<TweenManager>(FindObjectsSortMode.None);
                
            switch (tweenManagers.Length)
            {
                case 1:
                    return tweenManagers[0];
                case > 1:
                {
                    for (int i = 1; i < tweenManagers.Length; i++)
                    {
                        Destroy(tweenManagers[i].gameObject);
                    }
                    Debug.LogError("There can be only one UI Tween Manager in a scene! Surplus objects were destroyed.");
                    return tweenManagers[0];
                }
                default:
                {
                    var obj = new GameObject("UI Tween Manager", typeof(TweenManager));
                    TweenManager tweenManager = obj.GetComponent<TweenManager>();
                    Debug.Log("Created UI Tween Manager");
                    return tweenManager;
                }
            }
        }
    }
}