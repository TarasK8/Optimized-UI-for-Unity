using UnityEngine;

namespace TarasK8.UI.Deformation
{
    public abstract class BaseDeformer : MonoBehaviour
    {
        public abstract Vector2 DeformPoint(float xTime, float yTime);
    }
}
