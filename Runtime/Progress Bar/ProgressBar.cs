using System;
using UnityEngine;

namespace TarasK8.UI
{
    [AddComponentMenu("Optimized UI/Progress Bar/Progress Bar")]
    [ExecuteInEditMode]
    public class ProgressBar : ProgressBarBase, IBarSegmentUpdater
    {
        [SerializeField] private BarSegmentBase _fillBar;
        [SerializeField] private Direction _direction;
        [SerializeField, Range(0f, 1f)] private float _center = 0.5f;

        public BarSegmentBase FillBar
        {
            get => _fillBar;
            set
            {
                _fillBar = value;
                if(_fillBar != null)
                    UpdateVisualProgress(Value);
            }
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
            if(Application.isPlaying == false && FillBar != null)
                UpdateVisualProgress(Value);
        }
#endif

        protected override void OnSetValue(float oldValue, float newValue)
        {
            UpdateVisualProgress(newValue);
        }

        public float CalculateLeftCenter(float position)
        {
            return (-position + 1f) * Center;
        }

        public float CalculateRightCenter(float position)
        {
            return position * (1f - Center) + Center;
        }

        public void UpdateVisualProgress(float value)
        {
#if UNITY_EDITOR
            if (FillBar == null)
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
                    FillBar.SetPosition(0f, position);
                    break;
                }
                case Direction.Left:
                {
                    FillBar.SetPosition(-position + 1f, 1f);
                    break;
                }
                case Direction.Center:
                {
                    float left = CalculateLeftCenter(position);
                    float right = CalculateRightCenter(position);
                    FillBar.SetPosition(left, right);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
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
    }
}