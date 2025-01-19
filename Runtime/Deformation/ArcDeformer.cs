using System;
using UnityEngine;

namespace TarasK8.UI.Deformation
{
    public class ArcDeformer : MonoBehaviour, IDeformer
    {
        [SerializeField, Range(0f, 360f)] private float _lenght = 360f;
        [SerializeField] private float _radius = 100f;
        [SerializeField] private float _thickness = 50f;
        [SerializeField] private float _fLenght;

        private void OnValidate()
        {
            _fLenght = CalculateArcLenght();
        }

        public Vector3 GetPoint(float xTime, float yTime)
        {
            float angle = Mathf.Lerp(0f, _lenght, xTime);
            var up = CalculatePoint(angle, _radius + _thickness);
            var down = CalculatePoint(angle, _radius);
            var result = Vector3.LerpUnclamped(up, down, yTime);
            return result;
        }

        private Vector3 CalculatePoint(float angle, float radius)
        {
            float x = radius * Mathf.Cos(angle / Mathf.Rad2Deg);
            float y = radius * Mathf.Sin(angle / Mathf.Rad2Deg);
            float z = 0f;
            return new Vector3(x, y, z);
        }

        private float CalculateArcLenght()
        {
            float r = _radius + _thickness;
            float circleLenght = 2f * Mathf.PI * r;
            float arcLenght = circleLenght * (_lenght / 360f);
            return arcLenght;
        }
    }
}
