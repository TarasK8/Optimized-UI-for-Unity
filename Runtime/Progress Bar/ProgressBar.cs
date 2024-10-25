using System;
using UnityEngine;

namespace TarasK8.UI
{
    [AddComponentMenu("Optimized UI/Progress Bar/Progress Bar")]
    [ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform))]
    public class ProgressBar : MonoBehaviour
    {
        [SerializeField, HideInInspector] private RectTransform _rectTransform;
        
        [SerializeField] private BarSegment _bar;
        [SerializeField] private Direction _direction;
        [SerializeField, Range(0f, 1f)] private float _center = 0.5f;

        [SerializeField] private float _minValue = 0f;
        [SerializeField] private ValueMode _minValueMode = ValueMode.Custom;
        [SerializeField] private float _maxValue = 1f;
        [SerializeField] private ValueMode _maxValueMode = ValueMode.Custom;
        [SerializeField] private float _value = 1f;


        public event Action<float, float> OnValueChanged;

        private RectTransform RectTransform
        {
            get
            {
                if(_rectTransform == null)
                    _rectTransform = (RectTransform)transform;
                return _rectTransform;
            }
        }

        public float MinValue => GetValueByMode(_minValueMode, _minValue);
        public float MaxValue => GetValueByMode(_maxValueMode, _maxValue);
        public float Value
        {
            get => _value;
            set => SetValue(value);
        }
        public float Center
        {
            get => _center;
            set
            {
                _center = Mathf.Clamp01(value);
                SetValue(Value);
            }
        }
        public Direction direction
        {
            get => _direction;
            set
            {
                _direction = value;
                SetValue(Value);
            }
        }

#if UNITY_EDITOR
        private void Update()
        {
            if(Application.isPlaying == false && _bar != null)
                UpdateVisualProgress(Value);
        }
#endif

        public void AddValue(float value)
        {
            Value += value;
        }

        public void SetValue(float value)
        {
            value = Mathf.Clamp(value, MinValue, MaxValue);
            float oldValue = _value;
            
            UpdateVisualProgress(value);

            _value = value;
            OnValueChanged?.Invoke(oldValue, value);
        }

        public float CalculateLeftCenter(float position)
        {
            return (-position + 1f) * _center;
        }

        public float CalculateRightCenter(float position)
        {
            return position * (1f - _center) + _center;
        }

        public float CalculatePosition(float value)
        {
            return Mathf.InverseLerp(MinValue, MaxValue, value);
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