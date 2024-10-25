using System;
using UnityEngine;
using UnityEngine.Pool;

namespace TarasK8.UI
{
    [AddComponentMenu("Optimized UI/Bar Segmental Trail")]
    [RequireComponent(typeof(RectTransform))]
    public class BarSegmentalTrail : MonoBehaviour
    {
        [SerializeField] private BarSegment _targetSegment;
        [SerializeField] private PooledBarSegment _segmentPrefab;
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
            _targetSegment.OnStartPositionChange += BarSegment_OnStartPositionChange;
            _targetSegment.OnEndPositionChange += BarSegment_OnEndPositionChange;
        }

        private void OnDisable()
        {
            _targetSegment.OnStartPositionChange -= BarSegment_OnStartPositionChange;
            _targetSegment.OnEndPositionChange -= BarSegment_OnEndPositionChange;
        }

        private void BarSegment_OnStartPositionChange(float oldPosition, float newPosition)
        {
            if(Mathf.Approximately(oldPosition, newPosition))
                return;
            
            float lenght = Mathf.Abs(newPosition - oldPosition);
            
            if (IsCanMarge(lenght, _lastSpawnedStartTime) && _lastSpawnedStartSegment != null)
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
                
                if(newPosition < oldPosition)
                    segment.Increase();
                else
                    segment.Decrease();
                
                segment.Bar.SetPosition(oldPosition, newPosition);
                _lastSpawnedStartSegment = segment;
                _lastSpawnedStartTime = Time.time;
            }
        }

        private void BarSegment_OnEndPositionChange(float oldPosition, float newPosition)
        {
            if(Mathf.Approximately(oldPosition, newPosition))
                return;
            
            float lenght = Mathf.Abs(newPosition - oldPosition);
            Debug.Log($"Lenght {lenght}; IsCanMarge: {IsCanMarge(lenght, _lastSpawnedEndTime)}");
            if (IsCanMarge(lenght, _lastSpawnedEndTime) && _lastSpawnedEndSegment != null)
            {
                if(newPosition > _lastSpawnedEndSegment.Bar.PositionStart)
                    _lastSpawnedEndSegment.Bar.PositionEnd = newPosition;
                else
                    _lastSpawnedEndSegment.Bar.PositionStart = newPosition;
                _lastSpawnedEndSegment.ResetLifetime();
            }
            else
            {
                var segment = _segments.Get();
                segment.ResetLifetime();
                
                if(newPosition > oldPosition)
                    segment.Increase();
                else
                    segment.Decrease();
                
                segment.Bar.SetPosition(oldPosition, newPosition);
                _lastSpawnedEndSegment = segment;
                _lastSpawnedEndTime = Time.time;
            }
        }

        private bool IsCanMarge(float lenght, float lastSpawnedTime)
        {
            return lenght < _mergeThreshold && Time.time < lastSpawnedTime + _mergeTime;
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