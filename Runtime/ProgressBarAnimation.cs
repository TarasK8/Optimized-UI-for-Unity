using TarasK8.UI.Animations;
using TarasK8.UI.Animations.Tweening;
using UnityEngine;

namespace TarasK8.UI
{
    [AddComponentMenu("Optimized UI/Progress Bar Animation")]
    [RequireComponent(typeof(ProgressBar))]
    public class ProgressBarAnimation : MonoBehaviour
    {
        [SerializeField] private Animation _animationIncrease;
        [SerializeField] private Animation _animationDecrease;

        private ProgressBar _progressBar;

        private void Awake()
        {
            _progressBar = GetComponent<ProgressBar>();
        }

        private void OnEnable()
        {
            _progressBar.IsUpdateVisual = false;
            _progressBar.OnValueChanged += ProgressBar_OnValueChanged;
        }

        private void OnDisable()
        {
            _progressBar.OnValueChanged -= ProgressBar_OnValueChanged;
            _progressBar.IsUpdateVisual = true;
            _animationIncrease.Complate();
        }

        private void ProgressBar_OnValueChanged(float oldValue, float value)
        {
            if (Mathf.Approximately(oldValue, value))
                return;

            // Debug.Log($"VV: {_progressBar.VisualValue}; V: {value}; if: {_progressBar.VisualValue < value}");
            if(_progressBar.VisualValue < value)
            {
                PlayIncrease(value);
            }
            else if(_progressBar.VisualValue > value)
            {
                PlayDecrease(value);
            }
        }

        public void PlayDecrease(float targetValue)
        {
            _animationDecrease.Initialize(_progressBar, targetValue);
            _animationIncrease.Stop();
            _animationDecrease.Reset();
            TweenManager.StartTween(_animationDecrease);
        }

        public void PlayIncrease(float targetValue)
        {
            _animationIncrease.Initialize(_progressBar, targetValue);
            _animationDecrease.Stop();
            _animationIncrease.Reset();
            TweenManager.StartTween(_animationIncrease);
        }

        [System.Serializable]
        private class Animation : Tween
        {
            [field: SerializeField, Min(0f)] public override float Delay { get; protected set; }
            [field: SerializeField, Min(0f)] public override float Duration { get; protected set; }
            [SerializeField] private Easing _easing;

            private ProgressBar _target;
            private float _from;
            private float _to;

            public override void Process(float t)
            {
                if (_target == null)
                    return;
                float value = Mathf.LerpUnclamped(_from, _to, _easing.Evaluate(t));
                _target.UpdateVisualProgress(value);
            }

            public override void Start()
            {
                //Reset();
            }

            public void Initialize(ProgressBar progressBar, float targetValue)
            {
                _target = progressBar;
                _from = progressBar.VisualValue;
                _to = targetValue;
            }
        }
    }
}