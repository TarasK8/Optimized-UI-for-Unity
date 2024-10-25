using TarasK8.UI.Animations.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;

namespace TarasK8.UI
{
    [AddComponentMenu("Optimized UI/Progress Bar/Pooled Bar Segment")]
    [RequireComponent(typeof(BarSegment))]
    public class PooledBarSegment : MonoBehaviour
    {
        [SerializeField] private float _lifeTimeDuration = 1.5f;
        [field: Space]
        [field: SerializeField] public UnityEvent OnIncrease { get; private set; }
        [field: SerializeField] public UnityEvent OnDecrease { get; private set; }
        
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

        public void Increase()
        {
            OnIncrease?.Invoke();
        }

        public void Decrease()
        {
            OnDecrease?.Invoke();
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
    }
}