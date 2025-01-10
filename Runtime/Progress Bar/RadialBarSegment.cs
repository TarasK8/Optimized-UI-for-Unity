using System;
using UnityEngine;
using UnityEngine.UI;

namespace TarasK8.UI
{
    [AddComponentMenu("Optimized UI/Progress Bar/Radial Bar Segment")]
    [DisallowMultipleComponent]
    [ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(Image))]
    public class RadialBarSegment : BarSegmentBase
    {
        public const float Circle = 360f;

        [SerializeField] private float _startAngle = 0f;
        [SerializeField] private float _endAngle = 359.99f;
        [SerializeField, Range(0f, 1f)] private float _startPosition = 0f;
        [SerializeField, Range(0f, 1f)] private float _endPosition = 1f;
        
        [HideInInspector, SerializeField] private Image _image;
        [HideInInspector, SerializeField] private RectTransform _rectTransform;

        public float Length => RepeatAngle(_endAngle - _startAngle);

        private void OnValidate()
        {
            if(!_image)
                _image = GetComponent<Image>();
            if(!_rectTransform)
                _rectTransform = GetComponent<RectTransform>();
        }

#if UNITY_EDITOR
        private void Update()
        {
            if(Application.isPlaying || !_image || !_rectTransform) return;
            if(TryGetComponent<IBarSegmentUpdater>(out _)) return;
            
            SetPositionStart(_startPosition);
            SetPositionEnd(_endPosition);
        }
        #endif

        protected override void SetPositionStart(float position)
        {
            var a = AngleToPosition(Length);
            var b = Mathf.Lerp(0f, a, position);
            var rotation = _rectTransform.eulerAngles;
            var newRotation = new Vector3(rotation.x, rotation.y, PositionToAngle(b) + _startAngle);
            _rectTransform.eulerAngles = newRotation;
        }

        protected override void SetPositionEnd(float position)
        {
            var a = AngleToPosition(Length);
            var b = Mathf.Lerp(0f, a, position);
            var amount = b - GetPositionStart();
            _image.fillAmount = amount;
        }

        protected override float GetPositionStart()
        {
            var a = GetAngleStart();
            var b = _startAngle;
            return AngleToPosition(a - b);
        }

        protected override float GetPositionEnd()
        {
            var a = GetAngleEnd();
            var b = _endAngle;
            return AngleToPosition(a - b);
        }

        private float GetAngleStart()
        {
            return RepeatAngle(_rectTransform.eulerAngles.z);
        }

        private float GetAngleEnd()
        {
            return _rectTransform.eulerAngles.z + PositionToAngle(_image.fillAmount);
        }

        private float AngleToPosition(float angle)
        {
            float result = RepeatAngle(angle) / Circle;
            return result;
        }

        private float PositionToAngle(float value)
        {
            return value * Circle;
        }

        private float RepeatAngle(float angle)
        {
            angle = (float)Math.Round(angle, 3); // To avoid float precision troubles
            var result = Mathf.Repeat(angle, Circle);
            if(Mathf.Approximately(result, Circle)) return 0f;
            return result;
        }
    }
}