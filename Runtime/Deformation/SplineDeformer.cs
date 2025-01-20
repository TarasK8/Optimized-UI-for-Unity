#if UNITY_SPLINES
using UnityEngine;
using UnityEngine.Splines;

namespace TarasK8.UI.Deformation
{
    public class SplineDeformer : BaseDeformer
    {
        [SerializeField] private SplineContainer _curves;

        public override Vector2 DeformPoint(float xTime, float yTime)
        {
            var up = _curves[0].EvaluatePosition(xTime);
            var down = _curves[1].EvaluatePosition(xTime);
            var result = Vector3.LerpUnclamped(up, down, yTime);
            return result;
        }
    }
}
#endif
