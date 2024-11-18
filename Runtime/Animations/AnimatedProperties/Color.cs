using UnityEngine;
using UnityEngine.UI;

namespace TarasK8.UI.Animations.AnimatedProperties
{
    [System.Serializable]
    [TransitionMenuName("Image/Color")]
    public class Color : AnimatedProperty<Color.Data>
    {
        [SerializeField] private float _duration = 0.2f;
        [SerializeField] private float _delay = 0f;
        [SerializeField] public Easing _easing;
        [SerializeField] private Graphic _targetGraphic;

        private Data _data;
        private UnityEngine.Color _current;

        public override float Delay { get => _delay; protected set => _delay = value; }
        public override float Duration { get => _duration; protected set => _duration = value; }

        public override void Start(Data data)
        {
            _data = data;
            _current = _targetGraphic.color;
        }

        public override void Process(float t)
        {
            float lerp = _easing.Evaluate(t);
            _targetGraphic.color = UnityEngine.Color.LerpUnclamped(_current, _data.Color, lerp);
        }

        [System.Serializable]
        public class Data : IAnimationData
        {
            [field: SerializeField] public string Name { get; set; }
            [SerializeField] public UnityEngine.Color Color = UnityEngine.Color.white;
        }
    }
}