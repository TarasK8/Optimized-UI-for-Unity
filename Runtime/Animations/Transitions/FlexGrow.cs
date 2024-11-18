using System;
using TarasK8.UI.Layout;
using UnityEngine;

namespace TarasK8.UI.Animations.Transitions
{
    [Serializable]
    [TransitionMenuName("Flex/Grow")]
    public class FlexGrow : AnimatedProperty<FlexGrow.Data>
    {
        [field: SerializeField] public override float Duration { get; protected set; }
        [field: SerializeField] public override float Delay { get; protected set; }
        [SerializeField] public Easing _easing;
        [SerializeField] private FlexLayoutElement _targetElement;

        private Data _data;
        private float _currentGrow;

        public override void Start(Data data)
        {
            _data = data;
            _currentGrow = _targetElement.Grow;
        }

        public override void Process(float t)
        {
            float lerp = _easing.Evaluate(t);
            _targetElement.Grow = Mathf.LerpUnclamped(_currentGrow, _data.Grow, lerp);
        }

        [Serializable]
        public class Data : IAnimationData
        {
            [field: SerializeField] public string Name { get; set; }
            [SerializeField] public float Grow = 1f;
        }
    }
}