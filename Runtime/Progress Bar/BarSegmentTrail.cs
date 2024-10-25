using System;
using UnityEngine;
using UnityEngine.UI;

namespace TarasK8.UI
{
    [AddComponentMenu("Optimized UI/Progress Bar/Bar Segment Trail")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BarSegment))]
    public class BarSegmentTrail : MonoBehaviour
    {
        [SerializeField] private BarSegment _trackingBar;
        
        [Header("Color")]
        [SerializeField] private Image _targetImage;
        [SerializeField] private Color _increaseColor = new(0.2663314f, 0.8962264f, 0.5550476f);
        [SerializeField] private Color _decreaseColor = new(1f, 0.2705882f, 0.2705882f);
        
        private BarSegment _bar;

        public BarSegment Bar
        {
            get
            {
                if(_bar == null)
                    _bar = GetComponent<BarSegment>();
                return _bar;
            }
        }

        private void OnValidate()
        {
            if (_trackingBar == Bar)
            {
                _trackingBar = null;
                Debug.LogWarning("Cannot track the self bar");
            }
        }
        
        private void OnEnable()
        {
            _trackingBar.OnStartPositionChange += BarSegment_OnStartPositionChange;
            _trackingBar.OnEndPositionChange += BarSegment_OnEndPositionChange;
            Bar.Position = _trackingBar.Position;
        }

        private void OnDisable()
        {
            _trackingBar.OnStartPositionChange -= BarSegment_OnStartPositionChange;
            _trackingBar.OnEndPositionChange -= BarSegment_OnEndPositionChange;
        }

        private void BarSegment_OnStartPositionChange(float oldPosition, float newPosition)
        {
            if(Mathf.Approximately(oldPosition, newPosition))
                return;
            
            UpdateColor(newPosition, oldPosition);
            
            Bar.PositionStart = newPosition;
        }

        private void BarSegment_OnEndPositionChange(float oldPosition, float newPosition)
        {
            if(Mathf.Approximately(oldPosition, newPosition))
                return;
            
            UpdateColor(oldPosition, newPosition);
            
            Bar.PositionEnd = newPosition;
        }
        
        private void UpdateColor(float oldValue, float newValue)
        {
            if (_targetImage != null)
            {
                if (newValue > oldValue)
                {
                    _targetImage.color = _increaseColor;
                }
                else if (newValue < oldValue)
                {
                    _targetImage.color = _decreaseColor;
                }
            }
        }
    }
}