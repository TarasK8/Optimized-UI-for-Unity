using System;
using UnityEngine;

namespace TarasK8.UI.Animations.AnimatedProperties
{
    [Serializable]
    [TransitionMenuName("Rect Transform/Anchor Min")]
    public class AnchorMin : AnimatedProperty<AnchorMin.Data>
    {
        [field: SerializeField] public override float Delay { get; protected set; }
        [field: SerializeField] public override float Duration { get; protected set; }
        [SerializeField] public Easing _easing;
        [SerializeField] private RectTransform _targetTransform;

        private Vector2 _current;
        private Data _data;

        public override void Start(Data data)
        {
            _data = data;
            _current = _targetTransform.anchorMin;
        }

        public override void Process(float t)
        {
            float lerp = _easing.Evaluate(t);
            _targetTransform.anchorMin = Vector2.LerpUnclamped(_current, _data.AnchorMin, lerp);
        }

        [Serializable]
        public class Data : IAnimationData
        {
            [SerializeField] public Vector2 AnchorMin = Vector3.one;
        }
    }
}