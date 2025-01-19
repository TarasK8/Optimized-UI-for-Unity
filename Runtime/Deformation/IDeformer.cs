using UnityEngine;

namespace TarasK8.UI.Deformation
{
    public interface IDeformer
    {
        public Vector3 GetPoint(float xTime, float yTime);
    }
}
