using System;
using UnityEngine;

namespace TarasK8.UI
{
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
            get => GetPosition();
            set => SetPosition(value.start, value.end);
        }
        public float PositionStart
        {
            get => GetPositionStart();
            set => SetPositionStart(value);
        }
        public float PositionEnd
        {
            get => GetPositionEnd();
            set => SetPositionEnd(value);
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

            SetPositionStart(start);
            SetPositionEnd(end);
        }

        protected virtual void SetPositionStart(float position)
        {
            float oldPosition = PositionStart;

            RectTransform.anchorMin = _axis switch
            {
                Axis.Horizontal => new Vector2(position, 0f),
                Axis.Vertical => new Vector2(0f, position),
                _ => throw new NotImplementedException()
            };

            OnStartPositionChange?.Invoke(oldPosition, position);
        }

        protected virtual void SetPositionEnd(float position)
        {
            float oldPosition = PositionEnd;

            RectTransform.anchorMax = _axis switch
            {
                Axis.Horizontal => new Vector2(position, 1f),
                Axis.Vertical => new Vector2(1f, position),
                _ => throw new NotImplementedException()
            };

            OnEndPositionChange?.Invoke(oldPosition, position);
        }

        private (float start, float end) GetPosition()
        {
            return (GetPositionStart(), GetPositionEnd());
        }

        private float GetPositionStart() => _axis switch
        {
            Axis.Horizontal => RectTransform.anchorMin.x,
            Axis.Vertical => RectTransform.anchorMin.y,
            _ => throw new NotImplementedException()
        };

        private float GetPositionEnd() => _axis switch
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