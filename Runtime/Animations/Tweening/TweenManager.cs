using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TarasK8.UI.Animations.Tweening
{
    public class TweenManager : MonoBehaviour
    {

        private static TweenManager _instance;
        private List<Tween> _activeTweens = new List<Tween>();

        private void Awake()
        {
            _instance = GetOrCreateInstance();
        }

        private void Update()
        {
            foreach (var tween in _activeTweens)
            {
                tween.Update(Time.deltaTime);
            }

            _activeTweens.RemoveAll(t => t.IsCompleted);
        }

        public static List<Tween> GetActiveTweens()
        {
            return GetOrCreateInstance()._activeTweens;
        }

        public static void StartTween(Tween tween)
        {
            var activeTweens = GetOrCreateInstance()._activeTweens;
            if (activeTweens.Contains(tween)) return;
            activeTweens.Add(tween);
        }

        [MenuItem("GameObject/UI Optimazed/UI Tween Manager")]
        private static TweenManager GetOrCreateInstance()
        {
            if (_instance != null)
            {
                return _instance;
            }
            else
            {
                TweenManager[] tweenManagers = FindObjectsByType<TweenManager>(FindObjectsSortMode.None);
                if(tweenManagers.Length == 1)
                {
                    return tweenManagers[0];
                }
                if (tweenManagers.Length > 1)
                {
                    for (int i = 1; i < tweenManagers.Length; i++)
                    {
                        Destroy(tweenManagers[i].gameObject);
                    }
                    Debug.LogError("There can be only one UI Tween Manager in a scene! Surplus objects were destroyed.");
                    return tweenManagers[0];
                }
                else
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