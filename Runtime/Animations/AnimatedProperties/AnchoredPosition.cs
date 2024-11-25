using System;
using UnityEngine;

namespace TarasK8.UI.Animations.AnimatedProperties
{
    [Serializable]
    [TransitionMenuName("Rect Transform/Anchored Position")]
    public class AnchoredPosition : AnimatedProperty<AnchoredPosition.Data>
    {
        [field: SerializeField] public override float Delay { get; protected set; }
        [field: SerializeField] public override float Duration { get; protected set; }
        [SerializeField] public Easing _easing;
        [SerializeField] private RectTransform _targetTransform;

        [NonSerialized] private Data _data;
        private Vector2 _current;

        public override void Start(Data data)
        {
            _data = data;
            _current = _targetTransform.anchoredPosition;
        }

        public override void Process(float t)
        {
            float lerp = _easing.Evaluate(t);
            _targetTransform.anchoredPosition = Vector2.LerpUnclamped(_current, _data.Position, lerp);
        }

        [Serializable]
        public class Data : IAnimationData
        {
            [SerializeField] public Vector2 Position = Vector3.one;
        }
    }
}