using System;
using UnityEngine;

namespace TarasK8.UI.Animations.Transitions
{
    [Serializable]
    [TransitionMenuName("Transform/Rotation")]
    public class Rotation : AnimatedProperty<Rotation.Data>
    {
        [field: SerializeField] public override float Delay { get; protected set; }
        [field: SerializeField] public override float Duration { get; protected set; }
        [SerializeField] public Easing _easing;
        [SerializeField] private Transform _targetTransform;

        private Quaternion _current;
        private Data _data;

        public override void Start(Data data)
        {
            _data = data;
            _current = _targetTransform.localRotation;
        }

        public override void Process(float t)
        {
            float lerp = _easing.Evaluate(t);
            _targetTransform.localRotation = Quaternion.LerpUnclamped(_current, _data.Rotation, lerp);
        }

        [Serializable]
        public class Data : IAnimationData
        {
            [field: SerializeField] public string Name { get; set; }
            [SerializeField] public Quaternion Rotation = Quaternion.identity;
        }
    }
}