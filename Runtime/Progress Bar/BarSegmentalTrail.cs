using System;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;

namespace TarasK8.UI
{
    [AddComponentMenu("Optimized UI/Bar Segmental Trail")]
    [RequireComponent(typeof(RectTransform))]
    public class BarSegmentalTrail : MonoBehaviour
    {
        [SerializeField] private ProgressBar _progressBar;
        [SerializeField] private PooledBarSegment _segmentPrefab;
        [SerializeField] private int _segmentCount;
        [SerializeField, Range(0f, 1f)] private float _segmentsMergeThreshold = 0.1f;

        private RectTransform _rectTransform;
        private ObjectPool<PooledBarSegment> _segments;
        private PooledBarSegment _lastSpawnedStartSegment;
        // private float _lastSpawnedStartTime;
        private PooledBarSegment _lastSpawnedEndSegment;
        // private float _lastSpawnedEndTime;

        private void Awake()
        {
            _rectTransform = (RectTransform)transform;
            _segments = new ObjectPool<PooledBarSegment>(
                createFunc: CreateSegment,
                actionOnGet: ActionOnGet,
                actionOnRelease: OnReleaseSegment,
                actionOnDestroy: ActionOnDestroy,
                defaultCapacity: _segmentCount,
                maxSize: _segmentCount,
                collectionCheck: true);
        }

        private void OnEnable()
        {
            _progressBar.OnValueChanged += ProgressBar_OnValueChange;
        }

        private void OnDisable()
        {
            _progressBar.OnValueChanged -= ProgressBar_OnValueChange;
        }

        private void ProgressBar_OnValueChange(float oldValue, float newValue)
        {
            float oldPosition = _progressBar.CalculatePosition(oldValue);
            float newPosition = _progressBar.CalculatePosition(newValue);
            
            float lenght = Mathf.Abs(newPosition - oldPosition);
            
            if (lenght < _segmentsMergeThreshold && _lastSpawnedStartSegment != null)
            {
                if(newPosition > _lastSpawnedStartSegment.Bar.PositionStart)
                    _lastSpawnedStartSegment.Bar.PositionEnd = newPosition;
                else
                    _lastSpawnedStartSegment.Bar.PositionStart = newPosition;
                _lastSpawnedStartSegment.ResetLifetime();
            }
            else
            {
                var segment = _segments.Get();
                segment.ResetLifetime();
                
                if(newPosition > oldPosition)
                    segment.IncreaseAnimation();
                else
                    segment.DecreaseAnimation();
                
                segment.Bar.SetPosition(oldPosition, newPosition);
                _lastSpawnedStartSegment = segment;
                
            }
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
            if(_lastSpawnedStartSegment == segment)
                _lastSpawnedStartSegment = null;
            if(_lastSpawnedEndSegment == segment)
                _lastSpawnedEndSegment = null;
            
            segment.gameObject.SetActive(false);
        }

        private void ActionOnDestroy(PooledBarSegment obj)
        {
            obj.Destroy();
        }
    }
}