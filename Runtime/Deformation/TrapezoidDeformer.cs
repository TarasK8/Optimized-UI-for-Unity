using UnityEngine;

namespace TarasK8.UI.Deformation
{
    public class TrapezoidDeformer : BaseDeformer
    {
        [SerializeField] private float _horizontal;
        [SerializeField] private float _vertical;
        [SerializeField, Range(0f, 1f)] private float _sizeFactor;

        public override Vector2 DeformPoint(float xTime, float yTime)
        {
            var rectTransform = (RectTransform)transform;
            var rect = rectTransform.rect;
            
            var upLeftPoint = new Vector2(rect.xMin + _horizontal, rect.yMax + _vertical);
            var upRightPoint = new Vector2(rect.xMax - _horizontal, rect.yMax - _vertical);
            var downLeftPoint = new Vector2(rect.xMin - _horizontal, rect.yMin - _vertical);
            var downRightPoint = new Vector2(rect.xMax + _horizontal, rect.yMin + _vertical);
            
            var up = Vector2.LerpUnclamped(upLeftPoint, upRightPoint, xTime);
            var down = Vector2.LerpUnclamped(downLeftPoint, downRightPoint, xTime);
            var result = Vector2.LerpUnclamped(down, up, yTime);
            
            return result;
        }
    }
}
