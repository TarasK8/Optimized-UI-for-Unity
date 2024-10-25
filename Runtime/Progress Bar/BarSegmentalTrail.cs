using UnityEngine;
using UnityEngine.Pool;

namespace TarasK8.UI
{
    [AddComponentMenu("Optimized UI/Progress Bar/Bar Segmental Trail")]
    [RequireComponent(typeof(RectTransform))]
    public class BarSegmentalTrail : MonoBehaviour
    {
        [SerializeField] private BarSegment _targetSegment;
        [SerializeField] private PooledBarSegment _segmentPrefab;
        [Header("Active")]
        [SerializeField] private bool _increasing = true;
        [SerializeField] private bool _decreasing = true;
        [Header("Optimization")]
        [SerializeField] private int _maxSegmentCount = 20;
        [SerializeField, Range(0f, 1f)] private float _mergeThreshold = 0.015f;
        [SerializeField, Min(0f)] private float _mergeTime = 0.15f;

        private RectTransform _rectTransform;
        private ObjectPool<PooledBarSegment> _segments;
        private PooledBarSegment _lastSpawnedStartSegment;
        private float _lastSpawnedStartTime;
        private PooledBarSegment _lastSpawnedEndSegment;
        private float _lastSpawnedEndTime;

        private void Awake()
        {
            _rectTransform = (RectTransform)transform;
            _segments = new ObjectPool<PooledBarSegment>(
                createFunc: CreateSegment,
                actionOnGet: ActionOnGet,
                actionOnRelease: OnReleaseSegment,
                actionOnDestroy: ActionOnDestroy,
                defaultCapacity: _maxSegmentCount,
                maxSize: _maxSegmentCount,
                collectionCheck: true);
        }

        private void OnEnable()
        {
            if (_targetSegment != null)
            {
                _targetSegment.OnStartPositionChange += BarSegment_OnStartPositionChange;
                _targetSegment.OnEndPositionChange += BarSegment_OnEndPositionChange;
            }
        }

        private void OnDisable()
        {
            if (_targetSegment != null)
            {
                _targetSegment.OnStartPositionChange -= BarSegment_OnStartPositionChange;
                _targetSegment.OnEndPositionChange -= BarSegment_OnEndPositionChange;
            }
        }

        private void BarSegment_OnStartPositionChange(float oldPosition, float newPosition)
        {
            bool isIncrease = newPosition < oldPosition;
            
            if((isIncrease && _increasing) || (!isIncrease && _decreasing))
                HandlePositionChange(oldPosition, newPosition, isIncrease, ref _lastSpawnedStartSegment, ref _lastSpawnedStartTime);
        }

        private void BarSegment_OnEndPositionChange(float oldPosition, float newPosition)
        {
            bool isIncrease = newPosition > oldPosition;
            
            if((isIncrease && _increasing) || (!isIncrease && _decreasing))
                HandlePositionChange(oldPosition, newPosition, isIncrease, ref _lastSpawnedEndSegment, ref _lastSpawnedEndTime);
        }

        private void HandlePositionChange(float oldPosition, float newPosition, bool isIncrease, ref PooledBarSegment lastSpawnedSegment, ref float lastSpawnedTime)
        {
            if (Mathf.Approximately(oldPosition, newPosition))
                return;

            float length = Mathf.Abs(newPosition - oldPosition);

            if (CanMerge(length, lastSpawnedTime) && lastSpawnedSegment != null)
            {
                if (newPosition > lastSpawnedSegment.Bar.PositionStart)
                    lastSpawnedSegment.Bar.PositionEnd = newPosition;
                else
                    lastSpawnedSegment.Bar.PositionStart = newPosition;
                lastSpawnedSegment.ResetLifetime();
            }
            else
            {
                var segment = _segments.Get();
                segment.ResetLifetime();

                if (isIncrease)
                    segment.Increase();
                else
                    segment.Decrease();

                segment.Bar.SetPosition(oldPosition, newPosition);
                lastSpawnedSegment = segment;
                lastSpawnedTime = Time.time;
            }
        }

        private bool CanMerge(float length, float lastSpawnedTime)
        {
            return length < _mergeThreshold && Time.time < lastSpawnedTime + _mergeTime;
        }

        private PooledBarSegment CreateSegment()
        {
            var segment = Instantiate(_segmentPrefab, _rectTransform);
            segment.Initialize(_segments);
            return segment;
        }

        private void ActionOnGet(PooledBarSegment segment)
        {
            segment.gameObject.SetActive(true);
        }

        private void OnReleaseSegment(PooledBarSegment segment)
        {
            if (_lastSpawnedStartSegment == segment)
                _lastSpawnedStartSegment = null;
            if (_lastSpawnedEndSegment == segment)
                _lastSpawnedEndSegment = null;

            segment.gameObject.SetActive(false);
        }

        private void ActionOnDestroy(PooledBarSegment obj)
        {
            obj.Destroy();
        }
    }
}
