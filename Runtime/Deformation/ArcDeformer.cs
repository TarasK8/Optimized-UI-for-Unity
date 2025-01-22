using UnityEngine;

namespace TarasK8.UI.Deformation
{
    [AddComponentMenu("Optimized UI/Deformation/Arc Deformer")]
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    [ExecuteAlways]
    public class ArcDeformer : BaseDeformer
    {
        [SerializeField, Min(0f)] private float _radius = 100f;
        [SerializeField, Min(0f)] private float _thickness = 50f;
        [Space]
        [SerializeField] private float _startAngle = 90f;
        [SerializeField, Range(0f, 360f)] private float _lenght = 180f;
        [SerializeField, Range(0f, 1f)] private float _lengthCalculationPivot = 0.5f;

        [Header("Transform Handling")] 
        [SerializeField] private bool _widthByLenght = true;
        [SerializeField] private bool _heightByThickness = true;
        [Space]
        [SerializeField] private bool _positionToCenter = false;
        [SerializeField] private bool _rotationToCenter = false;

        private float StartAngle
        {
            get
            {
                if(_rotationToCenter)
                    return _lenght / 2f - _lenght + _startAngle;
                return _startAngle;
            }
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
            if(_positionToCenter)
                result -= CalculateCenter();
            return result;
        }

        private Vector2 CalculateAnglePoint(float angle, float radius)
        {
            float x = radius * Mathf.Cos(angle / Mathf.Rad2Deg);
            float y = radius * Mathf.Sin(angle / Mathf.Rad2Deg);
            return new Vector2(x, y);
        }

        private Vector2 CalculateCenter()
        {
            var angle = Mathf.Lerp(StartAngle, StartAngle + _lenght, 0.5f);
            var radius = Mathf.Lerp(_radius, _radius + _thickness, 0.5f);
            return CalculateAnglePoint(angle, radius);
        }

        private float CalculateArcLenght()
        {
            float r = Mathf.Lerp(_radius, _radius + _thickness, _lengthCalculationPivot);
            float circleLenght = 2f * Mathf.PI * r;
            float arcLenght = circleLenght * (_lenght / 360f);
            return arcLenght;
        }

        private void UpdateTransform()
        {
            if(!_widthByLenght && !_heightByThickness)
                return;
            
            var trans = (RectTransform)transform;
            var currentSize = trans.sizeDelta;
            
            float x = _widthByLenght ? CalculateArcLenght() : currentSize.x;
            float y = _heightByThickness ? _thickness : currentSize.y;
            Vector2 size = new Vector2(x, y);
            
            trans.sizeDelta = size;
        }
    }
}
