using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TarasK8.UI.Animations.Tweening
{
    public class TweenManager : MonoBehaviour
    {
        private static TweenManager _instance;
        private List<Tween> _activeTweens = new List<Tween>();
        private List<Tween> _activeTweensUnsaledTime = new List<Tween>();
        private bool _enabledUnscaledTime = false;

        private void Awake()
        {
            _instance = GetOrCreateInstance();
        }

        private IEnumerator Start()
        {
            // Fixes a bug when the elapsed animation time includes the game start time (if used unscaledTime)
            yield return null;
            _enabledUnscaledTime = true;
        }

        private void Update()
        {
            // Debug.Log($"Delta: {Time.deltaTime}; Unscaled: {Time.unscaledDeltaTime}\n Time: {Time.time} Unscaled Time: {Time.unscaledTime}");
            
            UpdateTweens(_activeTweens, Time.deltaTime);
            
            if(!_enabledUnscaledTime) return;
            UpdateTweens(_activeTweensUnsaledTime, Time.unscaledDeltaTime);
            
        }

        private void UpdateTweens(List<Tween> tweens, float delta)
        {
            tweens.RemoveAll(TryRemove);
            foreach (var tween in tweens)
            {
                tween.Update(delta);
            }
        }

        public static IReadOnlyCollection<Tween> GetActiveTweens()
        {
            return GetOrCreateInstance()._activeTweens;
        }
        
        public static IReadOnlyCollection<Tween> GetActiveTweensUnscaledTime()
        {
            return GetOrCreateInstance()._activeTweensUnsaledTime;
        }

        public static void StartTween(Tween tween, bool instantly = false, bool ignoreTimeScale = true)
        {
            bool isZeroDuration = Mathf.Approximately(tween.Duration, 0f) && Mathf.Approximately(tween.Delay, 0f);
            if(instantly || isZeroDuration)
            {
                tween.Start();
                tween.Complete();
                return;
            }

            var tweenManager = GetOrCreateInstance();
            var activeTweens = ignoreTimeScale ? tweenManager._activeTweensUnsaledTime : tweenManager._activeTweens;

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