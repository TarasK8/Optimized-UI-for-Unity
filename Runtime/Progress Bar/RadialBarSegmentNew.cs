using System;
using TarasK8.UI.Deformation;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TarasK8.UI
{
    [AddComponentMenu("Optimized UI/Progress Bar/Radial Bar Segment New")]
    [DisallowMultipleComponent]
    [ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(Image))]
    public class RadialBarSegmentNew : BarSegmentBase
    {
        public const float Circle = 360f;

        [SerializeField] private float _startAngle = 0f;
        [SerializeField] private float _endAngle = 359.99f;
        [SerializeField, Range(0f, 1f)] private float _startPosition = 0f;
        [SerializeField, Range(0f, 1f)] private float _endPosition = 1f;
        
        [Header("Visualization")]
        [SerializeField] private VisualizationMethod _visualizationMethod;
        [SerializeField] private ArcDeformer _arcDeformer;
        
        [HideInInspector, SerializeField] private Image _image;
        [HideInInspector, SerializeField] private RectTransform _rectTransform;

        public float Length => MathCircle.RepeatAngle(_endAngle - _startAngle);
        
        public float StartAngle
        {
            get => _startAngle;
            set {_startAngle = value; Rebuild(); }
        }

        public float EndAngle
        {
            get => _endAngle;
            set { _endAngle = value; Rebuild(); }
        }

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
            
            if(_endPosition < _startPosition)

            _image.fillMethod = Image.FillMethod.Radial360;
            _image.fillClockwise = false;
        }

        private void Update()
        {
            if(Application.isPlaying || !_image || !_rectTransform) return;
            if(TryGetComponent<IBarSegmentUpdater>(out _)) return;
            
            Rebuild();
        }
#endif

        public void SetStartEndAngles(float startAngle, float endAngle)
        {
            _startAngle = startAngle;
            _endAngle = endAngle;
            Rebuild();
        }

        public void Rebuild()
        {
            switch (_visualizationMethod)
            {
                case VisualizationMethod.ImageFill:
                {
                    ImageFillRebuild();
                    break;
                }
                case VisualizationMethod.ArcDeformer:
                {
                    ArcDeformerRebuild();
                    return;
                }
                default:
                    return;
            }
        }

        private void ImageFillRebuild()
        {
            var lengthRatio = MathCircle.AngleToRatio(Length);
            var startRatio = Mathf.Lerp(0f, lengthRatio, Mathf.Clamp(_startPosition, 0f, _endPosition));
            var rotation = _rectTransform.localEulerAngles;
            var newRotation = new Vector3(rotation.x, rotation.y, MathCircle.RatioToAngle(startRatio) + _startAngle);
            _rectTransform.localEulerAngles = newRotation;
            
            var endRatio = Mathf.Lerp(0f, lengthRatio, Mathf.Clamp(_endPosition, _startPosition, 1f));
            var amount = endRatio - startRatio;
            if (amount < 0f)
                amount = endRatio - (1f - startRatio); // crutch, to prevent a bug when an element can visually disappear
            _image.fillAmount = Mathf.Abs(amount);
        }

        private void ArcDeformerRebuild()
        {
            
        }

        protected override void SetPositionStart(float position)
        {
            _startPosition = position;
            Rebuild();
        }

        protected override void SetPositionEnd(float position)
        {
            _endPosition = position;
            Rebuild();
        }

        protected override float GetPositionStart() => _startPosition;
        protected override float GetPositionEnd() => _endPosition;

        private float GetAngleStart()
        {
            return MathCircle.RepeatAngle(_rectTransform.localEulerAngles.z);
        }

        private float GetAngleEnd()
        {
            return GetAngleStart() + MathCircle.RatioToAngle(_image.fillAmount);
        }

        private void UpdateRequirements()
        {
            if(!_image)
                _image = GetComponent<Image>();
            if(!_rectTransform)
                _rectTransform = GetComponent<RectTransform>();
        }

        [ContextMenu("Set Recommended Image Parameters")]
        private void SetRecommendedImageParameters()
        {
            _image.type = Image.Type.Filled;
            _image.fillMethod = Image.FillMethod.Radial360;
            _image.fillClockwise = false;
        }

        public enum VisualizationMethod
        {
            None,
            ImageFill,
            ArcDeformer
        }
    }
}