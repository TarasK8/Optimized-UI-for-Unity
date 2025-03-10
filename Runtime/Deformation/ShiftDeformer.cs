using TarasK8.UI.Deformation;
using UnityEngine;

namespace TarasK8.UI
{
    [AddComponentMenu("Optimized UI/Deformation/Shift Deformer")]
    public class ShiftDeformer : BaseDeformer
    {
        [SerializeField, Range(-100f, 100f)] private float _horizontal;
        [SerializeField, Range(-100f, 100f)] private float _vertical;
        
        private Vector2 _upLeftPoint;
        private Vector2 _upRightPoint;
        private Vector2 _downLeftPoint;
        private Vector2 _downRightPoint;

        public override void BeginDeform()
        {
            var rectTransform = (RectTransform)transform;
            var rect = rectTransform.rect;
            
            _upLeftPoint = new Vector2(rect.xMin + _horizontal, rect.yMax + _vertical);
            _upRightPoint = new Vector2(rect.xMax + _horizontal, rect.yMax - _vertical);
            _downLeftPoint = new Vector2(rect.xMin - _horizontal, rect.yMin + _vertical);
            _downRightPoint = new Vector2(rect.xMax - _horizontal, rect.yMin - _vertical);
        }

        public override Vector2 DeformPoint(float xTime, float yTime)
        {
            var up = Vector2.LerpUnclamped(_upLeftPoint, _upRightPoint, xTime);
            var down = Vector2.LerpUnclamped(_downLeftPoint, _downRightPoint, xTime);
            var result = Vector2.LerpUnclamped(down, up, yTime);
            
            return result;
        }
    }
}
