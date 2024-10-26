using TarasK8.UI.Animations;
using TarasK8.UI.Animations.Tweening;
using UnityEngine;
using System;
using TarasK8.UI.Utilites;

namespace TarasK8.UI
{
    [AddComponentMenu("Optimized UI/Progress Bar/Bar Segment Animated")]
    public class BarSegmentAnimated : BarSegment
    {
        [Header("Animation Settings")]
        [SerializeField] private bool _useVisualPositionAsOld;
        [Header("Increase")]
        [SerializeField] private bool _increaseCompleteBeforePlay;
        [SerializeField] private float _increaseDuration;
        [SerializeField] private float _increaseDelay;
        [SerializeField] private Easing _increaseEasing;
        [Header("Decrease")]
        [SerializeField] private bool _decreaseCompleteBeforePlay;
        [SerializeField] private float _decreaseDuration;
        [SerializeField] private float _decreaseDelay;
        [SerializeField] private Easing _decreaseEasing;
        
        private bool _initialized = false;
        private (Animation increase, Animation decrease) _animationStart;
        private (Animation increase, Animation decrease) _animationEnd;
        
        private float? _targetPositionStart;
        private float? _targetPositionEnd;
        
        public (float start, float end) VisualPosition => (VisualPositionStart, VisualPositionEnd);
        public float VisualPositionStart => base.GetPositionStart();
        public float VisualPositionEnd => base.GetPositionEnd();

        private void Start()
        {
            _animationStart.increase = new Animation(base.SetPositionStart);
            _animationStart.decrease = new Animation(base.SetPositionStart);
            _animationEnd.increase = new Animation(base.SetPositionEnd);
            _animationEnd.decrease = new Animation(base.SetPositionEnd);
            _animationStart.increase.InitializeParameters(_increaseDuration, _increaseDelay, _increaseEasing);
            _animationStart.decrease.InitializeParameters(_decreaseDuration, _decreaseDuration, _decreaseEasing);
            _animationEnd.increase.InitializeParameters(_increaseDuration, _increaseDelay, _increaseEasing);
            _animationEnd.decrease.InitializeParameters(_decreaseDuration, _decreaseDuration, _decreaseEasing);
            _initialized = true;
        }

        protected override float GetPositionStart()
        {
            return _targetPositionStart ?? VisualPositionStart;
        }

        protected override float GetPositionEnd()
        {
            return _targetPositionEnd ?? VisualPositionEnd;
        }

        protected override void SetPositionStart(float position)
        {
            if (_initialized == false || Application.isPlaying == false)
            {
                base.SetPositionStart(position);
                return;
            }
            
            float oldPosition = _useVisualPositionAsOld ? VisualPositionStart : PositionStart;
            
            if(oldPosition < position)
                PlayDecrease(_animationStart, oldPosition, position);
            else if(oldPosition > position)
                PlayIncrease(_animationStart, oldPosition, position);
            
            _targetPositionStart = position;
        }

        protected override void SetPositionEnd(float position)
        {
            if (_initialized == false || Application.isPlaying == false)
            {
                base.SetPositionEnd(position);
                return;
            }
            
            float oldPosition = _useVisualPositionAsOld ? VisualPositionEnd : PositionEnd;
            
            if(oldPosition < position)
                PlayIncrease(_animationEnd, oldPosition, position);
            else if(oldPosition > position)
                PlayDecrease(_animationEnd, oldPosition, position);
            
            _targetPositionEnd = position;
        }

        private void PlayIncrease((Animation increase, Animation decrease) animation, float from, float to)
        {
            if (_increaseCompleteBeforePlay)
            {
                if(animation.increase.IsCompleted == false)
                    animation.increase.Process(1f);
                if(animation.decrease.IsCompleted == false)
                    animation.decrease.Complete();
            }

            animation.increase.InitializeParameters(_increaseDuration, _increaseDelay, _increaseEasing);
            animation.increase.Initialize(from, to);
            animation.increase.Reset();
            animation.increase.Process(0f);
            TweenManager.StartTween(animation.increase);
        }

        private void PlayDecrease((Animation increase, Animation decrease) animation, float from, float to)
        {
            if (_decreaseCompleteBeforePlay)
            {
                if(animation.decrease.IsCompleted == false)
                    animation.decrease.Process(1f);
                if(animation.increase.IsCompleted == false)
                    animation.increase.Complete();
            }

            animation.decrease.InitializeParameters(_decreaseDuration, _decreaseDelay, _decreaseEasing);
            animation.decrease.Initialize(from, to);
            animation.decrease.Reset();
            animation.decrease.Process(0f);
            TweenManager.StartTween(animation.decrease);
        }

        [System.Serializable]
        private class Animation : Tween
        {
            [field: SerializeField, Min(0f)] public override float Delay { get; protected set; }
            [field: SerializeField, Min(0f)] public override float Duration { get; protected set; }
            [SerializeField] private Easing _easing;

            private Action<float> _target;
            private float _from;
            private float _to;

            public Animation(Action<float> target)
            {
                _target = target;
            }

            public override void Process(float t)
            {
                if (_target == null)
                    return;
                float value = Mathf.LerpUnclamped(_from, _to, _easing.Evaluate(t));
                _target.Invoke(value);
            }

            public override void Start()
            {
                //Reset();
            }

            public void InitializeParameters(float duration, float delay, Easing easing)
            {
                Duration = duration;
                Delay = delay;
                _easing = easing;
            }

            public void Initialize(float from, float to)
            {
                _from = from;
                _to = to;
            }
        }
    }
}