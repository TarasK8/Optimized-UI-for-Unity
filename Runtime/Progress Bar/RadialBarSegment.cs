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

        public float GlobalStartAngle => GetAngleStart();
        public float GlobalEndAngle => GetAngleEnd();

        private void Awake()
        {
            UpdateRequirements();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            UpdateRequirements();

            _image.fillMethod = Image.FillMethod.Radial360;
            _image.fillClockwise = false;
        }

        private void Update()
        {
            if(Application.isPlaying || !_image || !_rectTransform) return;
            if(TryGetComponent<IBarSegmentUpdater>(out _)) return;
            
            UpdateShape();
        }
#endif
        
        public void UpdateShape()
        {
            SetPositionStart(_startPosition);
            SetPositionEnd(_endPosition);
        }

        public void SetAngles(float startAngle, float endAngle)
        {
            _startAngle = startAngle;
            _endAngle = endAngle;
            UpdateShape();
        }

        protected override void SetPositionStart(float position)
        {
            var a = AngleToPosition(Length);
            var b = Mathf.Lerp(0f, a, position);
            var rotation = _rectTransform.localEulerAngles;
            var newRotation = new Vector3(rotation.x, rotation.y, PositionToAngle(b) + _startAngle);
            _rectTransform.localEulerAngles = newRotation;
        }

        protected override void SetPositionEnd(float position)
        {
            var a = AngleToPosition(Length);
            var b = Mathf.Lerp(0f, a, position);
            var startPos = GetPositionStart();
            var amount = b - startPos;
            if (amount < 0f)
                amount = b - (1f - startPos); // crutch, to prevent a bug when an element can visually disappear
            _image.fillAmount = Mathf.Abs(amount);
        }

        protected override float GetPositionStart()
        {
            var a = GetAngleStart();
            var b = _startAngle;
            var result = AngleToPosition(a - b);
            return result;
        }

        protected override float GetPositionEnd()
        {
            var a = GetAngleEnd();
            var b = _endAngle;
            return AngleToPosition(a - b);
        }

        private float GetAngleStart()
        {
            return RepeatAngle(_rectTransform.localEulerAngles.z);
        }

        private float GetAngleEnd()
        {
            return _rectTransform.localEulerAngles.z + PositionToAngle(_image.fillAmount);
        }

        [ContextMenu("Set Recommended Image Parameters")]
        private void SetRecommendedImageParameters()
        {
            _image.type = Image.Type.Filled;
            _image.fillMethod = Image.FillMethod.Radial360;
            _image.fillClockwise = false;
        }

        public static float AngleToPosition(float angle)
        {
            float result = RepeatAngle(angle) / Circle;
            return result;
        }

        public static float PositionToAngle(float value)
        {
            return value * Circle;
        }

        public static float RepeatAngle(float angle)
        {
            angle = (float)Math.Round(angle, 3); // To avoid float precision troubles
            var result = Mathf.Repeat(angle, Circle);
            if(Mathf.Approximately(result, Circle)) return 0f;
            return result;
        }

        private void UpdateRequirements()
        {
            if(!_image)
                _image = GetComponent<Image>();
            if(!_rectTransform)
                _rectTransform = GetComponent<RectTransform>();
        }
    }
}