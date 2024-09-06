using System;
using UnityEngine;

namespace TarasK8.UI.Animations.Transitions
{
    [Serializable]
    [TransitionMenuName("Rect Transform/Size Delta")]
    public class SizeDelta : Transition<SizeDelta.Data>
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
            _current = _targetTransform.sizeDelta;
        }

        public override void Process(float t)
        {
            float lerp = _easing.Evaluate(t);
            _targetTransform.sizeDelta = Vector2.LerpUnclamped(_current, _data.Size, lerp);
        }

        [Serializable]
        public class Data : IAnimationData
        {
            [field: SerializeField] public string Name { get; set; }
            [SerializeField] public Vector2 Size = Vector3.one;
        }
    }
}