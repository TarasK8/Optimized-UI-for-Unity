using System;
using UnityEngine;

namespace TarasK8.UI
{
    [AddComponentMenu("Optimized UI/Progress Bar")]
    [ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform))]
    public class ProgressBar : MonoBehaviour
    {
        [SerializeField, HideInInspector] private RectTransform _rectTransform;

        [Header("Visual")]
        [SerializeField] private RectTransform _bar;
        [SerializeField] private Axis _axis;
        [SerializeField] private Direction _direction;
        [SerializeField, Range(0f, 1f)] private float _center = 0.5f;

        [SerializeField] private float _minValue = 0f;
        [SerializeField] private ValueMode _minValueMode = ValueMode.Custom;
        [SerializeField] private float _maxValue = 1f;
        [SerializeField] private ValueMode _maxValueMode = ValueMode.Custom;
        [SerializeField] private float _value = 1f;


        public event Action<float, float> OnValueChanged;
        public float VisualValue { get; private set; }
        public bool IsUpdateVisual { get; set; } = true;
        public RectTransform RectTransform
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
        public Axis axis
        {
            get => _axis;
            set
            {
                _axis = value;
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

        private void Awake()
        {
            VisualValue = Value;
        }

#if UNITY_EDITOR
        private void Update()
        {
            if(_bar != null && IsUpdateVisual)
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

            if(IsUpdateVisual)
                UpdateVisualProgress(value);

            _value = value;
            OnValueChanged?.Invoke(oldValue, value);
        }

        public float CalculateLeftCenter(float position)
        {
            return (-position + 1f) * _center;
        }

        public float CalculatePosition(float value)
        {
            return Mathf.InverseLerp(MinValue, MaxValue, value);
        }

        public float CalculateRightCenter(float position)
        {
            return position * (1f - _center) + _center;
        }

        public void UpdateVisualProgress(float value)
        {
            var position = CalculatePosition(value);
            VisualValue = value;

            switch (_direction)
            {
                case Direction.Right:
                    SetVisualProgress(0f, position, _axis);
                    break;
                case Direction.Left:
                    SetVisualProgress(-position + 1f, 1f, _axis);
                    break;
                case Direction.Center:
                    float left = CalculateLeftCenter(position);
                    float right = CalculateRightCenter(position);
                    SetVisualProgress(left, right, _axis);
                    break;
                default:
                    break;
            }
        }

        private void SetVisualProgress(float start, float end, Axis axis)
        {
            if(axis == Axis.Horizontal)
            {
                _bar.anchorMin = new Vector2(start, 0f);
                _bar.anchorMax = new Vector2(end, 1f);
            }
            else if(axis == Axis.Vertical)
            {
                _bar.anchorMin = new Vector2(0f, start);
                _bar.anchorMax = new Vector2(1f, end);
            }
            else
            {
                throw new NotImplementedException();
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
                _ => throw new NotImplementedException(),
            };
        }

        private float GetHeightOrWidthOption()
        {
            if (_axis == Axis.Horizontal)
                return RectTransform.rect.width;
            else if (_axis == Axis.Vertical)
                return RectTransform.rect.height;
            else
                throw new NotImplementedException();
        }

        public enum Direction
        {
            Right,
            Left,
            Center,
        }

        public enum Axis
        {
            Horizontal = 0,
            Vertical = 1
        }

        public enum ValueMode
        {
            Custom,
            Width,
            Height,
            HeightOrWidth,
        }
    }
}