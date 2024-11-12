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
        
        [SerializeField] private BarSegment _fillBar;
        [SerializeField] private Direction _direction;
        [SerializeField, Range(0f, 1f)] private float _center = 0.5f;
        
        [SerializeField] private ValueMode _minValueMode = ValueMode.Custom;
        [SerializeField] private ValueMode _maxValueMode = ValueMode.Custom;

        public RectTransform RectTransform
        {
            get
            {
                if(_rectTransform == null)
                    _rectTransform = (RectTransform)transform;
                return _rectTransform;
            }
        }

        public BarSegment FillBar
        {
            get => _fillBar;
            set
            {
                _fillBar = value;
                if(_fillBar != null)
                    UpdateVisualProgress(Value);
            }
        }
        public override float MinValue
        {
            get => GetValueByMode(_minValueMode, base.MinValue);
            set => SetValueByMode(_minValueMode, value, (ctx) => base.MinValue = ctx);
        }
        public override float MaxValue
        {
            get => GetValueByMode(_maxValueMode, base.MaxValue);
            set => SetValueByMode(_maxValueMode, value, (ctx) => base.MaxValue = ctx);
        }

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
            if(Application.isPlaying == false && _fillBar != null)
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
#if UNITY_EDITOR
            if (_fillBar == null)
            {
                Debug.LogException(new NullReferenceException("Fill Bar is null"), this);
                return;
            }
#endif
            
            var position = CalculatePosition(value);

            switch (_direction)
            {
                case Direction.Right:
                {
                    _fillBar.SetPosition(0f, position);
                    break;
                }
                case Direction.Left:
                {
                    _fillBar.SetPosition(-position + 1f, 1f);
                    break;
                }
                case Direction.Center:
                {
                    float left = CalculateLeftCenter(position);
                    float right = CalculateRightCenter(position);
                    _fillBar.SetPosition(left, right);
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

        private void SetValueByMode(ValueMode mode, float value, Action<float> customValue)
        {
            switch (mode)
            {
                case ValueMode.Custom:
                {
                    customValue.Invoke(value);
                    base.MaxValue = value;
                    UpdateValue();
                    break;
                }
                case ValueMode.Width:
                {
                    var size = new Vector2(value, RectTransform.sizeDelta.y);
                    RectTransform.sizeDelta = size;
                    break;
                }
                case ValueMode.Height:
                {
                    var size = new Vector2(RectTransform.sizeDelta.x, value);
                    RectTransform.sizeDelta = size;
                    break;
                }
                case ValueMode.HeightOrWidth:
                {
                    SetHeightOrWidthOption(value);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }

        private float GetHeightOrWidthOption()
        {
            return _fillBar.axis switch
            {
                BarSegment.Axis.Horizontal => RectTransform.rect.width,
                BarSegment.Axis.Vertical => RectTransform.rect.height,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private void SetHeightOrWidthOption(float value)
        {
            var size = _fillBar.axis switch
            {
                BarSegment.Axis.Horizontal => new Vector2(value, RectTransform.sizeDelta.y),
                BarSegment.Axis.Vertical => new Vector2(RectTransform.sizeDelta.x, value),
                _ => throw new ArgumentOutOfRangeException()
            };
            RectTransform.sizeDelta = size;
            base.UpdateValue();
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