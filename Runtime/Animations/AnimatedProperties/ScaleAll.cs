using System;
using UnityEngine;

namespace TarasK8.UI.Animations.AnimatedProperties
{
    [Serializable]
    [TransitionMenuName("Transform/Scale All")]
    public class ScaleAll : AnimatedProperty<ScaleAll.Data>
    {
        [field: SerializeField] public override float Duration { get; protected set; }
        [field: SerializeField] public override float Delay { get; protected set; }
        [SerializeField] private Easing _easing;
        [SerializeField] private Transform _targetTransform;

        private Data _data;
        private float _current;

        public override void Start(Data data)
        {
            _data = data;
            _current = _targetTransform.localScale.x;
        }

        public override void Process(float t)
        {
            float lerp = _easing.Evaluate(t);
            float scale = Mathf.LerpUnclamped(_current, _data.Scale, lerp);
            _targetTransform.localScale = new Vector3(scale, scale, scale);
        }

        [Serializable]
        public class Data : IAnimationData
        {
            [field: SerializeField] public string Name { get; set; }
            [SerializeField] public float Scale = 1f;
        }
    }
}