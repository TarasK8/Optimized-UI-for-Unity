using System;
using UnityEngine;
using UnityEngine.UI;

namespace TarasK8.UI
{
    [ExecuteInEditMode]
    public class RadialBarSegment : BarSegmentBase
    {
        public const float Circle = 360f;
        
        [SerializeField] private float _startAngle;
        [SerializeField] private float _endAngle;
        [SerializeField, Range(0f, 1f)] private float _startPosition;
        [SerializeField, Range(0f, 1f)] private float _endPosition;
        [Header("Dependencies")]
        [SerializeField] private Image _image;
        [SerializeField] private RectTransform _rectTransform;

        public float Length => _endAngle - _startAngle;
        
        private void Update()
        {
            SetPositionStart(_startPosition);
            SetPositionEnd(_endPosition);
        }

        protected override void SetPositionStart(float position)
        {
            var a = AngleToPosition(Length);
            var b = Mathf.Lerp(0f, a, position);
            
            var rotation = _rectTransform.eulerAngles;
            var newRotation = new Vector3(rotation.x, rotation.y, PositionToAngle(b)+ _startAngle);
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
            if(Mathf.Approximately(a, b))
                return 0f;
            return AngleToPosition(a - b);
        }

        protected override float GetPositionEnd()
        {
            var a = GetAngleEnd();
            var b = _endAngle;
            if(Mathf.Approximately(a, b))
                return 0f;
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
            return RepeatAngle(angle) / Circle;
        }

        private float PositionToAngle(float value)
        {
            return value * Circle;
        }

        private float RepeatAngle(float angle)
        {
            return Mathf.Repeat(angle, Circle);
        }
    }
}