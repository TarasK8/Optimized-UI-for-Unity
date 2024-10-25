using System;
using UnityEngine;

namespace TarasK8.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class BarSegment : MonoBehaviour
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

        public event Action<float, float> OnStartPositionChange;
        public event Action<float, float> OnEndPositionChange;
        public (float start, float end) Position
        {
            get => (GetPositionStart(), GetPositionEnd());
            set => SetPosition(value.start, value.end);
        }
        public float PositionStart
        {
            get => GetPositionStart();
            set => SetPositionStartWithEvent(value);
        }
        public float PositionEnd
        {
            get => GetPositionEnd();
            set => SetPositionEndWithEvent(value);
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

        public void SetPosition(float start, float end)
        {
            if (start > end)
            {
                (start, end) = (end, start);
            }

            SetPositionStartWithEvent(start);
            SetPositionEndWithEvent(end);
        }

        private void SetPositionStartWithEvent(float position)
        {
            float oldPosition = PositionStart;
            SetPositionStart(position);
            OnStartPositionChange?.Invoke(oldPosition, position);
        }

        private void SetPositionEndWithEvent(float position)
        {
            float oldPosition = PositionEnd;
            SetPositionEnd(position);
            OnEndPositionChange?.Invoke(oldPosition, position);
        }

        protected virtual void SetPositionStart(float position)
        {
            RectTransform.anchorMin = _axis switch
            {
                Axis.Horizontal => new Vector2(position, 0f),
                Axis.Vertical => new Vector2(0f, position),
                _ => throw new NotImplementedException()
            };
        }

        protected virtual void SetPositionEnd(float position)
        {
            RectTransform.anchorMax = _axis switch
            {
                Axis.Horizontal => new Vector2(position, 1f),
                Axis.Vertical => new Vector2(1f, position),
                _ => throw new NotImplementedException()
            };
        }

        protected virtual float GetPositionStart() => _axis switch
        {
            Axis.Horizontal => RectTransform.anchorMin.x,
            Axis.Vertical => RectTransform.anchorMin.y,
            _ => throw new NotImplementedException()
        };

        protected virtual float GetPositionEnd() => _axis switch
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