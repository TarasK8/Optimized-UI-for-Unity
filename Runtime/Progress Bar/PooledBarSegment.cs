using System;
using TarasK8.UI.Animations.Tweening;
using UnityEngine;
using UnityEngine.Pool;

namespace TarasK8.UI
{
    public class PooledBarSegment : MonoBehaviour
    {
        private static readonly int Increase = Animator.StringToHash("Increase");
        private static readonly int Decrease = Animator.StringToHash("Decrease");
        [SerializeField] private float _lifeTimeDuration = 1.5f;
        [SerializeField] private Animator _animator;
        
        private WaitTween _lifeTime;
        private BarSegment _bar;
        private ObjectPool<PooledBarSegment> _pool;

        public BarSegment Bar
        {
            get
            {
                if(_bar == null)
                    _bar = GetComponent<BarSegment>();
                return _bar;
            }
        }

        public void Initialize(ObjectPool<PooledBarSegment> pool)
        {
            _pool = pool;
            _lifeTime = new WaitTween(Release, _lifeTimeDuration);
            ResetLifetime();
        }

        public void IncreaseAnimation()
        {
            _animator.SetTrigger(Increase);
        }

        public void DecreaseAnimation()
        {
            _animator.SetTrigger(Decrease);
        }

        public void ResetLifetime()
        {
            _lifeTime.Reset();
            TweenManager.StartTween(_lifeTime);
        }

        [ContextMenu("Release")]
        private void Release()
        {
            _pool.Release(this);
        }

        public void Destroy()
        {
            _lifeTime.Complete();
            Destroy(gameObject);
        }

        private class WaitTween : Tween
        {
            public override float Delay { get; protected set; }
            public override float Duration { get; protected set; }

            private readonly Action _action;

            public WaitTween(Action action, float delay)
            {
                Delay = delay;
                _action = action;
            }

            public override void Start()
            {
                _action?.Invoke();
            }

            public override void Process(float t) { }
        }
    }
}