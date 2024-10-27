using System;
using UnityEngine;

namespace TarasK8.UI
{
    [AddComponentMenu("Optimized UI/Progress Bar/Progress Bar")]
    [ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform))]
    public class ProgressBar : ProgressBarBase
    {
        [SerializeField, HideInInspector] private RectTransform _rectTransform;
        
        [SerializeField] private BarSegment _bar;
        [SerializeField] private Direction _direction;
        [SerializeField, Range(0f, 1f)] private float _center = 0.5f;
        
        [SerializeField] private ValueMode _minValueMode = ValueMode.Custom;
        [SerializeField] private ValueMode _maxValueMode = ValueMode.Custom;

        private RectTransform RectTransform
        {
            get
            {
                if(_rectTransform == null)
                    _rectTransform = (RectTransform)transform;
                return _rectTransform;
            }
        }

        public override float MinValue => GetValueByMode(_minValueMode, base.MinValue);
        public override float MaxValue => GetValueByMode(_maxValueMode, base.MaxValue);
        
        public float Center
        {
            get => _center;
            set
            {
                _center = Mathf.Clamp01(value);
                UpdateVisualProgress(Value);
            }
        }
        public Direction direction
        {
            get => _direction;
            set
            {
                _direction = value;
                UpdateVisualProgress(Value);
            }
        }

#if UNITY_EDITOR
        private void Update()
        {
            if(Application.isPlaying == false && _bar != null)
                UpdateVisualProgress(Value);
        }
#endif

        protected override void OnSetValue(float oldValue, float newValue)
        {
            UpdateVisualProgress(newValue);
        }

        public float CalculateLeftCenter(float position)
        {
            return (-position + 1f) * _center;
        }

        public float CalculateRightCenter(float position)
        {
            return position * (1f - _center) + _center;
        }

        public void UpdateVisualProgress(float value)
        {
            var position = CalculatePosition(value);

            switch (_direction)
            {
                case Direction.Right:
                {
                    _bar.SetPosition(0f, position);
                    break;
                }
                case Direction.Left:
                {
                    _bar.SetPosition(-position + 1f, 1f);
                    break;
                }
                case Direction.Center:
                {
                    float left = CalculateLeftCenter(position);
                    float right = CalculateRightCenter(position);
                    _bar.SetPosition(left, right);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private float GetValueByMode(ValueMode mode, float customValue)
        {
            return mode switch
            {
                ValueMode.Custom => customValue,
                ValueMode.Width => RectTransform.rect.width,
                ValueMode.Height => RectTransform.rect.height,
                ValueMode.HeightOrWidth => GetHeightOrWidthOption(),
                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        private float GetHeightOrWidthOption()
        {
            return _bar.axis switch
            {
                BarSegment.Axis.Horizontal => RectTransform.rect.width,
                BarSegment.Axis.Vertical => RectTransform.rect.height,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        protected override bool ShouldFilterSameValues()
        {
            return true;
        }

        public enum Direction
        {
            Right,
            Left,
            Center,
        }

        private enum ValueMode
        {
            Custom,
            Width,
            Height,
            HeightOrWidth,
        }
    }
}