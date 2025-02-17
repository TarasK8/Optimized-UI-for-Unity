using System;
using UnityEngine;

namespace TarasK8.UI.Animations.AnimatedProperties
{
    [Serializable]
    [TransitionMenuName("Transform/Position")]
    public class Position : AnimatedProperty<Position.Data>
    {
        [field: SerializeField] public override float Delay { get; protected set; }
        [field: SerializeField] public override float Duration { get; protected set; }
        [SerializeField] public Easing _easing;
        [SerializeField] private Transform _targetTransform;

        private Vector3 _current;
        private Data _data;

        public override void Start(Data data)
        {
            _data = data;
            _current = _targetTransform.position;
        }

        public override void Process(float t)
        {
            float lerp = _easing.Evaluate(t);
            _targetTransform.position = Vector2.LerpUnclamped(_current, _data.Position, lerp);
        }

        [Serializable]
        public class Data : IAnimationData
        {
            [SerializeField] public Vector3 Position = Vector3.one;
        }
    }
}