using UnityEngine;

namespace TarasK8.UI.Layout
{
    public interface IFlexElement
    {
        public float Grow { get; }
        public float Shrink  { get; }
        public float Basis  { get; }
        public float MinSize  { get; }
        public float MaxSize  { get; }
    }
}
