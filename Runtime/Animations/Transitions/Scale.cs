using System;
using UnityEngine;

namespace TarasK8.UI.Animations.Transitions
{
    [Serializable]
    [TransitionMenuName("Rect Transform/Scale")]
    public class Scale : Transition<Scale.Data>
    {
        // You can add the [field: SerializeField] attribute to avoid adding additional fields
        [field: SerializeField] public override float Duration { get; protected set; }
        [field: SerializeField] public override float Delay { get; protected set; }
        [SerializeField] public Easing _easing;
        [SerializeField] private RectTransform _targetTransform;

        private Vector3 _current;
        private Data _data;

        public override void Start(Data data)
        {
            _data = data;
            _current = _targetTransform.localScale;
        }

        public override void Process(float t)
        {
            float lerp = _easing.Evaluate(t);
            _targetTransform.localScale = Vector3.Lerp(_current, _data.Scale, lerp);
        }

        [Serializable]
        public class Data : IAnimationData
        {
            [field: SerializeField] public string Name { get; set; }
            [SerializeField] public Vector2 Scale = Vector3.one;
        }
    }
}