using System;
using UnityEngine;

namespace TarasK8.UI
{
    [AddComponentMenu("Optimized UI/Progress Bar/Bar Segment")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class BarSegment : BarSegmentBase
    {
        [SerializeField, HideInInspector] private RectTransform _rectTransform;
        [SerializeField] private Axis _axis;

        public Axis axis
        {
            get
            {
                return _axis;
            }
            set
            {
                var temp = Position;
                _axis = value;
                Position = temp;
            }
        }

        private RectTransform RectTransform
        {
            get
            {
                if (_rectTransform == null)
                    _rectTransform = (RectTransform)transform;
                return _rectTransform;
            }
        }

        protected override void SetPositionStart(float position)
        {
            RectTransform.anchorMin = _axis switch
            {
                Axis.Horizontal => new Vector2(position, 0f),
                Axis.Vertical => new Vector2(0f, position),
                _ => throw new NotImplementedException()
            };
        }

        protected override void SetPositionEnd(float position)
        {
            RectTransform.anchorMax = _axis switch
            {
                Axis.Horizontal => new Vector2(position, 1f),
                Axis.Vertical => new Vector2(1f, position),
                _ => throw new NotImplementedException()
            };
        }

        protected override float GetPositionStart() => _axis switch
        {
            Axis.Horizontal => RectTransform.anchorMin.x,
            Axis.Vertical => RectTransform.anchorMin.y,
            _ => throw new NotImplementedException()
        };

        protected override float GetPositionEnd() => _axis switch
        {
            Axis.Horizontal => RectTransform.anchorMax.x,
            Axis.Vertical => RectTransform.anchorMax.y,
            _ => throw new NotImplementedException()
        };

        public enum Axis
        {
            Horizontal = 0,
            Vertical = 1
        }
    }
}