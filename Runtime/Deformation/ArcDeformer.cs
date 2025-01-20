using System;
using UnityEngine;

namespace TarasK8.UI.Deformation
{
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    [ExecuteAlways]
    public class ArcDeformer : BaseDeformer
    {
        [SerializeField, Range(0f, 360f)] private float _lenght = 360f;
        [SerializeField] private float _startAngle;
        [SerializeField, Min(0f)] private float _radius = 100f;
        [SerializeField, Min(0.001f)] private float _thickness = 50f;
        [SerializeField] private float _fLenght;

#if UNITY_EDITOR
        private bool _editorUpdate = true;

        private void OnValidate()
        {
            _fLenght = CalculateArcLenght();
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
            float angle = Mathf.Lerp(0f, _lenght, xTime) + _startAngle;
            var up = CalculateAnglePoint(angle, _radius + _thickness);
            var down = CalculateAnglePoint(angle, _radius);
            var result = Vector2.LerpUnclamped(up, down, yTime);
            return result;
        }

        private Vector2 CalculateAnglePoint(float angle, float radius)
        {
            float x = radius * Mathf.Cos(angle / Mathf.Rad2Deg);
            float y = radius * Mathf.Sin(angle / Mathf.Rad2Deg);
            return new Vector2(x, y);
        }

        private float CalculateArcLenght()
        {
            float r = _radius + _thickness;
            float circleLenght = 2f * Mathf.PI * r;
            float arcLenght = circleLenght * (_lenght / 360f);
            return arcLenght;
        }

        private void UpdateTransform()
        {
            var trans = (RectTransform)transform;
            Vector2 size = new Vector2(CalculateArcLenght(), _thickness);
            trans.sizeDelta = size;
        }
    }
}
