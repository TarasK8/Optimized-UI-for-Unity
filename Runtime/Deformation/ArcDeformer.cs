using UnityEngine;

namespace TarasK8.UI.Deformation
{
    [AddComponentMenu("Optimized UI/Deformation/Arc Deformer")]
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    [ExecuteAlways]
    public class ArcDeformer : BaseDeformer
    {
        private const float RotationOffset = 90f;
        
        [SerializeField, Min(0f)] private float _radius = 100f;
        [SerializeField, Min(0f)] private float _thickness = 50f;
        [Space]
        [SerializeField] private float _startAngle = 0f;
        [SerializeField, Range(0f, 360f)] private float _lenght = 180f;
        [SerializeField, Range(0f, 1f)] private float _lengthCalculationPivot = 0.5f;

        [Header("Transform Handling")] 
        [SerializeField] private bool _widthByLenght = true;
        [SerializeField] private bool _heightByThickness = true;
        [Space]
        [SerializeField] private bool _positionToCenter = false;
        [SerializeField] private bool _rotationToCenter = false;

        public float StartAngle
        {
            get
            {
                if(RotationToCenter)
                    return CalculateRotationToCenter() + _startAngle + RotationOffset;
                return _startAngle + RotationOffset;
            }
            set => _startAngle = value;
        }

        public float Lenght
        {
            get => _lenght;
            set { _lenght = value; AttachedDeformableGraphic.Rebuild(); }
        }

        public bool WidthByLenght
        {
            get => _widthByLenght;
            set => _widthByLenght = value;
        }

        public bool HeightByThickness
        {
            get => _heightByThickness;
            set => _heightByThickness = value;
        }

        public bool PositionToCenter
        {
            get => _positionToCenter;
            set => _positionToCenter = value;
        }

        public bool RotationToCenter
        {
            get => _rotationToCenter;
            set => _rotationToCenter = value;
        }


#if UNITY_EDITOR
        private bool _editorUpdate = true;

        protected override void OnValidate()
        {
            base.OnValidate();
            _editorUpdate = true;
        }

        private void Update()
        {
            if (_editorUpdate)
            {
                UpdateTransform();
                _editorUpdate = false;
            }
        }
#endif
        
        private void OnEnable()
        {
            UpdateTransform();
        }

        private void OnDisable()
        {
            UpdateTransform();
        }

        public override Vector2 DeformPoint(float xTime, float yTime)
        {
            float angle = Mathf.LerpUnclamped(0f, _lenght, xTime) + StartAngle;
            var up = CalculateAnglePoint(angle, _radius + _thickness);
            var down = CalculateAnglePoint(angle, _radius);
            var result = Vector2.LerpUnclamped(up, down, yTime);
            if(PositionToCenter)
                result -= CalculateLocalCenter();
            return result;
        }

        public Vector2 CalculateCenter(float rotation)
        {
            
            var angle = Mathf.Lerp(rotation + RotationOffset, rotation + RotationOffset + _lenght, 0.5f);
            var radius = Mathf.Lerp(_radius, _radius + _thickness, 0.5f);
            return CalculateAnglePoint(angle, radius);
        }

        private Vector2 CalculateLocalCenter()
        {
            var angle = Mathf.Lerp(StartAngle, StartAngle + _lenght, 0.5f);
            var radius = Mathf.Lerp(_radius, _radius + _thickness, 0.5f);
            return CalculateAnglePoint(angle, radius);
        }

        public float CalculateRotationToCenter()
        {
            return _lenght / 2f - _lenght;
        }

        public void UpdateTransform()
        {
            if(!WidthByLenght && !HeightByThickness)
                return;
            
            var trans = (RectTransform)transform;
            var currentSize = trans.sizeDelta;
            
            float x = WidthByLenght ? CalculateArcLenght() : currentSize.x;
            float y = HeightByThickness ? _thickness : currentSize.y;
            Vector2 size = new Vector2(x, y);
            
            trans.sizeDelta = size;
        }

        private Vector2 CalculateAnglePoint(float angle, float radius)
        {
            float x = radius * Mathf.Cos(angle / Mathf.Rad2Deg);
            float y = radius * Mathf.Sin(angle / Mathf.Rad2Deg);
            return new Vector2(x, y);
        }

        private float CalculateArcLenght()
        {
            float r = Mathf.Lerp(_radius, _radius + _thickness, _lengthCalculationPivot);
            float circleLenght = 2f * Mathf.PI * r;
            float arcLenght = circleLenght * (_lenght / 360f);
            return arcLenght;
        }
    }
}
